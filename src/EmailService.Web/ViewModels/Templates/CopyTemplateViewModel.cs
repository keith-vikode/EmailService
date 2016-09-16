using EmailService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Templates
{
    public class CopyTemplateViewModel : IValidatableObject
    {
        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Please supply a new template name")]
        [StringLength(NameFieldMaxLength)]
        public string NewName { get; set; }

        [StringLength(DescriptionFieldMaxLength)]
        public string Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.Equals(TemplateName, NewName, StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("New template name cannot be the same as the old", new string[] { nameof(NewName) });
            }
        }

        public async Task<Template> CopyAsync(EmailServiceContext ctx)
        {
            Template copy = null;

            var template = await ctx.Templates.Include(t => t.Translations).FirstOrDefaultAsync(t => t.Id == TemplateId);
            if (template != null)
            {
                copy = new Template
                {
                    ApplicationId = template.ApplicationId,
                    Name = NewName,
                    Description = Description,
                    BodyTemplate = template.BodyTemplate,
                    SubjectTemplate = template.SubjectTemplate,
                    UseHtml = template.UseHtml,
                    SampleData = template.SampleData
                };
                ctx.Templates.Add(copy);

                foreach (var translation in template.Translations)
                {
                    var copyTran = new Translation { TemplateId = copy.Id, Language = translation.Language, SubjectTemplate = translation.SubjectTemplate, BodyTemplate = translation.BodyTemplate };
                    ctx.Translations.Add(copyTran);
                    copy.Translations.Add(copyTran);
                }

                await ctx.SaveChangesAsync();
            }

            return copy;
        }
    }
}
