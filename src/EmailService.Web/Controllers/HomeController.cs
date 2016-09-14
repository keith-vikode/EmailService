using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EmailService.Web.ViewModels;

namespace EmailService.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Error(int? id = null)
        {
            if (id.HasValue)
            {
                StatusErrorViewModel model = GetStatusModel(id.Value);
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
