using EmailService.Core;
using EmailService.Core.Abstraction;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailService.Transports
{
    public class SmtpEmailTransport : IEmailTransport
    {
        private readonly SmtpOptions _options;

        private readonly MailboxAddress _sender;
        private readonly SecureSocketOptions _socketOptions;
        private readonly string _localDomain = "localhost";

        public SmtpEmailTransport(SmtpOptions options)
        {
            _options = options;

            var sender = options.SenderAddress ?? "no-reply@localhost";

            _sender = new MailboxAddress(options.SenderName, sender);

            _socketOptions = options.UseEncryption ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            Uri senderUri;
            if (Uri.TryCreate($"mailto:{sender}", UriKind.Absolute, out senderUri))
            {
                _localDomain = senderUri.Host;
            }
        }

        public async Task<bool> SendAsync(SenderParams args)
        {
            var emailMessage = new MimeMessage();

            emailMessage.Headers.Add("Auto-Submitted", "auto-replied");
            emailMessage.Headers.Add("Precedence", "list");

            emailMessage.From.Add(_sender);

            if (!string.IsNullOrEmpty(args.SenderAddress))
            {
                emailMessage.ReplyTo.Add(new MailboxAddress(args.SenderName, args.SenderAddress));
            }

            CopyAddresses(args.To, emailMessage.To);
            CopyAddresses(args.CC, emailMessage.Cc);
            CopyAddresses(args.Bcc, emailMessage.Bcc);

            emailMessage.Subject = args.Subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = args.Body;
            emailMessage.Body = builder.ToMessageBody();

            try
            {
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
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void CopyAddresses(IEnumerable<string> input, InternetAddressList target)
        {
            foreach (var address in input)
            {
                target.Add(new MailboxAddress(string.Empty, address));
            }
        }
    }
}
