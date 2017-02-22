using EmailService.Core.Abstraction;
using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Core.Templating;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Core.Test
{
    public class QueueProcessorTests : IClassFixture<EmailSenderTestFixture>
    {
        private EmailSenderTestFixture _fixture;

        // TODO: mock queue receiver and blob store
        private Mock<IEmailTransport> _transport;
        private Mock<IEmailTransportFactory> _transportFactory;
        private Mock<IEmailLogWriter> _logWriter;
        
        private QueueProcessor<TestMessage> _target;

        public QueueProcessorTests(EmailSenderTestFixture fixture)
        {
            _fixture = fixture;
            
            _transport = new Mock<IEmailTransport>(MockBehavior.Loose);
            _transport.Setup(m => m.SendAsync(It.IsAny<SenderParams>()))
                .Returns(Task.FromResult(true));

            _transportFactory = new Mock<IEmailTransportFactory>();
            _transportFactory.Setup(m => m.CreateTransport(It.IsAny<Transport>()))
                .Returns(_transport.Object);

            _logWriter = new Mock<IEmailLogWriter>(MockBehavior.Loose);
            
            _target = new QueueProcessor<TestMessage>(
                _fixture.Database,
                null,
                null,
                _transportFactory.Object,
                _logWriter.Object,
                _fixture.LoggerFactory);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldReturnTrue_IfSuccess()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                TemplateId = _fixture.TemplateId,
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };

            // act
            var outcome = await _target.TrySendEmailAsync(args);

            // assert
            Assert.NotNull(outcome);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowInvalidOperation_IfTemplateNotFound()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                TemplateId = Guid.NewGuid(),
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };

            // act/assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _target.TrySendEmailAsync(args));
        }

        [Fact]
        public async Task SendEmailAsync_ThrowsInvalidOperation_IfApplicationNotFound()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = Guid.NewGuid(),
                TemplateId = _fixture.TemplateId,
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };

            // act/assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _target.TrySendEmailAsync(args));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldUseTemplate_IfTemplateIdProvided()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                TemplateId = _fixture.TemplateId,
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };
            
            var template = await _fixture.Database.FindTemplateAsync(_fixture.TemplateId);
            var expected = await MustacheTemplateTransformer.Instance.TransformTemplateAsync(
                new EmailTemplate(
                    template.SubjectTemplate,
                    template.BodyTemplate),
                args.Data,
                args.GetCulture());
            
            // act
            var success = await _target.TrySendEmailAsync(args);

            // assert
            _transport.Verify(m => m.SendAsync(It.Is<SenderParams>(p =>
                p.Subject == expected.Subject && p.Body == expected.Body)));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldUseTranslation_IfLanguageProvided()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                TemplateId = _fixture.TemplateId,
                Culture = _fixture.OtherCulture,
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };

            var template = await _fixture.Database.FindTemplateAsync(_fixture.TemplateId);
            var translation = template.Translations.FirstOrDefault(t => t.Language == args.Culture);
            var expected = await MustacheTemplateTransformer.Instance.TransformTemplateAsync(
                new EmailTemplate(
                    translation.SubjectTemplate,
                    translation.BodyTemplate),
                args.Data,
                args.GetCulture());

            // act
            var success = await _target.TrySendEmailAsync(args);

            // assert
            _transport.Verify(m => m.SendAsync(It.Is<SenderParams>(p =>
                p.Subject == expected.Subject && p.Body == expected.Body)));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldUseBaseTemplate_IfInvalidCultureProvided()
        {
            // arrange
            var args = new EmailMessageParams
            {
                Culture = "zu-ZA",
                ApplicationId = _fixture.ApplicationId,
                TemplateId = _fixture.TemplateId,
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };

            var template = await _fixture.Database.FindTemplateAsync(_fixture.TemplateId);
            var expected = await MustacheTemplateTransformer.Instance.TransformTemplateAsync(
                new EmailTemplate(
                    template.SubjectTemplate,
                    template.BodyTemplate),
                args.Data,
                args.GetCulture());

            // act
            var success = await _target.TrySendEmailAsync(args);

            // assert
            _transport.Verify(m => m.SendAsync(It.Is<SenderParams>(p =>
                p.Subject == expected.Subject && p.Body == expected.Body)));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldUseSubjectAndBody_IfNoTemplateIdProvided()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                To = new List<string> { "someone@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } },
                Subject = "Hi {{Name}}",
                BodyEncoded = EmailMessageParams.EncodeBody("Hi {{Name}}")
            };

            var template = await _fixture.Database.FindTemplateAsync(_fixture.TemplateId);
            var expected = await MustacheTemplateTransformer.Instance.TransformTemplateAsync(
                new EmailTemplate(
                    args.Subject,
                    args.GetBody()),
                args.Data,
                args.GetCulture());

            // act
            var success = await _target.TrySendEmailAsync(args);

            // assert
            _transport.Verify(m => m.SendAsync(It.Is<SenderParams>(p =>
                p.Subject == expected.Subject && p.Body == expected.Body)));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldUseRawSubjectAndBody_IfNoTemplateIdAndNoDataProvided()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                To = new List<string> { "someone@example.com" },
                Subject = "Hi Bob",
                BodyEncoded = EmailMessageParams.EncodeBody("Hi Bob")
            };

            var template = await _fixture.Database.FindTemplateAsync(_fixture.TemplateId);
            var expected = new EmailTemplate(args.Subject, args.Subject);

            // act
            var success = await _target.TrySendEmailAsync(args);

            // assert
            _transport.Verify(m => m.SendAsync(It.Is<SenderParams>(p =>
                p.Subject == expected.Subject && p.Body == expected.Body)));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendToAllRecipients()
        {
            // arrange
            var args = new EmailMessageParams
            {
                ApplicationId = _fixture.ApplicationId,
                TemplateId = _fixture.TemplateId,
                To = new List<string> { "one@example.com", "two@example.com" },
                CC = new List<string> { "three@example.com", "four@example.com" },
                Bcc = new List<string> { "five@example.com", "six@example.com" },
                Data = new Dictionary<string, object> { { "Name", "Someone" } }
            };

            List<string> to = null;
            List<string> cc = null;
            List<string> bcc = null;

            _transport
                .Setup(m => m.SendAsync(It.IsAny<SenderParams>()))
                .ReturnsAsync(true)
                .Callback<SenderParams>(p =>
                {
                    to = new List<string>(p.To);
                    cc = new List<string>(p.CC);
                    bcc = new List<string>(p.Bcc);
                });

            // act
            var success = await _target.TrySendEmailAsync(args);

            // assert
            Assert.Equal(args.To, to);
            Assert.Equal(args.CC, cc);
            Assert.Equal(args.Bcc, bcc);
        }
    }

    public class TestMessage : IEmailQueueMessage
    {
        public int DequeueCount { get; set; }

        public EmailQueueToken Token { get; set; }
    }

    public class EmailSenderTestFixture : IDisposable
    {
        public EmailSenderTestFixture()
        {
            var builder = new DbContextOptionsBuilder<EmailServiceContext>();
            builder.UseInMemoryDatabase();
            Database = new EmailServiceContext(builder.Options);

            SetupEntities();
        }

        public ILoggerFactory LoggerFactory => new LoggerFactory();

        public Guid ApplicationId { get; private set; }

        public Guid TemplateId { get; private set; }

        public string OtherCulture { get; private set; }

        public EmailServiceContext Database { get; }

        public void Dispose()
        {
            Database?.Dispose();
        }

        private void SetupEntities()
        {
            var localhost = new Transport
            {
                Type = TransportType.Smtp,
                Hostname = "localhost",
                SenderAddress = "localhost@localhost"
            };

            Database.Transports.Add(localhost);
            
            var app = new Application
            {
                Name = "Test application",
                PrimaryApiKey = new byte[] { 0x1, 0x2 },
                SecondaryApiKey = new byte[] { 0x1, 0x2 },
                SenderAddress = "sender@example.com"
            };

            ApplicationId = app.Id;
            app.Transports.Add(new ApplicationTransport { ApplicationId = app.Id, TransportId = localhost.Id, Priority = 0 });

            Database.Applications.Add(app);

            var template = new Template
            {
                ApplicationId = app.Id,
                SubjectTemplate = "Hello {{Name}}",
                BodyTemplate = "<p>Hello {{Name}}</p>",
                SampleData = "{ \"Name\": \"Bob\" }",
                UseHtml = true
            };

            TemplateId = template.Id;

            OtherCulture = "fr-FR";
            template.Translations.Add(new Translation
            {
                Language = OtherCulture,
                SubjectTemplate = "Bonjour {{Name}}",
                BodyTemplate = "<p>Bonjour {{Name}}</p>"
            });

            Database.Templates.Add(template);

            Database.SaveChanges();
        }
    }
}
