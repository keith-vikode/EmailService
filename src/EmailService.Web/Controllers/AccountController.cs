using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Web.Controllers
{
    public class AccountController : Controller
    {
        [Authorize(ActiveAuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action("SignedOut", "Account", values: null, protocol: Request.Scheme);
            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult SignedOut() => View();
    }
}
