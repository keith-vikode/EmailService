using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using static EmailService.Core.Constants;

namespace EmailService.Web.Controllers.Api
{
    [Authorize(ActiveAuthenticationSchemes = ApiKey)]
    public class ApiControllerBase : Controller
    {
        protected Guid GetApplicationId()
        {
            Guid appId = Guid.Empty;
            if (User.Identity.IsAuthenticated &&
                User.Identity.AuthenticationType == ApiKey)
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == ApplicationId);
                Guid.TryParse(claim?.Value, out appId);
            }

            return appId;
        }
    }
}
