using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Core.Entities
{
    public class Layout
    {
        public List<Template> Templates { get; set; } = new List<Template>();

        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }

        public Application Application { get; set; }

        [Required]
        [MaxLength(NameFieldMaxLength)]
        public string Name { get; set; }

        [Required]
        public string BodyHtml { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Timestamp]
        public byte[] ConcurrencyToken { get; set; }
    }
}