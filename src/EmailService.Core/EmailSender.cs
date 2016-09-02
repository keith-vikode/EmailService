using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Core.Templating;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core
{
    /// <summary>
    /// Handles the transformation and sending of emails.
    /// </summary>
    public class EmailSender
    {
        // TODO: in-memory cache with expirations
        private readonly EmailServiceContext _database;
        private readonly IEmailTransportFactory _transportFactory;
        private ITemplateTransformer _templateTransformer;

        public EmailSender(EmailServiceContext database, IEmailTransportFactory transportFactory)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (transportFactory == null)
            {
                throw new ArgumentNullException(nameof(transportFactory));
            }

            _database = database;
            _transportFactory = transportFactory;
        }

        /// <summary>
        /// Gets or sets the <see cref="ITemplateTransformer"/> to use when rendering emails.
        /// </summary>
        /// <remarks>
        /// This property allows you to plug in a custom transformer, or test against a mock.
        /// </remarks>
        public ITemplateTransformer Transformer
        {
            get
            {
                if (_templateTransformer == null)
                {
                    _templateTransformer = MustacheTemplateTransformer.Instance;
                }

                return _templateTransformer;
            }

            set
            {
                _templateTransformer = value;
            }
        }

        public async Task<bool> SendEmailAsync(EmailMessageParams args)
        {
            EmailTemplate email;
            
            // we're doing a lot of database work here that I'd like to try avoiding in future;
            // caching in this class might help, but we could also consider writing the templates
            // to a blob store in JSON, including the transport and application details in one go
            var transports = new Queue<Transport>(await GetTransportsAsync(args.ApplicationId));
            if (!transports.Any())
            {
                return false;
            }

            var application = await GetApplicationAsync(args.ApplicationId);
            if (application == null)
            {
                return false;
            }

            if (args.TemplateId.HasValue)
            {
                email = await GetTemplateAsync(args.TemplateId.Value, args.GetCulture());
                if (email == null)
                {
                    return false;
                }
            }
            else
            {
                email = new EmailTemplate(args.Subject, args.GetBody(), args.GetCulture());
            }
            
            if (args.Data != null)
            {
                email = await Transformer.TransformTemplateAsync(email, args.Data);
            }

            var senderParams = new SenderParams
            {
                To = args.To,
                CC = args.CC,
                Bcc = args.Bcc,
                Subject = email.Subject,
                Body = email.Body,
                SenderName = args.SenderName ?? application.SenderName,
                SenderAddress = args.SenderAddress ?? application.SenderAddress
            };

            bool success = false;
            while (!success && transports.Any())
            {
                var transportInfo = transports.Dequeue();
                var transport = _transportFactory.CreateTransport(transportInfo);

                try
                {
                    success = await transport.SendAsync(senderParams);
                }
                catch (Exception)
                {
                    success = false;
                }
            }

            return success;
        }

        private Task<Application> GetApplicationAsync(Guid applicationId)
        {
            // TODO: in-memory caching of applications
            return _database.Applications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        private Task<List<Transport>> GetTransportsAsync(Guid applicationId)
        {
            // TODO: in-memory caching of transports
            return _database.Applications
                .AsNoTracking()
                .Where(a => a.Id == applicationId)
                .SelectMany(t => t.Transports)
                .OrderBy(t => t.Priority)
                .Select(t => t.Transport)
                .ToListAsync();
        }

        private async Task<EmailTemplate> GetTemplateAsync(Guid templateId, CultureInfo culture)
        {
            // TODO: in-memory caching of templates
            EmailTemplate result = null;
            
            // if no culture or an invariant culture was supplied, don't bother loading translations
            // at all, and just load the bare template
            if (culture == null || culture == CultureInfo.InvariantCulture)
            {
                var template = await _database.Templates.AsNoTracking().SingleOrDefaultAsync(t => t.Id == templateId);
                if (template != null)
                {
                    result = new EmailTemplate(template.SubjectTemplate, template.BodyTemplate, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                // we want a specific culture, so we need to load the translations and the root
                // template in case the translation we're after doesn't exist
                var template = await _database.Templates.Include(t => t.Translations).AsNoTracking().SingleOrDefaultAsync(t => t.Id == templateId);
                if (template != null)
                {
                    var translation = template.Translations.FirstOrDefault(t => t.Language == culture.Name);
                    if (translation != null)
                    {
                        result = new EmailTemplate(translation.SubjectTemplate, translation.BodyTemplate, culture);
                    }
                    else
                    {
                        result = new EmailTemplate(template.SubjectTemplate, template.BodyTemplate, CultureInfo.InvariantCulture);
                    }
                }
            }

            return result;
        }
    }
}
