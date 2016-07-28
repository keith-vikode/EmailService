using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core.Entities
{
    public static class EmailServiceContextExtensions
    {
        public static Task<Template> FindTemplateAsync(this EmailServiceContext ctx, Guid id)
        {
            return ctx.Templates.FirstOrDefaultAsync(t => t.Id == id);
        }

        public static Task<Template> FindTemplateWithTranslationsAsync(this EmailServiceContext ctx, Guid id)
        {
            return ctx.Templates.Include(t => t.Translations).FirstOrDefaultAsync(t => t.Id == id);
        }

        public static Task<Transport> FindTransportAsync(this EmailServiceContext ctx, Guid id)
        {
            return ctx.Transports.FirstOrDefaultAsync(t => t.Id == id);
        }

        public static Task<Application> FindApplicationAsync(this EmailServiceContext ctx, Guid id)
        {
            return ctx.Applications.FirstOrDefaultAsync(t => t.Id == id);
        }
        
        public static Task<List<Transport>> GetApplicationTransportsAsync(this EmailServiceContext ctx, Guid applicationId)
        {
            return ctx.Applications
                .SelectMany(a => a.Transports)
                .Where(t => t.ApplicationId == applicationId)
                .OrderByDescending(t => t.Priority)
                .Select(t => t.Transport)
                .ToListAsync();
        }
    }
}
