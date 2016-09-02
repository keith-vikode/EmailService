using EmailService.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Transports
{
    public class EditSmtpViewModel
    {
        public EditSmtpViewModel()
        {
        }

        public EditSmtpViewModel(Transport existing)
        {
            Id = existing.Id;
            Name = existing.Name;
            Hostname = existing.Hostname;
            PortNum = existing.PortNum.GetValueOrDefault();
            Username = existing.Username;
            Password = existing.Password;
            UseSSL = existing.UseSSL;
            SenderName = existing.SenderName;
            SenderAddress = existing.SenderAddress;
        }

        public Guid Id { get; set; }

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

        public Transport UpdateDbModel(Transport existing)
        {
            existing.Name = Name;
            existing.Hostname = Hostname;
            existing.Username = Username;
            existing.Password = Password;
            existing.PortNum = PortNum;
            existing.UseSSL = UseSSL;
            existing.SenderName = SenderName;
            existing.SenderAddress = SenderAddress;

            return existing;
        }
    }
}
