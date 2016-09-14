using System;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides cryptographic services for use in API authentication.
    /// </summary>
    public interface ICryptoServices
    {
        byte[] GenerateKey();

        byte[] GetApiKey(Guid applicationId, byte[] privateKey);

        bool VerifyApiKey(Guid applicationId, byte[] apiKey, byte[] privateKey);
    }
}
