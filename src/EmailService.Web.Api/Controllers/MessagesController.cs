using EmailService.Core;
using EmailService.Core.Services;
using EmailService.Web.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Web.Api.Controllers
{
    /// <summary>
    /// Provides an API for email messages.
    /// </summary>
    [Authorize]
    [Route("v1/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IEmailQueueSender _sender;
        private readonly IEmailQueueBlobStore _blobStore;
        private readonly IEmailLogWriter _logWriter;
        private readonly IEmailLogReader _logReader;

        private readonly ILogger _logger;

        public MessagesController(
            IEmailQueueSender sender,
            IEmailQueueBlobStore blobStore,
            IEmailLogReader logReader,
            IEmailLogWriter logWriter,
            ILoggerFactory loggerFactory)
        {
            _sender = sender;
            _blobStore = blobStore;
            _logReader = logReader;
            _logWriter = logWriter;
            _logger = loggerFactory.CreateLogger<MessagesController>();
        }

        /// <summary>
        /// Checks the status of a queued email request.
        /// </summary>
        /// <param name="token">Encoded token for the request (this is obtained from the result of posting a new request)</param>
        /// <returns>A response describing the current status of the request identified by <paramref name="token"/>.</returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NotFound, "Token does not represent a valid request, or is too old and has been purged from the records")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Malformed token")]
        [SwaggerResponse(HttpStatusCode.OK, "Token match found", typeof(TokenEnquiryResponse))]
        public async Task<IActionResult> GetRequest([FromQuery] string token)
        {
            EmailQueueToken decoded;

            try
            {
                decoded = EmailQueueToken.DecodeString(token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to parse message token {0}: {1}", token, ex.Message);
                return BadRequest();
            }

            var response = new TokenEnquiryResponse();
            response.Submitted = decoded.TimeStamp;

            var entries = await _logReader.GetProcessingLogsAsync(decoded);
            if (entries.Any())
            {
                var latest = entries.OrderBy(e => e.RetryCount).Last();
                response.Status = latest.Status;
                response.LastProcessed = latest.ProcessFinishedUtc;
                response.RetryCount = latest.RetryCount;
                response.ErrorMessage = latest.ErrorMessage;
            }

            return Json(response);
        }

        /// <summary>
        /// Submits a request to send a new email message to the engine.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Accepted, "The request has been queued for processing", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "The request was not valid and could not be queued", typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post(
            [FromForm] PostEmailRequest args,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var applicationId = User.GetApplicationId();

            // the token will be used by both the processing engine and the
            // client to track this request from start to finish
            var token = EmailQueueToken.Create(applicationId);

            _logger.LogInformation("Sending email using token {0}", token);

            if (ModelState.IsValid)
            {
                // create an object that we then store as a BLOB (emails run
                // the risk of being too large to fit in the queue, so BLOB
                // storage is the best option)
                var message = BuildMessage(args);
                message.ApplicationId = applicationId;
                await _blobStore.AddAsync(token, message, cancellationToken);

                // now we can let the back-end processor know that there's a
                // new message that it has to process
                await _sender.SendAsync(token, cancellationToken);

                // log that we queued the message for processing
                await _logWriter.TryLogProcessAttemptAsync(token, 0, ProcessingStatus.Pending, token.TimeStamp, token.TimeStamp, null, cancellationToken);

                // all done - let the client know that we've accepted their
                // request, and what the tracking token is
                Response.StatusCode = (int)HttpStatusCode.Accepted;
                return Json(new
                {
                    Token = token.EncodeString()
                });
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private EmailMessageParams BuildMessage(PostEmailRequest args)
        {
            const string EmptyData = "{}";
            return new EmailMessageParams
            {
                To = args.To,
                CC = args.CC,
                Bcc = args.Bcc,
                LogLevel = args.LogLevel,
                TemplateId = args.Template,
                Subject = args.Subject,
                BodyEncoded = EmailMessageParams.EncodeBody(args.Body),
                Data = JObject.Parse(args.Data ?? EmptyData).ToObject<Dictionary<string, object>>()
            };
        }
    }
}
