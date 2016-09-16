using System;
using System.ComponentModel.DataAnnotations;

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
