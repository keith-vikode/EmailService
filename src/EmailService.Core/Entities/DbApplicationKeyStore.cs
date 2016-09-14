using EmailService.Core.Services;
using System;
using System.Threading.Tasks;

namespace EmailService.Core.Entities
{
    public class DbApplicationKeyStore : IApplicationKeyStore
    {
        private EmailServiceContext _ctx;

        public DbApplicationKeyStore(EmailServiceContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Tuple<byte[], byte[]>> GetKeysAsync(Guid applicationId)
        {
            var app = await _ctx.FindApplicationAsync(applicationId);
            if (app != null)
            {
                return new Tuple<byte[], byte[]>(app.PrimaryApiKey, app.SecondaryApiKey);
            }

            return null;
        }
    }
}
