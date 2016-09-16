using EmailService.Core.Entities;
using EmailService.Core.Templating;
using EmailService.Web.ViewModels;
using EmailService.Web.ViewModels.Templates;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public partial class TemplatesController : Controller
    {
        private EmailServiceContext _ctx;

        public TemplatesController(EmailServiceContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> Index(Guid? applicationId)
        {
            var model = await IndexViewModel.LoadAsync(_ctx, applicationId);
            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var model = await TemplateDetailsViewModel.LoadAsync(_ctx, id);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        public async Task<IActionResult> Create(Guid? applicationId)
        {
            var model = new CreateTemplateViewModel
            {
                ApplicationId = applicationId,
                SampleData = "{\n\t\"Name\": \"Bob\"\n}",
                BodyTemplate = "<!DOCTYPE html>\n<html>\n<body>\n\t<p>Hello {{Name}}!</p>\n</body>\n</html>"
            };

            var applications = await _ctx.Applications.ToListAsync();
            if (!applications.Any())
            {
                return View("NoApplications");
            }

            model.Applications = new SelectList(applications, nameof(Application.Id), nameof(Application.Name));

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = model.CreateDbModel();
                _ctx.Templates.Add(entity);
                await _ctx.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var applications = await _ctx.Applications.ToListAsync();
            model.Applications = new SelectList(applications, nameof(Application.Id), nameof(Application.Name));

            return View(model);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            await Task.Delay(200);
            return View();
        }
        
        public async Task<IActionResult> Copy(Guid id)
        {
            var template = await _ctx.FindTemplateAsync(id);
            if (template != null)
            {
                return View(new CopyTemplateViewModel
                {
                    TemplateId = template.Id,
                    TemplateName = template.Name,
                    Description = template.Description
                });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Copy(CopyTemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var copy = await model.CopyAsync(_ctx);
                    return RedirectToAction(nameof(Details), new { id = copy.Id });
                }
                catch (Exception ex)
                {
                    ModelState.TryAddModelError(string.Empty, ex.GetBaseException().Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Deactivate(Guid id)
        {
            var template = await _ctx.FindTemplateAsync(id);
            if (template != null)
            {
                return View(new ActivationViewModel { Id = id, Name = template.Name });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(ActivationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var template = await _ctx.FindTemplateAsync(model.Id);
                if (template != null)
                {
                    template.IsActive = false;
                    await _ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = template.Id });
                }

                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> Reactivate(Guid id)
        {
            var template = await _ctx.FindTemplateAsync(id);
            if (template != null)
            {
                return View(new ActivationViewModel { Id = id, Name = template.Name });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(ActivationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var template = await _ctx.FindTemplateAsync(model.Id);
                if (template != null)
                {
                    template.IsActive = true;
                    await _ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = template.Id });
                }

                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Preview(
            string template,
            string json)
        {
            var data = JObject.Parse(json);
            var html = await MustacheTemplateTransformer.Instance.TransformTextAsync(template, data);
            return Content(html, "text/html");
        }
    }
}
