using EmailService.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Templates
{
    public class AddTranslationViewModel
    {
        public AddTranslationViewModel()
            : this(new List<string>())
        {
        }

        public AddTranslationViewModel(IEnumerable<string> existingLanguages)
        {
            Languages =
                Cultures.AllCultures
                .OrderBy(c => c.DisplayName)
                .Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.DisplayName,
                    Disabled = existingLanguages.Contains(c.Name)
                });
        }

        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }
        
        [Required]
        public string Language { get; set; }

        [Required]
        public string SubjectTemplate { get; set; }

        [Required]
        public string BodyTemplate { get; set; }

        public IEnumerable<SelectListItem> Languages { get; }

        public static async Task<AddTranslationViewModel> LoadAsync(EmailServiceContext ctx, Guid templateId)
        {
            var template = await ctx.Templates.Include(t => t.Translations).FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
            {
                return null;
            }

            var langs = template.Translations.Select(t => t.Language);

            return new AddTranslationViewModel(langs)
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                BodyTemplate = template.BodyTemplate,
                SubjectTemplate = template.SubjectTemplate
            };
        }
    }
}
