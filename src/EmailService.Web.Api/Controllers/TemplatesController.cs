using EmailService.Core.Entities;
using EmailService.Web.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.SwaggerGen.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EmailService.Web.Api.Controllers
{
    /// <summary>
    /// Provides access to pre-defined templates.
    /// </summary>
    [Authorize]
    [Route("v1/[controller]")]
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
        /// <returns>A list of template key/name pairs.</returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, "Returned list of templates", typeof(IEnumerable<TemplateListingViewModel>))]
        public async Task<IActionResult> Get()
        {
            var applicationId = User.GetApplicationId();

            var data = await _context.Templates
                .Where(t => t.ApplicationId == applicationId && t.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var jsonDic = data.ToDictionary(d => d.Id, d => d.Name);
            return Ok(jsonDic);
        }
    }
}
