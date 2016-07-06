using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core
{
    public class Template
    {
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public byte[] ConcurrencyToken { get; set; }
    }
}
