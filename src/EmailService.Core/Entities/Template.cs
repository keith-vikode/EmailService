using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using static EmailService.Core.Constants;

namespace EmailService.Core.Entities
{
    public class Template
    {
        public List<Translation> Translations { get; set; } = new List<Translation>();

        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }

        public Application Application { get; set; }

        public Guid? LayoutId { get; set; }

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
        
        public bool UseHtml { get; set; } = true;

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Timestamp]
        public byte[] ConcurrencyToken { get; set; }

        public EmailTemplate TryGetTranslation(CultureInfo culture)
        {
            var translation = Translations?.FirstOrDefault(t => t.Language == culture.Name);
            var subject = translation?.SubjectTemplate ?? SubjectTemplate;
            var body = translation?.BodyTemplate ?? BodyTemplate;
            return new EmailTemplate(subject, body, culture, Name);
        }
    }
}
