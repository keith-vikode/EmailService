﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Core.Entities
{
    public class Application
    {
        public List<ApplicationTransport> Transports { get; set; } = new List<ApplicationTransport>();

        public List<Template> Templates { get; set; } = new List<Template>();

        public List<Layout> Layouts { get; set; } = new List<Layout>();

        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();

        [Required]
        [MaxLength(NameFieldMaxLength)]
        public string Name { get; set; }

        [MaxLength(DescriptionFieldMaxLength)]
        public string Description { get; set; }

        [Required]
        [MaxLength(SenderAddressMaxLength)]
        public string SenderAddress { get; set; }

        [MaxLength(SenderNameMaxLength)]
        public string SenderName { get; set; }

        public byte[] PrimaryApiKey { get; set; }

        public byte[] SecondaryApiKey { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Timestamp]
        public byte[] ConcurrencyToken { get; set; }
    }
}
