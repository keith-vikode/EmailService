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
        private const string DefaultSender = "no-reply@" + DefaultDomain;
        private const string DefaultDomain = "localhost";

        // set these up as static to save allocating them for each request
        private static readonly Header AutoSubmittedHeader = new Header("Auto-Submitted", "auto-replied");
        private static readonly Header PrecedenceHeader = new Header("Precedence", "list");
        
        private readonly SmtpOptions _options;
        private readonly MailboxAddress _sender;
        private readonly SecureSocketOptions _socketOptions;
        private readonly string _localDomain = DefaultDomain;

        public SmtpEmailTransport(SmtpOptions options)
        {
            _options = options;

            var sender = options.SenderAddress ?? DefaultSender;

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

            // these should prevent auto-replies being sent back to the sender address
            // (e.g. out of office replies)
            emailMessage.Headers.Add(AutoSubmittedHeader);
            emailMessage.Headers.Add(PrecedenceHeader);

            // the address that the email is actually from (usually configured against
            // the application)
            emailMessage.From.Add(_sender);

            // if the client has supplied a sender address, use this as the reply-to
            //if (!string.IsNullOrEmpty(args.SenderAddress))
            //{
            //    emailMessage.ReplyTo.Add(new MailboxAddress(args.SenderName, args.SenderAddress));
            //}

            CopyAddresses(args.To, emailMessage.To);
            CopyAddresses(args.CC, emailMessage.Cc);
            CopyAddresses(args.Bcc, emailMessage.Bcc);

            // MimeKit doesn't like `null` subjects, but is fine with an empty string
            // SMTP RFC can accept emails with no subject, this is good otherwise we'd
            // have to have a resource file with appropriate translations for "no 
            // subject"!
            emailMessage.Subject = args.Subject ?? string.Empty;

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
