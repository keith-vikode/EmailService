using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmailService.Web.ViewModels.Applications
{
    public class PrioritiseTransportsViewModel : IValidatableObject
    {
        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public List<TransportPriorityViewModel> Transports { get; set; }

        public static Task<PrioritiseTransportsViewModel> LoadAsync(EmailServiceContext ctx, Guid id)
        {
            PrioritiseTransportsViewModel model = null;

            var app = ctx.Applications
                .Include(a => a.Transports)
                .ThenInclude(a => a.Transport)
                .FirstOrDefault(a => a.Id == id);

            if (app != null)
            {
                model = new PrioritiseTransportsViewModel
                {
                    ApplicationId = app.Id,
                    ApplicationName = app.Name
                };
                
                model.Transports = app.Transports.Select(t => new TransportPriorityViewModel
                {
                    TransportId = t.TransportId,
                    Priority = t.Priority,
                    Name = t.Transport.Name
                }).ToList();
            }

            return Task.FromResult(model);
        }

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var app = await ctx.FindApplicationAsync(ApplicationId);

            foreach (var transport in Transports)
            {
                var existing = await ctx.Transports
                    .Where(t => t.Id == transport.TransportId)
                    .SelectMany(t => t.Applications)
                    .FirstOrDefaultAsync(a => a.ApplicationId == ApplicationId);
                if (existing != null)
                {
                    if (transport.Remove)
                    {
                        app.Transports.Remove(existing);
                        ctx.Remove(existing);
                    }
                    else
                    {
                        existing.Priority = transport.Priority;
                    }
                }
            }

            await ctx.SaveChangesAsync();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Transports.All(t => t.Remove))
            {
                yield return new ValidationResult("At least one transport must be left");
            }
        }
    }

    public class TransportPriorityViewModel
    {
        public Guid TransportId { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }

        public bool Remove { get; set; }
    }
}
