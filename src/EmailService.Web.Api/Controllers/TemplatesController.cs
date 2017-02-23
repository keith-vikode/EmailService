using EmailService.Core.Entities;
using EmailService.Core.Templating;
using EmailService.Web.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private ITemplateTransformer _transformer = MustacheTemplateTransformer.Instance;

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
                .Include(t => t.Translations)
                .Where(t => t.ApplicationId == applicationId && t.IsActive)
                .AsNoTracking()
                .ToListAsync();
            
            return Ok(data.Select(d => new TemplateListingViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Subject = d.SubjectTemplate,
                Translations = d.Translations.Select(t => t.Language)
            }));
        }

        [HttpPost("{id}/Transform/{lang?}")]
        public async Task<IActionResult> Transform(
            [FromRoute] Guid id,
            [FromRoute] string lang,
            [FromBody] JToken data)
        {
            var template = await _context.FindTemplateWithTranslationsAsync(id, User.GetApplicationId());
            if (template == null)
            {
                return NotFound(new
                {
                    TemplateId = id,
                    ApplicationId = User.GetApplicationId()
                });
            }

            var culture = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(lang))
            {
                try
                {
                    culture = new CultureInfo(lang);
                }
                catch (CultureNotFoundException ex)
                {
                    ModelState.AddModelError(nameof(lang), ex.Message);
                    return BadRequest(ModelState);
                }
            }

            var email = template.TryGetTranslation(culture);
            var transformed = await _transformer.TransformTemplateAsync(email, data, culture);

            return Ok(transformed);
        }
    }
}
