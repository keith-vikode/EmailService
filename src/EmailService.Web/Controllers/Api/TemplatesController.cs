using EmailService.Core.Entities;
using EmailService.Web.ViewModels.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers.Api
{
    /// <summary>
    /// Provides access to pre-defined templates.
    /// </summary>
    [Route("api/v1/{applicationId}/[controller]")]
    public class TemplatesController : Controller
    {
        private EmailServiceContext _context;

        public TemplatesController(EmailServiceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a list of templates available for the current application.
        /// </summary>
        /// <param name="applicationId">Application identifier</param>
        /// <returns>A list of template key/name pairs.</returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, "Returned list of templates", typeof(IEnumerable<TemplateListingViewModel>))]
        public async Task<IActionResult> Get(Guid applicationId)
        {
            var data = await _context.Templates
                .Where(t => t.ApplicationId == applicationId && t.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return Ok(data.Select(t => new TemplateListingViewModel { Id = t.Id, Name = t.Name }));
        }
    }
}
