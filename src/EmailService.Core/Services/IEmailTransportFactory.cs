using EmailService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    public interface IEmailTransportFactory
    {
        IEmailTransport CreateTransport(Transport definition);
    }
}
