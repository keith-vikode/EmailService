using EmailService.Core.Entities;
using System;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Templates
{
    public class RemoveTranslationViewModel
    {
        public Guid TemplateId { get; set; }

        public string Language { get; set; }

        public string TemplateName { get; set; }

        public string LanguageName { get; set; }

        public static async Task<RemoveTranslationViewModel> LoadAsync(EmailServiceContext ctx, Guid id, string language)
        {
            var template = await ctx.FindTemplateWithTranslationsAsync(id);
            var translation = template?.Translations.Find(t => t.Language == language);
            if (translation != null)
            {
                return new RemoveTranslationViewModel
                {
                    TemplateId = template.Id,
                    TemplateName = template.Name,
                    Language = translation.Language,
                    LanguageName = translation.GetCultureName()
                };
            }

            return null;
        }

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var template = await ctx.FindTemplateWithTranslationsAsync(TemplateId);
            var translation = template?.Translations.Find(t => t.Language == Language);
            if (translation != null)
            {
                ctx.Translations.Remove(translation);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
