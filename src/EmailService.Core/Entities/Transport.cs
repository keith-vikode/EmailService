using EmailService.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Core.Entities
{
    public class Transport : IValidatableObject, ITransportDefinition
    {
        public List<ApplicationTransport> Applications { get; set; }

        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();

        [Required]
        [StringLength(NameFieldMaxLength)]
        public string Name { get; set; }

        public TransportType Type { get; set; }

        [MaxLength(HostnameFieldMaxLength)]
        public string Hostname { get; set; }

        [MaxLength(255)]
        public string Username { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        public short? PortNum { get; set; }

        public bool UseSSL { get; set; }

        public bool IsActive { get; set; }

        [Required]
        [MaxLength(SenderAddressMaxLength)]
        public string SenderAddress { get; set; }

        [MaxLength(SenderNameMaxLength)]
        public string SenderName { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Timestamp]
        public byte[] ConcurrencyToken { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == TransportType.Smtp)
            {
                if (string.IsNullOrWhiteSpace(Hostname))
                {
                    yield return new ValidationResult("Hostname is required for SMTP transports", new string[] { nameof(Hostname) });
                }
            }
        }
    }
}
