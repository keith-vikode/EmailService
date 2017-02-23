using EmailService.Core;
using EmailService.Web.Api.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Web.Api.Test
{
    public class MessagesTests : IClassFixture<ApiTestFixture>
    {
        private ApiTestFixture _fixture;

        public MessagesTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_Should_ReturnUnauthorized_IfInvalidUser()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetInvalidAuthHeader();

            // act
            var response = await _fixture.Client.GetAsync("v1/Messages");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Post_Should_ReturnToken()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                To = new List<string> { "bob@example.com" },
                Subject = "Hello",
                Body = "Hi bob"
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAndGetJsonAsync<PostMessageResponse>("v1/Messages", model);

            // assert
            var decoded = response.Decode();
            Assert.Equal(_fixture.TestApp.Id, decoded.ApplicationId);
            Assert.NotEqual(Guid.Empty, decoded.RequestId);
        }

        [Fact]
        public async Task Post_Should_ReturnHttpAccepted()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                To = new List<string> { "bob@example.com" },
                Subject = "Hello",
                Body = "Hi bob"
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAsync("v1/Messages", model);

            // assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task Post_Should_ReturnBadRequest_IfNoRecipients()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                Body = "This won't work"
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAsync("v1/Messages", model);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Should_ReturnBadRequest_IfRecipientInvalid()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                To = new List<string> { "this won't work" },
                Body = "Hello world"
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAsync("v1/Messages", model);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Should_EnqueueMessage()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                To = new List<string> { "bob@example.com" },
                Subject = "Hello",
                Body = "Hi bob"
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAndGetJsonAsync<PostMessageResponse>("v1/Messages", model);

            // assert
            var decoded = response.Decode();
            Assert.Contains(Stubs.InMemoryEmailQueue.Queue, m => m.Token.RequestId == decoded.RequestId);
            Assert.Contains(Stubs.InMemoryEmailQueueBlobStore.Blobs, b => b.Key.RequestId == decoded.RequestId);
            Assert.Contains(Stubs.InMemoryEmailQueueBlobStore.Blobs, b => b.Value.ApplicationId == _fixture.TestApp.Id);
        }

        [Fact]
        public async Task Post_Should_LogMessage()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                To = new List<string> { "bob@example.com" },
                Subject = "Hello",
                Body = "Hi bob"
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAndGetJsonAsync<PostMessageResponse>("v1/Messages", model);

            // assert
            var decoded = response.Decode();
            Assert.Contains(Stubs.InMemoryEmailLog.ProcessingLog, m =>
                m.Token.RequestId == decoded.RequestId &&
                m.RetryCount == 0 &&
                m.Status == ProcessingStatus.Pending &&
                m.Token.TimeStamp == decoded.TimeStamp);
        }

        [Fact]
        public async Task Post_Should_SerializeDataParams()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var model = new PostEmailRequest
            {
                To = new List<string> { "bob@example.com" },
                Template = _fixture.WelcomeTemplate.Id,
                Data = JsonConvert.SerializeObject(new { Name = "Bob" })
            }.ToFormContent();

            // act
            var response = await _fixture.Client.PostAndGetJsonAsync<PostMessageResponse>("v1/Messages", model);

            // assert
            var decoded = response.Decode();
            Assert.Contains(Stubs.InMemoryEmailQueueBlobStore.Blobs, b =>
                b.Key.RequestId == decoded.RequestId &&
                b.Value.Data?.ContainsKey("Name") == true);
        }

        private class PostMessageResponse
        {
            public string Token { get; set; }

            public EmailQueueToken Decode() => EmailQueueToken.DecodeString(Token);
        }
    }
}
