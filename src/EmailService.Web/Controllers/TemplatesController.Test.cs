using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Web.ViewModels.Templates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers
{
    public partial class TemplatesController
    {
        public async Task<IActionResult> Test(Guid id)
        {
            var template = await _ctx.FindTemplateWithTranslationsAsync(id);
            if (template != null)
            {
                return View(new TestTemplateViewModel
                {
                    TemplateId = template.Id,
                    TemplateName = template.Name,
                    ApplicationId = template.ApplicationId,
                    SampleData = template.SampleData,
                    Translations = template.Translations.Select(t => new SelectListItem
                    {
                        Value = t.Language,
                        Text = t.GetCultureName()
                    })
                });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Test(
            TestTemplateViewModel model,
            [FromServices] EmailSender emailSender)
        {
            if (ModelState.IsValid)
            {
                var success = await emailSender.SendEmailAsync(new EmailMessageParams
                {
                    ApplicationId = model.ApplicationId,
                    TemplateId = model.TemplateId,
                    Culture = model.Language,
                    Data = JObject.Parse(model.SampleData)
                });

                return RedirectToAction(nameof(Details), new { id = model.TemplateId });
            }

            // TODO: reload translations
            return View(model);
        }
    }
}
