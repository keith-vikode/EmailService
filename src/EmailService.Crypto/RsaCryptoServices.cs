using EmailService.Core.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace EmailService.Crypto
{
    /// <summary>
    /// Provides cryptographic services using in-memory RSA keys.
    /// </summary>
    public class RsaCryptoServices : ICryptoServices
    {
        private const int KeyLength = 1024;

        private static readonly Lazy<RsaCryptoServices> InstanceLazy = new Lazy<RsaCryptoServices>(() => new RsaCryptoServices(), true);

        private RsaCryptoServices()
        {
        }

        public static RsaCryptoServices Instance => InstanceLazy.Value;

        public void GenerateKey(out string publicKey, out string privateKey)
        {
            using (var csp = new RSACryptoServiceProvider(KeyLength))
            {
                privateKey = csp.ExportParameters(true).ToXmlString();
                publicKey = EncodePublicKey(csp.ExportParameters(false));
            }
        }

        public string Encrypt(string publicKey, string plaintext)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(DecodePublicKey(publicKey));
                var data = Encoding.Unicode.GetBytes(plaintext);
                var encrypted = rsa.Encrypt(data, false);
                return Convert.ToBase64String(encrypted);
            }
        }

        public bool TryDecrypt(string privateKey, string encrypted, out string decrypted)
        {
            bool result;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                var data = Convert.FromBase64String(encrypted);
                try
                {
                    var decryptedBytes = rsa.Decrypt(data, false);
                    decrypted = Encoding.Unicode.GetString(decryptedBytes);
                    result = true;
                }
                catch (CryptographicException)
                {
                    decrypted = string.Empty;
                    result = false;
                }
            }

            return result;
        }
        
        private static string EncodePublicKey(RSAParameters publicKey)
        {
            var encoded = new byte[publicKey.Modulus.Length + publicKey.Exponent.Length];
            publicKey.Modulus.CopyTo(encoded, 0);
            publicKey.Exponent.CopyTo(encoded, publicKey.Modulus.Length);
            return Convert.ToBase64String(encoded);
        }

        private static RSAParameters DecodePublicKey(string apiKey)
        {
            const int ModulusLen = 128;
            var bytes = Convert.FromBase64String(apiKey);
            using (var ms = new MemoryStream(bytes))
            using (var reader = new BinaryReader(ms))
            {
                return new RSAParameters
                {
                    Modulus = reader.ReadBytes(ModulusLen),
                    Exponent = reader.ReadBytes(bytes.Length - ModulusLen)
                };
            }
        }
    }

    public static class RsaExtensions
    {
        private static readonly XmlSerializer RSAXmlSerializer = new XmlSerializer(typeof(RSAParameters));

        public static string ToXmlString(this RSAParameters param)
        {
            using (var sw = new StringWriter())
            {
                RSAXmlSerializer.Serialize(sw, param);
                return sw.ToString();
            }
        }

        public static void FromXmlString(this RSACryptoServiceProvider rsa, string xml)
        {
            using (var sr = new StringReader(xml))
            {
                var rsaParams = (RSAParameters)RSAXmlSerializer.Deserialize(sr);
                rsa.ImportParameters(rsaParams);
            }
        }
    }
}
