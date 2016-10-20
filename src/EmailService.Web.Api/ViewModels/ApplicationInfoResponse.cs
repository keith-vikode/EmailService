using System;

namespace EmailService.Web.Api.ViewModels
{
    public class ApplicationInfoResponse
    {
        public Guid ApplicationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
