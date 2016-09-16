using System;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    public interface IApplicationKeyStore
    {
        Task<ApplicationKeyInfo> GetKeysAsync(Guid applicationId);
    }

    public class ApplicationKeyInfo
    {
        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public byte[] PrimaryApiKey { get; set; }

        public byte[] SecondaryApiKey { get; set; }
    }
}
