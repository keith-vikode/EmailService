using EmailService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace EmailService.Core.Entities
{
    public class DbApplicationKeyStore : IApplicationKeyStore
    {
        private readonly EmailServiceContext _ctx;
        private readonly IMemoryCache _cache;

        public DbApplicationKeyStore(DbContextOptions<EmailServiceContext> options, IMemoryCache cache)
        {
            _ctx = new EmailServiceContext(options);
            _cache = cache;
        }

        public async Task<ApplicationKeyInfo> GetKeysAsync(Guid applicationId)
        {
            // TODO: implement caching
            var app = await _ctx.FindApplicationAsync(applicationId);
            if (app != null)
            {
                return new ApplicationKeyInfo
                {
                    ApplicationId = app.Id,
                    ApplicationName = app.Name,
                    PrimaryApiKey = app.PrimaryApiKey,
                    SecondaryApiKey = app.SecondaryApiKey
                };
            }

            return null;
        }
    }
}
