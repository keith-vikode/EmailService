using EmailService.Core.Entities;
using EmailService.Web.ViewModels.Templates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers
{
    public partial class TemplatesController
    {
        public async Task<IActionResult> AddTranslation(Guid id)
        {
            var model = await AddTranslationViewModel.LoadAsync(_ctx, id);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddTranslation(AddTranslationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var template = await _ctx.FindTemplateAsync(model.TemplateId);
                if (template == null)
                {
                    return NotFound();
                }

                template.Translations.Add(new Translation
                {
                    Language = model.Language,
                    SubjectTemplate = model.SubjectTemplate,
                    BodyTemplate = model.BodyTemplate
                });

                await _ctx.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = model.TemplateId });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTranslation(Guid id, string language)
        {
            var model = await EditTranslationViewModel.LoadAsync(_ctx, id, language);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditTranslation(Guid id, string language, EditTranslationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id = model.TemplateId });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveTranslation(Guid id, string language)
        {
            var model = await RemoveTranslationViewModel.LoadAsync(_ctx, id, language);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTranslation(RemoveTranslationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id = model.TemplateId });
            }

            return View(model);
        }
    }
}
