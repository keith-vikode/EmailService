using System;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides cryptographic services for use in API authentication.
    /// </summary>
    public interface ICryptoServices
    {
        byte[] GeneratePrivateKey();

        string GetApiKey(Guid applicationId, byte[] privateKey);

        bool VerifyApiKey(Guid applicationId, string apiKey, byte[] privateKey);
    }
}
