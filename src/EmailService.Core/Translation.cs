using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core
{
    public class Translation
    {
        public CultureInfo Language { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
