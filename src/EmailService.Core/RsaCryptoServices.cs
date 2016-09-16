using EmailService.Core.Services;
using System;
using System.Security.Cryptography;

namespace EmailService.Core
{
    /// <summary>
    /// Provides cryptographic services using in-memory RSA keys.
    /// </summary>
    public class RsaCryptoServices : ICryptoServices
    {
        private const int KeyLength = 1024;

        private static readonly HashAlgorithm Hasher = SHA256.Create();

        private static readonly Lazy<RsaCryptoServices> InstanceLazy = new Lazy<RsaCryptoServices>(() => new RsaCryptoServices(), true);

        private RsaCryptoServices()
        {
        }

        public static RsaCryptoServices Instance => InstanceLazy.Value;
        
        public byte[] GeneratePrivateKey()
        {
            using (var csp = new RSACryptoServiceProvider(KeyLength))
            {
                return csp.ExportCspBlob(true);
            }
        }

        public string GetApiKey(Guid applicationId, byte[] privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            using (var csp = new RSACryptoServiceProvider())
            {
                csp.ImportCspBlob(privateKey);
                var key = csp.SignData(applicationId.ToByteArray(), Hasher);
                return Convert.ToBase64String(key);
            }
        }

        public bool VerifyApiKey(Guid applicationId, string apiKey, byte[] privateKey)
        {
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            using (var csp = new RSACryptoServiceProvider())
            {
                csp.ImportCspBlob(privateKey);
                try
                {
                    var keyBytes = Convert.FromBase64String(apiKey);
                    return csp.VerifyData(applicationId.ToByteArray(), Hasher, keyBytes);
                }
                catch (FormatException)
                {
                    return false;
                }
            }
        }
    }
}
