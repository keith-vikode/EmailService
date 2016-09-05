using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Core.Test
{
    public class EmailQueueTokenTests
    {
        private const int ExpectedByteLength = 40;
        private static readonly Guid Application1 = Guid.NewGuid();

        [Fact]
        public void EncodeBytes_ShouldReturnByteArrayOfCorrectLength()
        {
            // arrange
            int expected = ExpectedByteLength;
            int actual;
            var token = EmailQueueToken.Create(Application1);

            // act
            actual = token.EncodeBytes().Length;

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DecodeBytes_ShouldThrowArgumentException_IfByteArrayIsNotCorrectLength()
        {
            // arrange
            var bytes = new byte[8] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 };

            // act/assert
            Assert.Throws<ArgumentOutOfRangeException>(() => EmailQueueToken.DecodeBytes(bytes));
        }

        [Fact]
        public void DecodeBytes_ShouldDeserializeValidByteArray()
        {
            // arrange
            var original = EmailQueueToken.Create(Application1);
            var bytes = original.EncodeBytes();

            // act
            var decoded = EmailQueueToken.DecodeBytes(bytes);

            // assert
            Assert.Equal(original.TimeStamp, decoded.TimeStamp);
            Assert.Equal(original.RequestId, decoded.RequestId);
        }

        [Fact]
        public void DecodeBytes_ShouldDeserializeValidString()
        {
            // arrange
            var original = EmailQueueToken.Create(Application1);
            var base64 = original.EncodeString();

            // act
            var decoded = EmailQueueToken.DecodeString(base64);

            // assert
            Assert.Equal(original.TimeStamp, decoded.TimeStamp);
            Assert.Equal(original.ApplicationId, decoded.ApplicationId);
            Assert.Equal(original.RequestId, decoded.RequestId);
        }
    }
}
