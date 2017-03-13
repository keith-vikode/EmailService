using Microsoft.AspNetCore.Mvc;
using EmailService.Web.ViewModels;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using EmailService.Core.Entities;
using EmailService.Web.ViewModels.Home;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly EmailServiceContext _ctx;

        public HomeController(EmailServiceContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> Index()
        {
            var model = await IndexViewModel.LoadAsync(_ctx);
            return View(model);
        }
        
        public IActionResult Error(int? id = null)
        {
            if (id.HasValue)
            {
                var model = GetStatusModel(id.Value);
                return View(model);
            }
            else
            {
                return View(StatusErrorViewModel.Default);
            }
        }

        private StatusErrorViewModel GetStatusModel(int value)
        {
            switch (value)
            {
                case 400:
                    return StatusErrorViewModel.BadRequst;
                case 401:
                    return StatusErrorViewModel.Unauthorized;
                case 404:
                    return StatusErrorViewModel.NotFound;
                case 410:
                    return StatusErrorViewModel.Gone;
                default:
                    return StatusErrorViewModel.Default;
            }
        }
    }
}
