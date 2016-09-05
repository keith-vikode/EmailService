using EmailService.Core;
using EmailService.Core.Entities;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Transports
{
    public class CreateSendGridViewModel
    {
        [Required]
        [StringLength(NameFieldMaxLength)]
        [Display(Prompt = "My SendGrid account")]
        public string Name { get; set; }

        [Required]
        public string ApiKey { get; set; }

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
                Type = TransportType.SendGrid,
                Password = ApiKey,
                UseSSL = true,
                IsActive = true,
                SenderAddress = SenderAddress,
                SenderName = SenderName
            };
        }
    }
}
