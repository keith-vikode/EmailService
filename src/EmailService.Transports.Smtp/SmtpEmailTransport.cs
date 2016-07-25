using EmailService.Core;
using EmailService.Core.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace EmailService.Transports.Smtp
{
    public class SmtpEmailTransport : IEmailTransport
    {
        private readonly SmtpOptions _options;
        private readonly ILogger _logger;

        private readonly MailboxAddress _sender;
        private readonly SecureSocketOptions _socketOptions;
        private readonly string _localDomain = "localhost";

        public SmtpEmailTransport(IOptions<SmtpOptions> options, ILoggerFactory loggerFactory)
        {
            _options = options.Value;
            _logger = loggerFactory?.CreateLogger<SmtpEmailTransport>();

            _sender = new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress);
            _socketOptions = options.Value.UseEncryption ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            Uri senderUri;
            if (Uri.TryCreate($"mailto:{options.Value.SenderAddress}", UriKind.Absolute, out senderUri))
            {
                _localDomain = senderUri.Host;
            }
        }

        public async Task SendAsync(SenderParams args)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(_sender);

            if (!string.IsNullOrEmpty(args.SenderEmail))
            {
                emailMessage.ReplyTo.Add(new MailboxAddress(args.SenderName, args.SenderEmail));
            }

            foreach (var address in args.To)
            {
                emailMessage.To.Add(new MailboxAddress(string.Empty, address));
            }

            foreach (var address in args.CC)
            {
                emailMessage.Cc.Add(new MailboxAddress(string.Empty, address));
            }

            emailMessage.Subject = args.Subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = args.Body;
            emailMessage.Body = builder.ToMessageBody();

            _logger?.LogInformation("Opening a new SMTP connection to {0}:{1} ({2})", _options.Host, _options.Port, _socketOptions);
            using (var client = new SmtpClient())
            {
                client.LocalDomain = _localDomain;
                await client.ConnectAsync(_options.Host, _options.Port, _socketOptions).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(_options.Username))
                {
                    await client.AuthenticateAsync(_options.Username, _options.Password).ConfigureAwait(false);
                }

                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
                _logger?.LogInformation("Email sent successfully to {0}", emailMessage.To.ToString());
            }
        }
    }
}
