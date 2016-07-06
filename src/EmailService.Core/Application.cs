using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core
{
    public class Application
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public byte[] ConcurrencyToken { get; set; }
    }
}
