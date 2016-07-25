using EmailService.Core.Services;
using Moq;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Core.Test
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class EmailSenderTests
    {
        // mocks
        private Mock<IEmailTransport> mockSender = new Mock<IEmailTransport>(MockBehavior.Loose);
        private Mock<ITemplateStore> mockTemplateStore = new Mock<ITemplateStore>(MockBehavior.Loose);
        private Mock<ITemplateTransformer> mockTemplateTransformer = new Mock<ITemplateTransformer>(MockBehavior.Loose);

        // test object(s)
        private EmailSender _target;
        private EmailTemplate _testTemplate = new EmailTemplate("Test", "Test.");

        public EmailSenderTests()
        {
            // default mock behaviour
            mockSender.Setup(m => m.SendAsync(It.IsAny<SenderParams>())).Returns(Task.FromResult(EmailSenderResult.Success));
            mockTemplateStore.Setup(m => m.LoadTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>())).Returns(Task.FromResult(_testTemplate));
            mockTemplateTransformer.Setup(m => m.TransformTemplateAsync(It.IsAny<EmailTemplate>(), It.IsAny<object>())).Returns(Task.FromResult(_testTemplate));

            _target = new EmailSender(mockSender.Object, mockTemplateStore.Object, mockTemplateTransformer.Object);
        }

        [Fact]
        public void SendEmailAsync_If_EmptyParams_ShouldThrowArgumentException()
        {
            var args = new EmailParams(); // empty arguments that can't be processed
			Assert.ThrowsAsync<ArgumentException>(() => _target.SendEmailAsync(args));
        }

        [Fact]
        public async Task SendEmailAsync_If_SentSuccessfully_ShouldReturnSuccess()
        {
            var args = new EmailParams("test@test.com", "test_template");

            var result = await _target.SendEmailAsync(args);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SendEmailAsync_If_UnhandledException_ShouldReturnFault()
        {
            var args = new EmailParams("test@test.com", "test_template");

            mockSender.Setup(m => m.SendAsync(It.IsAny<SenderParams>())).Throws<InvalidOperationException>();

            var result = await _target.SendEmailAsync(args);

            Assert.False(result.Succeeded);
            Assert.Equal(result.ErrorCode, EmailSenderResult.ErrorCodes.Unhandled);
        }

        [Fact]
        public async Task SendEmailAsync_If_TemplateNotFound_ShouldReturnsError()
        {
            var args = new EmailParams("test@test.com", "test_template");

            // setup the template store to return nothing for the given template
            EmailTemplate nullTemplate = null;
            mockTemplateStore.Setup(m => m.LoadTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>()))
                .Returns(Task.FromResult(nullTemplate));

            var result = await _target.SendEmailAsync(args);

            Assert.False(result.Succeeded);
            Assert.Equal(result.ErrorCode, EmailSenderResult.ErrorCodes.TemplateNotFound);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldCallTheTransformer()
        {
            var args = new EmailParams("test@test.com", "test_template");

            var template = new EmailTemplate("Test {{Number}}", "<p>Test {{Number}}</p>");
            var transformed = new EmailTemplate("Test 123", "<p>Test 123</p>");

            mockTemplateStore.Setup(m => m.LoadTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>()))
                .Returns(Task.FromResult(template));
            mockTemplateTransformer.Setup(m => m.TransformTemplateAsync(It.IsAny<EmailTemplate>(), It.IsAny<object>()))
                .Returns(Task.FromResult(transformed));

            var result = await _target.SendEmailAsync(args);

            // verify that our 'transformed' email template was actually sent
            var predicate = new Func<SenderParams, bool>(p => p.Subject == transformed.Subject && p.Body == transformed.Body);
            mockSender.Verify(m => m.SendAsync(It.Is<SenderParams>(e => predicate(e))), Times.Once());
        }
    }
}
