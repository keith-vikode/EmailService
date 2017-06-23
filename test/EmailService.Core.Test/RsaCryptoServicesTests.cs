using EmailService.Core.Services;
using System;
using Xunit;

namespace EmailService.Core.Test
{
    public class RsaCryptoServicesTests
    {
        private ICryptoServices _target = RsaCryptoServices.Instance;

        [Fact]
        public void GenerateKey_ShouldReturnByteArrayOfExpectedLength()
        {
            // arrange
            var expected = 867; // length of a 1024-bit key

            // act
            var actual = _target.GeneratePrivateKey().Length;

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void VerifyPublicKey_ShouldReturnTrue_IfValid()
        {
            // arrange
            Guid appId = Guid.NewGuid();
            byte[] privateKey = _target.GeneratePrivateKey();
            string apiKey = _target.GetApiKey(appId, privateKey);

            // act
            var verified = _target.VerifyApiKey(appId, apiKey, privateKey);

            // assert
            Assert.True(verified);
        }

        [Fact]
        public void VerifyPublicKey_ShouldReturnFalse_ForAnyOtherAppId()
        {
            // arrange
            Guid appId = Guid.NewGuid();
            Guid otherAppId = Guid.NewGuid();
            byte[] privateKey = _target.GeneratePrivateKey();
            string apiKey = _target.GetApiKey(appId, privateKey);

            // act
            var verified = _target.VerifyApiKey(otherAppId, apiKey, privateKey);

            // assert
            Assert.False(verified);
        }

        [Fact]
        public void VerifyPublicKey_ShouldReturnFalse_ForAnyOtherApiKey()
        {
            // arrange
            Guid appId = Guid.NewGuid();
            byte[] privateKey1 = _target.GeneratePrivateKey();
            byte[] privateKey2 = _target.GeneratePrivateKey();
            string apiKey = _target.GetApiKey(appId, privateKey1);

            // act
            var verified = _target.VerifyApiKey(appId, apiKey, privateKey2);

            // assert
            Assert.False(verified);
        }

        [Fact]
        public void VerifyPublicKey_ShouldReturnFalse_ForInvalidSignature()
        {
            // arrange
            Guid appId = Guid.NewGuid();
            byte[] privateKey = _target.GeneratePrivateKey();
            string apiKey = "random gibberish";

            // act
            var verified = _target.VerifyApiKey(appId, apiKey, privateKey);

            // assert
            Assert.False(verified);
        }
    }
}
