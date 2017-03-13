using EmailService.Core.Templating;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Core.Test.Templating
{
    public class MustacheTemplateTransformerTests
    {
        private ITemplateTransformer _target = MustacheTemplateTransformer.Instance;

        [Fact]
        public async Task TransformText_ShouldUseData()
        {
            // arrange
            var data = new { Name = "Keith" };
            var text = "Hello {{Name}}";

            // act
            var transformed = await _target.TransformTextAsync(text, data, CultureInfo.InvariantCulture);

            // assert
            Assert.Equal("Hello Keith", transformed);
        }

        [Fact]
        public async Task TransformText_ShouldUseJToken()
        {
            // arrange
            var data = JToken.Parse("{ \"Name\": \"Keith\" }");
            var text = "Hello {{Name}}";

            // act
            var transformed = await _target.TransformTextAsync(text, data, CultureInfo.InvariantCulture);

            // assert
            Assert.Equal("Hello Keith", transformed);
        }

        [Theory]
        [InlineData("en-GB", "£2.99")]
        [InlineData("en-US", "$2.99")]
        public async Task TransformText_ShouldUseCorrectCulture(string culture, string expected)
        {
            // arrange
            var data = new { Value = 2.99 };
            var text = "{{Value:C}}";
            var formatter = new CultureInfo(culture);

            // act
            var transformed = await _target.TransformTextAsync(text, data, formatter);

            // assert
            Assert.Equal(expected, transformed);
        }
    }
}
