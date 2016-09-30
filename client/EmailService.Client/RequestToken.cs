using System;
using System.IO;

namespace EmailService.Client
{
    /// <summary>
    /// A token is returned after a request to send an email has been queued. The token
    /// can be used to check the status of the request, or cancel it if it has not already
    /// been processed.
    /// </summary>
    public class RequestToken
    {
        private Lazy<Tuple<DateTime, Guid, Guid>> _decoded;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestToken"/> class.
        /// </summary>
        /// <param name="token">Base-64 encoded token string</param>
        public RequestToken(string token)
        {
            Token = token;
            _decoded = new Lazy<Tuple<DateTime, Guid, Guid>>(() => DecodeToken(token), true);
        }

        /// <summary>
        /// Gets the encoded token.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Gets the time that the request was received.
        /// </summary>
        public DateTime TimeStamp => _decoded.Value.Item1;

        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        public Guid ApplicationId => _decoded.Value.Item2;

        /// <summary>
        /// Gets the globally unique request ID.
        /// </summary>
        public Guid RequestId => _decoded.Value.Item3;

        public override string ToString() => Token;

        private static Tuple<DateTime, Guid, Guid> DecodeToken(string token)
        {
            const int GuidLen = 16;
            const int ExpectedLength = 40; // 8 + 16 + 16
            
            var encoded = Convert.FromBase64String(token);

            if (encoded?.Length != ExpectedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(encoded), $"Cannot parse the given byte array: expected an array of length {ExpectedLength}, actual length is {encoded?.Length}");
            }

            using (var ms = new MemoryStream(encoded))
            using (var br = new BinaryReader(ms))
            {
                return new Tuple<DateTime, Guid, Guid>(
                    new DateTime(br.ReadInt64()),
                    new Guid(br.ReadBytes(GuidLen)),
                    new Guid(br.ReadBytes(GuidLen)));
            }
        }
    }
}
