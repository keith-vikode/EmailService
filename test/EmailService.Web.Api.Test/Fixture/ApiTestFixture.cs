using EmailService.Core;
using EmailService.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EmailService.Web.Api.Test
{
    public class ApiTestFixture
    {
        private TestServer _server;
        private DbContextOptions<EmailServiceContext> _contextOptions;

        public ApiTestFixture()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();
            _server = new TestServer(builder);

            _contextOptions = new DbContextOptionsBuilder<EmailServiceContext>()
                .UseInMemoryDatabase()
                .UseMemoryCache(null)
                .Options;

            Client = GetClient();
            Context = GetDbContext();

            SetupData(Context);
        }

        public HttpClient Client { get; }

        public EmailServiceContext Context { get; private set; }

        public Transport TestSmtp { get; private set; }

        public Application TestApp { get; private set; }

        public Template WelcomeTemplate { get; private set; }

        public Application CreateApplication(string name = "Test App", string description = "Test description")
        {
            var app = new Application { Name = name, Description = description, IsActive = true };
            app.PrimaryApiKey = RsaCryptoServices.Instance.GeneratePrivateKey();
            app.SecondaryApiKey = RsaCryptoServices.Instance.GeneratePrivateKey();
            return app;
        }

        public AuthenticationHeaderValue GetValidAuthHeader(Application app)
        {
            var apiKey = RsaCryptoServices.Instance.GetApiKey(app.Id, app.PrimaryApiKey);
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{app.Id}:{apiKey}"));
            return new AuthenticationHeaderValue("Basic", auth);
        }

        public AuthenticationHeaderValue GetInvalidAuthHeader()
        {
            var apiKey = Convert.ToBase64String(new byte[] { 0x1, 0x2, 0x3, 0x4 });
            var appId = Guid.NewGuid();
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appId}:{apiKey}"));
            return new AuthenticationHeaderValue("Basic", auth);
        }

        private HttpClient GetClient()
        {
            var client = _server.CreateClient();
            var ub = new UriBuilder(client.BaseAddress);
            ub.Port = 443;
            ub.Scheme = "https";
            client.BaseAddress = ub.Uri;
            return client;
        }

        private EmailServiceContext GetDbContext()
        {
            var ctx = new EmailServiceContext(_contextOptions);
            ctx.Database.EnsureDeleted();
            return ctx;
        }

        private void SetupData(EmailServiceContext ctx)
        {
            var app = CreateApplication();

            var smtp = new Transport
            {
                Type = TransportType.Smtp,
                PortNum = 25,
                Hostname = "smtp.example.com",
                Username = "smtpuser",
                Password = "smtppass",
                SenderAddress = "noreply@example.com",
                SenderName = "Example.com",
                Name = "Example SMTP"
            };

            app.Transports.Add(new ApplicationTransport { Transport = smtp });

            var welcome = new Template
            {
                Name = "Welcome!",
                Description = "Sent on signup",
                UseHtml = true,
                SubjectTemplate = "Welcome {{Name}}",
                BodyTemplate = "Welcome {{Name}}"
            };

            welcome.Translations.Add(new Translation
            {
                Language = "fr-FR",
                SubjectTemplate = "Bonjour {{Name}}",
                BodyTemplate = "Bonjour {{Name}}"
            });

            var inactive = new Template
            {
                Name = "Inactive",
                IsActive = false,
                SubjectTemplate = "Not active",
                BodyTemplate = "Not active"
            };

            app.Templates.Add(welcome);
            app.Templates.Add(inactive);

            ctx.Add(smtp);
            ctx.Add(app);
            ctx.SaveChanges();

            TestApp = app;
            WelcomeTemplate = welcome;
            TestSmtp = smtp;
        }
    }
}
