using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core
{
    public class EmailQueueToken
    {
        private EmailQueueToken()
        {
        }

        public EmailQueueToken(Guid applicationId, Guid requestId, DateTime timeStamp)
        {
            ApplicationId = applicationId;
            RequestId = requestId;
            TimeStamp = timeStamp;
        }

        public Guid ApplicationId { get; private set; }

        public Guid RequestId { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public static EmailQueueToken Create(Guid applicationId)
        {
            return new EmailQueueToken
            {
                ApplicationId = applicationId,
                RequestId = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow
            };
        }

        public static EmailQueueToken DecodeBytes(byte[] encoded)
        {
            const int GuidLen = 16;
            const int ExpectedLength = 40; // 8 + 16 + 16
            if (encoded?.Length != ExpectedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(encoded), $"Cannot de-serialize an EmailQueueToken from the given byte array: expected an array of length {ExpectedLength}, actual length is {encoded?.Length}");
            }

            using (var ms = new MemoryStream(encoded))
            using (var br = new BinaryReader(ms))
            {
                return new EmailQueueToken
                {
                    TimeStamp = new DateTime(br.ReadInt64()),
                    ApplicationId = new Guid(br.ReadBytes(GuidLen)),
                    RequestId = new Guid(br.ReadBytes(GuidLen))
                };
            }
        }

        public static EmailQueueToken DecodeString(string encoded)
        {
            return DecodeBytes(Convert.FromBase64String(encoded));
        }

        public byte[] EncodeBytes()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(TimeStamp.Ticks);
                bw.Write(ApplicationId.ToByteArray());
                bw.Write(RequestId.ToByteArray());
                return ms.ToArray();
            }
        }

        public string EncodeString()
        {
            return Convert.ToBase64String(EncodeBytes());
        }

        public override string ToString()
        {
            return $"{ApplicationId}:{TimeStamp:s}:{RequestId}";
        }
    }
}
