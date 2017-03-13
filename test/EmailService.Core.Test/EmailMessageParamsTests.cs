using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit;

namespace EmailService.Core.Test
{
    public class EmailMessageParamsTests
    {
        [Fact]
        public void ToJson_ShouldSerializeDataAsObject()
        {
            // arrange
            var data = new Dictionary<string, object> { { "Name", "Keith" } };
            var expected = JsonConvert.SerializeObject(data, Formatting.None);
            var target = new EmailMessageParams { Data = data };

            // act
            var actual = EmailMessageParams.ToJson(target, Formatting.None);

            // assert
            Assert.Contains(expected, actual);
        }

        [Fact]
        public void ToJson_ShouldSerializeComplexDataAsObject()
        {
            // arrange
            var data = new Dictionary<string, object>
            {
                { "Name", "Keith" },
                { "Roles", new string[] { "Developer", "Manager" } },
                { "Computer", new
                {
                    Make = "Microsoft",
                    Model = "Surface Book"
                } }
            };
            var expected = JsonConvert.SerializeObject(data, Formatting.None);
            var target = new EmailMessageParams { Data = data };

            // act
            var actual = EmailMessageParams.ToJson(target, Formatting.None);

            // assert
            Assert.Contains(expected, actual);
        }

        [Fact]
        public void ToJson_ShouldDeserializeDataObject()
        {
            // arrange
            var data = new Dictionary<string, object> { { "Name", "Keith" } };
            var target = new EmailMessageParams { Data = data };

            // act
            var serialized = EmailMessageParams.ToJson(target);
            var deserialized = EmailMessageParams.FromJson(serialized);

            // assert
            Assert.Equal(data, deserialized.Data);
        }
    }
}
