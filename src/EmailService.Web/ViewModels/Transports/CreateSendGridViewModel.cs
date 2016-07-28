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

        public Transport CreateDbModel()
        {
            return new Transport
            {
                Name = Name,
                Type = TransportType.SendGrid,
                Password = ApiKey,
                UseSSL = true,
                IsActive = true
            };
        }
    }
}
