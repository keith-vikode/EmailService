using EmailService.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Transports
{
    public class EditSendGridViewModel
    {
        public EditSendGridViewModel()
        {
        }

        public EditSendGridViewModel(Transport existing)
        {
            Id = existing.Id;
            Name = existing.Name;
            ApiKey = existing.Password;
        }

        public Guid Id { get; set; }

        [Required]
        [StringLength(NameFieldMaxLength)]
        [Display(Prompt = "My SendGrid account")]
        public string Name { get; set; }

        [Required]
        public string ApiKey { get; set; }

        public Transport UpdateDbModel(Transport transport)
        {
            transport.Name = Name;
            transport.Password = ApiKey;
            return transport;
        }
    }
}
