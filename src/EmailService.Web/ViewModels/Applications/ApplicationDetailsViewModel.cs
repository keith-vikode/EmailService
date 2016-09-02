using EmailService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Applications
{
    public class ApplicationDetailsViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string SenderAddress { get; set; }

        public string SenderName { get; set; }

        public string ApiKey { get; set; }

        public DateTime CreatedUtc { get; set; }

        public bool IsActive { get; set; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";

        public List<KeyValuePair<Guid, string>> Templates { get; set; }

        public List<KeyValuePair<Guid, string>> Transports { get; set; }

        public static async Task<ApplicationDetailsViewModel> LoadAsync(EmailServiceContext ctx, Guid id)
        {
            var app = await ctx.Applications
                .Include(a => a.Templates)
                .Include(a => a.Transports)
                .ThenInclude(t => t.Transport)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app != null)
            {
                return new ApplicationDetailsViewModel
                {
                    Id = app.Id,
                    Name = app.Name,
                    Description = app.Description,
                    SenderAddress = app.SenderAddress,
                    SenderName = app.SenderName,
                    ApiKey = app.PublicKey,
                    CreatedUtc = app.CreatedUtc,
                    IsActive = app.IsActive,
                    Templates = app.Templates.Select(t => new KeyValuePair<Guid, string>(t.Id, t.Name)).ToList(),
                    Transports = app.Transports.Select(t => new KeyValuePair<Guid, string>(t.TransportId, t.Transport.Name)).ToList()
                };
            }

            return null;
        }
    }
}
