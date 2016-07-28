using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmailService.Web.ViewModels.Templates
{
    public class TestTemplateViewModel : IValidatableObject
    {
        public Guid TemplateId { get; set; }

        public Guid ApplicationId { get; set; }

        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Sample data is required in JSON format")]
        public string SampleData { get; set; }

        public string Language { get; set; }

        public IEnumerable<SelectListItem> Translations { get; set; } = new List<SelectListItem>();

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
