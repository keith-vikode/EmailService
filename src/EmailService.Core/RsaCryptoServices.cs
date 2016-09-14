using EmailService.Core.Services;
using System;
using System.Security.Cryptography;
using System.Text;

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
        
        public byte[] GenerateKey()
        {
            using (var csp = new RSACryptoServiceProvider(KeyLength))
            {
                return csp.ExportCspBlob(true);
            }
        }

        public byte[] GetApiKey(Guid applicationId, byte[] privateKey)
        {
            using (var csp = new RSACryptoServiceProvider())
            {
                csp.ImportCspBlob(privateKey);
                return csp.SignData(applicationId.ToByteArray(), Hasher);
            }
        }

        public bool VerifyApiKey(Guid applicationId, byte[] apiKey, byte[] privateKey)
        {
            using (var csp = new RSACryptoServiceProvider())
            {
                csp.ImportCspBlob(privateKey);
                return csp.VerifyData(applicationId.ToByteArray(), Hasher, apiKey);
            }
        }
    }
}
