using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core.Entities
{
    public class ApplicationTransport
    {
        public Guid ApplicationId { get; set; }

        public Guid TransportId { get; set; }

        public Application Application { get; set; }

        public Transport Transport { get; set; }

        [Required]
        [Range(0, 999)]
        public int Priority { get; set; } = 0;
    }
}
