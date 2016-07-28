using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Core.Entities
{
    public class Translation
    {
        public Guid TemplateId { get; set; }

        public Template Template { get; set; }

        [MaxLength(10)]
        public string Language { get; set; }

        [Required]
        [MaxLength(SubjectFieldMaxLength)]
        public string SubjectTemplate { get; set; }

        [Required]
        public string BodyTemplate { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Timestamp]
        public byte[] ConcurrencyToken { get; set; }

        public string GetCultureName() => GetCulture().DisplayName;

        public CultureInfo GetCulture()
        {
            if (!string.IsNullOrWhiteSpace(Language))
            {
                return new CultureInfo(Language);
            }

            return CultureInfo.InvariantCulture;
        }
    }
}
