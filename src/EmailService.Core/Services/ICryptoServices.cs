namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides cryptographic services for use in API authentication.
    /// </summary>
    public interface ICryptoServices
    {
        void GenerateKey(out string publicKey, out string privateKey);

        string Encrypt(string publicKey, string plaintext);

        bool TryDecrypt(string privateKey, string encrypted, out string decrypted);
    }
}
