using System;

namespace EmailService.Core.Abstraction
{
    public interface ITransportDefinition
    {
        Guid Id { get; }
        bool IsActive { get; }
        string Hostname { get; }
        string Name { get; }
        string Password { get; }
        short? PortNum { get; }
        string SenderAddress { get; }
        string SenderName { get; }
        TransportType Type { get; }
        string Username { get; }
        bool UseSSL { get; }
    }
}