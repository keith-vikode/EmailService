using EmailService.Core.Entities;
using EmailService.Core.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Web.ViewModels.Applications
{
    public class CreateApplicationViewModel : IValidatableObject
    {
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

        public Guid[] SelectedTransports { get; set; }

        public IEnumerable<SelectListItem> Transports { get; set; }

        public Application CreateDbModel()
        {
            var app = new Application
            {
                Name = Name,
                Description = Description,
                SenderAddress = SenderAddress,
                SenderName = SenderName,
                IsActive = true
            };

            foreach (var value in SelectedTransports)
            {
                app.Transports.Add(new ApplicationTransport { TransportId = value });
            }

            return app;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedTransports?.Length < 1)
            {
                yield return new ValidationResult("Please select a transport", new string[] { nameof(SelectedTransports) });
            }
        }

        public async Task LoadTransportAsync(EmailServiceContext ctx)
        {
            var transports = await ctx.Transports.ToListAsync();
            Transports = new SelectList(transports, nameof(Transport.Id), nameof(Transport.Name));
        }

        public async Task<Application> SaveChangesAsync(EmailServiceContext ctx, ICryptoServices crypto)
        {
            string publicKey, privateKey;
            crypto.GenerateKey(out publicKey, out privateKey);

            var app = CreateDbModel();
            app.PublicKey = publicKey;
            app.PrivateKey = privateKey;
            ctx.Applications.Add(app);
            await ctx.SaveChangesAsync();
            return app;
        }
    }
}
