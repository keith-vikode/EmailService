using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Core.Entities;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Applications
{
    public class EditApplicationViewModel
    {
        public Guid Id { get; set; }

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

        public static async Task<EditApplicationViewModel> LoadAsync(EmailServiceContext ctx, Guid id)
        {
            var application = await ctx.FindApplicationAsync(id);
            if (application != null)
            {
                return new EditApplicationViewModel
                {
                    Id = application.Id,
                    Name = application.Name,
                    Description = application.Description,
                    SenderAddress = application.SenderAddress,
                    SenderName = application.SenderName
                };
            }

            return null;
        }

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var existing = await ctx.FindApplicationAsync(Id);
            if (existing != null)
            {
                existing.Name = Name;
                existing.Description = Description;
                existing.SenderAddress = SenderAddress;
                existing.SenderName = SenderName;
                await ctx.SaveChangesAsync();
            }
        }
    }
}
