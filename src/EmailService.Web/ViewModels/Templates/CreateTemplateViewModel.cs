using EmailService.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Templates
{
    public class CreateTemplateViewModel : IValidatableObject
    {
        public IEnumerable<SelectListItem> Applications { get; set; } = new List<SelectListItem>();

        [Required]
        public Guid? ApplicationId { get; set; }

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

        public string SampleData { get; set; } = "{ }";

        public bool UseHtml { get; set; }

        public Template CreateDbModel()
        {
            return new Template
            {
                ApplicationId = ApplicationId.GetValueOrDefault(),
                Name = Name,
                Description = Description,
                SubjectTemplate = SubjectTemplate,
                BodyTemplate = BodyTemplate,
                SampleData = SampleData,
                UseHtml = UseHtml
            };
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
