using EmailService.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EmailService.Core.Entities
{
    public class DbApplicationKeyStore : IApplicationKeyStore
    {
        private readonly EmailServiceContext _ctx;
        private readonly ILogger _logger;

        public DbApplicationKeyStore(EmailServiceContext context, ILoggerFactory loggerFactory)
        {
            _ctx = context;
            _logger = loggerFactory.CreateLogger<DbApplicationKeyStore>();
        }

        public async Task<ApplicationKeyInfo> GetKeysAsync(Guid applicationId)
        {
            _logger.LogDebug($"Attempting to load API keys for application {applicationId}");

            var app = await _ctx.FindApplicationAsync(applicationId);
            if (app != null)
            {
                _logger.LogDebug($"Found application {applicationId} in the database");
                return new ApplicationKeyInfo
                {
                    ApplicationId = app.Id,
                    ApplicationName = app.Name,
                    PrimaryApiKey = app.PrimaryApiKey,
                    SecondaryApiKey = app.SecondaryApiKey
                };
            }
            else
            {
                _logger.LogDebug($"Could not find application {applicationId} in the database");
            }

            return null;
        }
    }
}
