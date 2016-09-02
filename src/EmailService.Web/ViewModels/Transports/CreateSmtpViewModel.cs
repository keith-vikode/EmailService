using EmailService.Core;
using EmailService.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Transports
{
    public class CreateSmtpViewModel
    {
        [Required]
        [StringLength(NameFieldMaxLength)]
        [Display(Prompt = "My SMTP server")]
        public string Name { get; set; }

        [Required]
        [StringLength(HostnameFieldMaxLength)]
        [Display(Prompt = "mail.example.com")]
        public string Hostname { get; set; }

        public short PortNum { get; set; } = 25;

        [Display(Prompt = "emailuser")]
        public string Username { get; set; }

        [Display(Prompt = "password123")]
        public string Password { get; set; }
        
        public bool UseSSL { get; set; }

        [Required]
        [MaxLength(SenderAddressMaxLength)]
        public string SenderAddress { get; set; }

        [MaxLength(SenderNameMaxLength)]
        public string SenderName { get; set; }

        public Transport CreateDbModel()
        {
            return new Transport
            {
                Name = Name,
                Hostname = Hostname,
                Username = Username,
                Password = Password,
                PortNum = PortNum,
                UseSSL = UseSSL,
                IsActive = true,
                SenderAddress = SenderAddress,
                SenderName = SenderName
            };
        }
    }
}
