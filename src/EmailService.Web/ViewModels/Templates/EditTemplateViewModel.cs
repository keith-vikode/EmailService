using EmailService.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Templates
{
    public class EditTemplateViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(NameFieldMaxLength)]
        public string Name { get; set; }

        [MaxLength(DescriptionFieldMaxLength)]
        public string Description { get; set; }

        [Required]
        [MaxLength(SubjectFieldMaxLength)]
        public string SubjectTemplate { get; set; }

        [Required]
        public string BodyTemplate { get; set; }

        public string SampleData { get; set; }

        public bool UseHtml { get; set; }

        public static async Task<EditTemplateViewModel> LoadAsync(EmailServiceContext ctx, Guid templateId)
        {
            var template = await ctx.Templates.FindAsync(templateId);

            return new EditTemplateViewModel
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                SampleData = template.SampleData,
                BodyTemplate = template.BodyTemplate,
                SubjectTemplate = template.SubjectTemplate,
                UseHtml = template.UseHtml
            };
        }

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var template = await ctx.Templates.FindAsync(Id);
            if (template != null)
            {
                template.Name = Name;
                template.Description = Description;
                template.SubjectTemplate = SubjectTemplate;
                template.BodyTemplate = BodyTemplate;
                template.SampleData = SampleData;
                template.UseHtml = UseHtml;

                await ctx.SaveChangesAsync();
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(SampleData))
            {
                bool invalid = false;
                try
                {
                    var converted = JObject.Parse(SampleData);
                }
                catch (Exception)
                {
                    invalid = true;
                }

                if (invalid)
                {
                    yield return new ValidationResult("Invalid JSON data", new string[] { nameof(SampleData) });
                }
            }
        }
    }
}
