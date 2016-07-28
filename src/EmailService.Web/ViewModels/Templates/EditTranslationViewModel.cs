
using EmailService.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Templates
{
    public class EditTranslationViewModel
    {
        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        public string Language { get; set; }

        [Required]
        public string SubjectTemplate { get; set; }

        [Required]
        public string BodyTemplate { get; set; }

        public static async Task<EditTranslationViewModel> LoadAsync(EmailServiceContext ctx, Guid templateId, string language)
        {
            var template = await ctx.Templates.Include(t => t.Translations).FirstOrDefaultAsync(t => t.Id == templateId);
            var translation = template?.Translations.FirstOrDefault(t => t.Language == language);
            if (translation == null)
            {
                return null;
            }

            var langs = template.Translations.Select(t => t.Language);

            return new EditTranslationViewModel
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                Language = language,
                BodyTemplate = translation.BodyTemplate,
                SubjectTemplate = translation.SubjectTemplate
            };
        }

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var template = await ctx.Templates.Include(t => t.Translations).FirstOrDefaultAsync(t => t.Id == TemplateId);
            var translation = template?.Translations.FirstOrDefault(t => t.Language == Language);
            if (translation != null)
            {
                translation.SubjectTemplate = SubjectTemplate;
                translation.BodyTemplate = BodyTemplate;

                await ctx.SaveChangesAsync();
            }
        }
    }
}
