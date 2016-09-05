using EmailService.Core;
using EmailService.Core.Abstraction;
using EmailService.Core.Services;
using System;

namespace EmailService.Transports
{
    public class EmailTransportFactory : IEmailTransportFactory
    {
        private static readonly Lazy<EmailTransportFactory> _Instance =
            new Lazy<EmailTransportFactory>(() => new EmailTransportFactory(), true);

        private EmailTransportFactory()
        {
        }

        public static EmailTransportFactory Instance => _Instance.Value;

        public IEmailTransport CreateTransport(ITransportDefinition transport)
        {
            switch (transport.Type)
            {
                case TransportType.SendGrid:
                    return new SendGridEmailTransport(new SendGridOptions
                    {
                        Enabled = transport.IsActive,
                        ApiKey = transport.Password,
                        SenderAddress = transport.SenderAddress,
                        SenderName = transport.SenderName
                    });
                case TransportType.Smtp:
                    return new SmtpEmailTransport(new SmtpOptions
                    {
                        Enabled = transport.IsActive,
                        Host = transport.Hostname,
                        Port = (ushort)(transport.PortNum ?? 25),
                        Username = transport.Username,
                        Password = transport.Password,
                        UseEncryption = transport.UseSSL,
                        SenderAddress = transport.SenderAddress,
                        SenderName = transport.SenderName
                    });
                default:
                    return DebugSender.Instance;
            }
        }
    }
}
