using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Core.Entities
{
    public class Template
    {
        public List<Translation> Translations { get; set; } = new List<Translation>();

        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public Application Application { get; set; }

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
    }
}
