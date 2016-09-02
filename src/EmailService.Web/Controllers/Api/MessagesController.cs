using EmailService.Core;
using EmailService.Core.Services;
using EmailService.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers.Api
{
    /// <summary>
    /// Provides an API for email messages.
    /// </summary>
    [Route("api/v1/{applicationId}/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IEmailQueueSender _sender;
        private readonly IEmailMessageBlobStore _blobStore;

        private readonly ILogger _logger;

        public MessagesController(
            IEmailQueueSender sender,
            IEmailMessageBlobStore blobStore,
            ILoggerFactory loggerFactory)
        {
            _sender = sender;
            _blobStore = blobStore;
            _logger = loggerFactory.CreateLogger<MessagesController>();
        }

        [HttpGet("{token}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NotFound, "Token does not represent a valid request, or is too old and has been purged from the records")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Malformed token")]
        [SwaggerResponse(HttpStatusCode.OK, "Token match found", typeof(TokenEnquiryResponse))]
        public async Task<IActionResult> GetRequest(Guid applicationId, Guid token)
        {
            // TODO
            await Task.Delay(100);
            return Ok(new TokenEnquiryResponse());
        }

        /// <summary>
        /// Submits a request to send a new email message to the engine.
        /// </summary>
        /// <param name="applicationId">Application identifier</param>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Accepted, "The request has been queued for processing", typeof(PostEmailResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "The request was not valid and could not be queued", typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post(
            [FromRoute] Guid applicationId,
            [FromForm] PostEmailRequest args,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // the token will be used by both the processing engine and the
            // client to track this request from start to finish
            var token = Guid.NewGuid();
            var received = DateTime.UtcNow;

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

                // all done - let the client know that we've accepted their
                // request, and what the tracking token is
                var response = new PostEmailResponse(token);
                Response.StatusCode = (int)HttpStatusCode.Accepted;
                return Ok(response);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private EmailMessageParams BuildMessage(PostEmailRequest args)
        {
            return new EmailMessageParams
            {
                To = new List<string>(args.To),
                CC = new List<string>(args.CC),
                Bcc = new List<string>(args.Bcc),
                LogLevel = args.LogLevel,
                Subject = args.Subject,
                BodyEncoded = EmailMessageParams.EncodeBody(args.Body),
                TemplateId = args.Template,
                Data = JObject.Parse(args.Data ?? "{}")
            };
        }
    }
}
