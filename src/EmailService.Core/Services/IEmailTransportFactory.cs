using EmailService.Core.Abstraction;

namespace EmailService.Core.Services
{
    public interface IEmailTransportFactory
    {
        IEmailTransport CreateTransport(ITransportDefinition definition);
    }
}
