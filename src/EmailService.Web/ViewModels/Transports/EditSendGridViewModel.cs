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
            SenderName = existing.SenderName;
            SenderAddress = existing.SenderAddress;
        }

        public Guid Id { get; set; }

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

        public Transport UpdateDbModel(Transport existing)
        {
            existing.Name = Name;
            existing.Password = ApiKey;
            existing.SenderName = SenderName;
            existing.SenderAddress = SenderAddress;

            return existing;
        }
    }
}
