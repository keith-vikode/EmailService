using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Web.ViewModels;
using EmailService.Web.ViewModels.Applications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers
{
    public partial class ApplicationsController : Controller
    {
        private EmailServiceContext _ctx;

        public ApplicationsController(EmailServiceContext ctx)
        {
            _ctx = ctx;
        }
        
        public async Task<IActionResult> Index()
        {
            var model = await IndexViewModel.LoadAsync(_ctx);
            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var model = await ApplicationDetailsViewModel.LoadAsync(_ctx, id);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        public async Task<IActionResult> Create()
        {
            var model = new CreateApplicationViewModel();
            await model.LoadTransportAsync(_ctx);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateApplicationViewModel model, [FromServices] ICryptoServices crypto)
        {
            if (ModelState.IsValid)
            {
                var app = await model.SaveChangesAsync(_ctx, crypto);
                return RedirectToAction(nameof(Details), new { id = app.Id });
            }
            
            await model.LoadTransportAsync(_ctx);
            return View(model);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await EditApplicationViewModel.LoadAsync(_ctx, id);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, EditApplicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }

        public async Task<IActionResult> Deactivate(Guid id)
        {
            var app = await _ctx.FindApplicationAsync(id);
            if (app != null)
            {
                return View(new ActivationViewModel { Id = id, Name = app.Name, IsActive = false });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(Guid id, ActivationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }

        public async Task<IActionResult> Reactivate(Guid id)
        {
            var app = await _ctx.FindApplicationAsync(id);
            if (app != null)
            {
                return View(new ActivationViewModel { Id = id, Name = app.Name, IsActive = true });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(Guid id, ActivationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }
    }
}
