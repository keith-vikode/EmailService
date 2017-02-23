using EmailService.Web.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Web.Api.Test
{
    public class TemplatesTests : IClassFixture<ApiTestFixture>
    {
        private ApiTestFixture _fixture;

        public TemplatesTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task Get_Should_ReturnUnauthorized_IfInvalidUser()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetInvalidAuthHeader();

            // act
            var response = await _fixture.Client.GetAsync("v1/Templates");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_Should_ReturnActiveTemplates()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);

            // act
            var model = await _fixture.Client.GetJsonAsync<List<TemplateListingViewModel>>("v1/Templates");

            // assert
            Assert.Contains(model, m => m.Name == "Welcome!");
            Assert.Contains(model, m => m.Translations.Contains("fr-FR"));
        }

        [Fact]
        public async Task Get_ShouldNot_ReturnInctiveTemplates()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);

            // act
            var model = await _fixture.Client.GetJsonAsync<List<TemplateListingViewModel>>("v1/Templates");

            // assert
            Assert.DoesNotContain(model, m => m.Name == "Inactive");
        }

        [Fact]
        public async Task Transform_Should_ReturnNotFoundForInvalidTemplate()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var url = $"v1/Templates/{Guid.NewGuid()}/Transform";
            var model = new { }.ToJsonContent();

            // act
            var response = await _fixture.Client.PostAsync(url, model);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Transform_Should_ReturnBadRequest_IfLanguageInvalid()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var url = $"v1/Templates/{_fixture.WelcomeTemplate.Id}/Transform/aaaaaaaaaaaaaaa";
            var model = new { Name = "Keith" }.ToJsonContent();

            // act
            var response = await _fixture.Client.PostAsync(url, model);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("", "Welcome Keith")]
        [InlineData("fr-FR", "Bonjour Keith")] // translation available
        [InlineData("hi-IN", "Welcome Keith")] // fallback to default
        public async Task Transform_Should_ReturnTransformedContent_InSpecifiedLanguage(string language, string expected)
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var url = $"v1/Templates/{_fixture.WelcomeTemplate.Id}/Transform/{language}";
            var model = new { Name = "Keith" }.ToJsonContent();

            // act
            var response = await _fixture.Client.PostAndGetJsonAsync<TransformResponse>(url, model);

            // assert
            Assert.Equal(expected, response.Subject);
            Assert.Equal(expected, response.Body);
        }

        [Fact]
        public async Task Transform_Should_ReturnUntransformedContent_IfDataIsOmitted()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);
            var url = $"v1/Templates/{_fixture.WelcomeTemplate.Id}/Transform";
            var model = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var expected = "Welcome {{Name}}";

            // act
            var response = await _fixture.Client.PostAndGetJsonAsync<TransformResponse>(url, model);

            // assert
            Assert.Equal(expected, response.Subject);
            Assert.Equal(expected, response.Body);
        }

        private class TransformResponse
        {
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Language { get; set; }
        }
    }
}
