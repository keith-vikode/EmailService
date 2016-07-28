using EmailService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Transports
{
    public class TransportDetailsViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public TransportType Type { get; set; }

        public bool IsActive { get; set; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";

        public IEnumerable<TransportApplicationInfo> Applications { get; set; } = new List<TransportApplicationInfo>();

        public static async Task<TransportDetailsViewModel> LoadAysnc(EmailServiceContext ctx, Guid id)
        {
            var transport = await ctx.Transports
                .Include(a => a.Applications)
                .ThenInclude(a => a.Application)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transport != null)
            {
                return new TransportDetailsViewModel
                {
                    Id = transport.Id,
                    Name = transport.Name,
                    Type = transport.Type,
                    IsActive = transport.IsActive,
                    Applications = transport.Applications.Select(a => new TransportApplicationInfo
                    {
                        ApplicationId = a.ApplicationId,
                        ApplicationName = a.Application.Name,
                        IsActive = a.Application.IsActive,
                        Priority = a.Priority
                    })
                };
            }

            return null;
        }
    }

    public class TransportApplicationInfo
    {
        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public int Priority { get; set; }

        public bool IsActive { get; set; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";
    }
}
