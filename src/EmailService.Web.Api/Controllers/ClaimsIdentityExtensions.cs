using System;
using System.Security.Claims;

namespace EmailService.Web.Api.Controllers
{
    public static class ClaimsIdentityExtensions
    {
        public static Guid GetApplicationId(this ClaimsPrincipal user)
        {
            Guid appId = Guid.Empty;
            if (user.Identity.IsAuthenticated)
            {
                var claim = user.FindFirst(ClaimTypes.NameIdentifier);
                Guid.TryParse(claim?.Value, out appId);
            }

            return appId;
        }
    }
}
