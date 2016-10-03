using EmailService.Core.Abstraction;
using EmailService.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core.Entities
{
    public class DbTemplateStore : IEmailTemplateStore
    {
        private readonly DbContextOptions<EmailServiceContext> _options;

        public DbTemplateStore(DbContextOptions<EmailServiceContext> options)
        {
            _options = options;
        }

        public async Task<EmailTemplateInfo> GetTemplateAsync(EmailMessageParams args)
        {
            var applicationKey = $"app_{args.ApplicationId}";
            var transportsKey = $"trn_{args.ApplicationId}";
            var templateKey = $"tpl_{args.ApplicationId}_{args.Culture}";

            var response = new EmailTemplateInfo();

            // ugly caching code
            var application = await GetApplicationAsync(args.ApplicationId);

            if (application == null)
            {
                return response;
            }

            response.ApplicationName = application.Name;
            response.SenderAddress = application.SenderAddress;
            response.SenderName = application.SenderName;

            // ugly caching code
            var transports = (await GetTransportsAsync(args.ApplicationId)).ToArray();

            response.TransportQueue = new Queue<ITransportDefinition>(transports);
            
            if (args.TemplateId.HasValue)
            {
                // ugly caching code
                response.Template = await GetTemplateAsync(args.TemplateId.Value, args.GetCulture());
            }
            else
            {
                response.Template = new EmailTemplate(args.Subject, args.GetBody(), args.GetCulture());
            }

            return response;
        }

        private async Task<Application> GetApplicationAsync(Guid applicationId)
        {
            Application app;
            using (var database = new EmailServiceContext(_options))
            {
                app = await database.Applications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == applicationId);
            }

            return app;
        }

        private async Task<List<Transport>> GetTransportsAsync(Guid applicationId)
        {
            List<Transport> transports;
            using (var database = new EmailServiceContext(_options))
            {
                transports = await database.Applications
                    .AsNoTracking()
                    .Where(a => a.Id == applicationId)
                    .SelectMany(t => t.Transports)
                    .OrderBy(t => t.Priority)
                    .Select(t => t.Transport)
                    .ToListAsync();
            }

            return transports;
        }

        private async Task<EmailTemplate> GetTemplateAsync(Guid templateId, CultureInfo culture)
        {
            EmailTemplate result = null;

            using (var database = new EmailServiceContext(_options))
            {
                // if no culture or an invariant culture was supplied, don't bother loading translations
                // at all, and just load the bare template
                if (culture == null || culture == CultureInfo.InvariantCulture)
                {
                    var template = await database.Templates.AsNoTracking().SingleOrDefaultAsync(t => t.Id == templateId);
                    if (template != null)
                    {
                        result = new EmailTemplate(template.SubjectTemplate, template.BodyTemplate, CultureInfo.InvariantCulture, template.Name);
                    }
                }
                else
                {
                    // we want a specific culture, so we need to load the translations and the root
                    // template in case the translation we're after doesn't exist
                    var template = await database.Templates.Include(t => t.Translations).AsNoTracking().SingleOrDefaultAsync(t => t.Id == templateId);
                    if (template != null)
                    {
                        var translation = template.Translations.FirstOrDefault(t => t.Language == culture.Name);
                        if (translation != null)
                        {
                            result = new EmailTemplate(translation.SubjectTemplate, translation.BodyTemplate, culture, template.Name);
                        }
                        else
                        {
                            result = new EmailTemplate(template.SubjectTemplate, template.BodyTemplate, CultureInfo.InvariantCulture, template.Name);
                        }
                    }
                }
            }

            return result;
        }
    }
}
