using EmailService.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Applications
{
    public class AddTransportViewModel
    {
        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        [Required(ErrorMessage = "Please select a transport")]
        public Guid? TransportId { get; set; }

        [Range(0, 999, ErrorMessage = "Priority must be between 0 and 999")]
        public int Priority { get; set; }

        public List<SelectListItem> Transports { get; set; } = new List<SelectListItem>();

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var app = await ctx.FindApplicationAsync(ApplicationId);
            if (app != null)
            {
                app.Transports.Add(new ApplicationTransport
                {
                    TransportId = TransportId.GetValueOrDefault(),
                    Priority = Priority
                });

                await ctx.SaveChangesAsync();
            }
        }
    }
}
