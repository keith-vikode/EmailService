using EmailService.Web.Api.ViewModels;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Web.Api.Test
{
    [Collection(ApiCollection.Name)]
    public class ApplicationTests
    {
        private ApiTestFixture _fixture;

        public ApplicationTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task Get_Should_ReturnApplicationDetails_IfValidUser()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetValidAuthHeader(_fixture.TestApp);

            // act
            var model = await _fixture.Client.GetJsonAsync<ApplicationInfoResponse>("v1/Application");

            // assert
            Assert.Equal(_fixture.TestApp.Id, model.ApplicationId);
            Assert.Equal(_fixture.TestApp.Name, model.Name);
            Assert.Equal(_fixture.TestApp.Description, model.Description);
        }

        [Fact]
        public async Task Get_Should_ReturnUnauthorized_IfInvalidUser()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Authorization = _fixture.GetInvalidAuthHeader();

            // act
            var response = await _fixture.Client.GetAsync("v1/Application");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_Should_ReturnUnauthorized_IfNoAuthorizationHeader()
        {
            // arrange
            _fixture.Client.DefaultRequestHeaders.Clear();

            // act
            var response = await _fixture.Client.GetAsync("v1/Application");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
