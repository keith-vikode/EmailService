using Microsoft.AspNetCore.Builder;
using static EmailService.Core.Constants;

namespace EmailService.Web.Auth
{
    public class ApiKeyAuthenticationOptions : AuthenticationOptions
    {
        public ApiKeyAuthenticationOptions()
        {
            AuthenticationScheme = ApiKey;
        }
        
        public string AuthorizationHeader { get; set; } = Authorization;

        public string ApplicationIdClaim { get; set; } = ApplicationId;
    }
}
