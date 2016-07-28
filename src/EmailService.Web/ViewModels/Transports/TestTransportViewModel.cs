using System;
using System.ComponentModel.DataAnnotations;

namespace EmailService.Web.ViewModels.Transports
{
    public class TestTransportViewModel
    {
        public Guid TransportId { get; set; }

        public string TransportName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; } = "This is a test email from the Postbox admin console.";
    }
}
