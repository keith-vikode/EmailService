using System;

namespace EmailService.Web.Api.ViewModels
{
    public class PostEmailResponse
    {
        public PostEmailResponse(Guid requestId)
        {
            RequestId = requestId;
        }

        /// <summary>
        /// The unique ID of the request that can be used to track the progress of this email.
        /// </summary>
        public Guid RequestId { get; }
    }
}
