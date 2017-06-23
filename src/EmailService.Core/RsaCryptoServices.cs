using EmailService.Core.Services;
using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace EmailService.Core
{
    /// <summary>
    /// Provides cryptographic services using in-memory RSA keys.
    /// </summary>
    public class RsaCryptoServices : ICryptoServices
    {
        private const int KeyLength = 1024;

        private static readonly RSASignaturePadding Padding = RSASignaturePadding.Pkcs1;
        private static readonly HashAlgorithmName HasherName = HashAlgorithmName.SHA256;

        private static readonly Lazy<RsaCryptoServices> InstanceLazy = new Lazy<RsaCryptoServices>(() => new RsaCryptoServices(), true);

        private RsaCryptoServices()
        {
        }

        public static RsaCryptoServices Instance => InstanceLazy.Value;

        public byte[] GeneratePrivateKey()
        {
            using (var csp = CreateProvider())
            {
                return ExportRsa(csp);
            }
        }

        public string GetApiKey(Guid applicationId, byte[] privateKey)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            using (var csp = ImportRsa(privateKey))
            {
                // generate the API key by signing the application GUID
                var data = applicationId.ToByteArray();
                var key = csp.SignData(data, HasherName, Padding);

                // return as base64 for transport and storage
                return Convert.ToBase64String(key);
            }
        }

        public bool VerifyApiKey(Guid applicationId, string apiKey, byte[] privateKey)
        {
            if (apiKey == null)
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            using (var csp = ImportRsa(privateKey))
            {
                try
                {
                    // get byte arrays from the provided data
                    var signature = Convert.FromBase64String(apiKey);
                    var data = applicationId.ToByteArray();

                    // verify that the given API key is a match for the application ID
                    // signed with the given private key
                    return csp.VerifyData(data, signature, HasherName, Padding);
                }
                catch (FormatException)
                {
                    return false;
                }
            }
        }

        private RSA CreateProvider()
        {
            var csp = RSA.Create();
            csp.KeySize = KeyLength;
            return csp;
        }

        private byte[] ExportRsa(RSA rsa)
        {
            var @params = rsa.ExportParameters(true);
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(@params));
        }

        private RSA ImportRsa(byte[] data)
        {
            var text = Encoding.ASCII.GetString(data);
            var @params = JsonConvert.DeserializeObject<RSAParameters>(text);

            var rsa = CreateProvider();
            rsa.ImportParameters(@params);

            return rsa;
        }
    }
}
