using EmailService.Core.Entities;
using EmailService.Web.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EmailService.Web.Api.Controllers
{
    /// <summary>
    /// Provides access to the current application.
    /// </summary>
    [Authorize]
    [Route("v1/[controller]")]
    public class ApplicationController : Controller
    {
        private EmailServiceContext _context;

        public ApplicationController(EmailServiceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets information about the currently-authenticated application.
        /// </summary>
        /// <returns>An object providing information about the application.</returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, "Returns application information", typeof(ApplicationInfoResponse))]
        public async Task<IActionResult> Get()
        {
            var applicationId = User.GetApplicationId();

            var app = await _context.FindApplicationAsync(applicationId);
            if (app == null)
            {
                throw new InvalidOperationException("Cannot find the authorised application - this is weird");
            }

            return Ok(new ApplicationInfoResponse
            {
                ApplicationId = app.Id,
                Name = app.Name,
                Description = app.Description
            });
        }
    }
}
