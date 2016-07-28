using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Web.ViewModels.Templates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            [FromServices] IEmailTransportFactory transportFactory,
            [FromServices] ITemplateTransformer templateTransformer)
        {
            if (ModelState.IsValid)
            {
                // TODO: all of this needs to be moved into a testable object
                var template = await _ctx.FindTemplateWithTranslationsAsync(model.TemplateId);
                var app = await _ctx.FindApplicationAsync(model.ApplicationId);
                var transports = await _ctx.GetApplicationTransportsAsync(model.ApplicationId);
                var selectedTransport = transports.FirstOrDefault();
                var transport = transportFactory.CreateTransport(selectedTransport);

                var emailTemplate = new EmailTemplate(template.SubjectTemplate, template.BodyTemplate);
                if (!string.IsNullOrWhiteSpace(model.Language))
                {
                    var translation = template.Translations.FirstOrDefault(t => t.Language == model.Language);
                    if (translation != null)
                    {
                        emailTemplate = new EmailTemplate(translation.SubjectTemplate, translation.BodyTemplate, translation.GetCulture());
                    }
                }

                var transformed = await templateTransformer.TransformTemplateAsync(
                    emailTemplate,
                    JObject.Parse(model.SampleData));

                await transport.SendAsync(new SenderParams
                {
                    To = new List<string> { model.EmailAddress },
                    Subject = transformed.Subject,
                    Body = transformed.Body,
                    SenderName = app.SenderName ?? selectedTransport.SenderName,
                    SenderEmail = app.SenderAddress ?? selectedTransport.SenderAddress
                });

                return RedirectToAction(nameof(Details), new { id = model.TemplateId });
            }

            // TODO: reload translations
            return View(model);
        }

    }
}
