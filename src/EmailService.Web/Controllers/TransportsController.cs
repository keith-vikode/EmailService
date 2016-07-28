using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EmailService.Core.Entities;
using EmailService.Web.ViewModels.Transports;
using EmailService.Web.ViewModels;
using EmailService.Core.Services;

namespace EmailService.Web.Controllers
{
    public class TransportsController : Controller
    {
        private EmailServiceContext _ctx;

        public TransportsController(EmailServiceContext ctx)
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
            var model = await TransportDetailsViewModel.LoadAysnc(_ctx, id);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        public IActionResult CreateSmtp() => View(new CreateSmtpViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateSmtp(CreateSmtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = model.CreateDbModel();
                _ctx.Transports.Add(entity);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult CreateSendGrid() => View(new CreateSendGridViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateSendGrid(CreateSendGridViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = model.CreateDbModel();
                _ctx.Transports.Add(entity);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        
        public async Task<IActionResult> Edit(Guid id)
        {
            var existing = await _ctx.FindTransportAsync(id);
            if (existing != null)
            {
                if (existing.Type == TransportType.SendGrid)
                {
                    return RedirectToAction(nameof(EditSendGrid), new { id });
                }
                else if (existing.Type == TransportType.Smtp)
                {
                    return RedirectToAction(nameof(EditSmtp), new { id });
                }
                else
                {
                    return BadRequest();
                }
            }

            return NotFound();
        }

        public async Task<IActionResult> EditSmtp(Guid id)
        {
            var existing = await _ctx.FindTransportAsync(id);
            if (existing != null && existing.Type == TransportType.Smtp)
            {
                var model = new EditSmtpViewModel(existing);
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditSmtp(Guid id, EditSmtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _ctx.FindTransportAsync(id);
                if (existing != null && existing.Type == TransportType.Smtp)
                {
                    model.UpdateDbModel(existing);
                    await _ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id });
                }

                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> EditSendGrid(Guid id)
        {
            var existing = await _ctx.FindTransportAsync(id);
            if (existing != null && existing.Type == TransportType.SendGrid)
            {
                var model = new EditSendGridViewModel(existing);
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditSendGrid(Guid id, EditSendGridViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _ctx.FindTransportAsync(id);
                if (existing != null && existing.Type == TransportType.SendGrid)
                {
                    model.UpdateDbModel(existing);
                    await _ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id });
                }

                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> Deactivate(Guid id)
        {
            var transport = await _ctx.FindTransportAsync(id);
            if (transport != null)
            {
                return View(new ActivationViewModel { Id = id, Name = transport.Name });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(ActivationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var transport = await _ctx.FindTransportAsync(model.Id);
                if (transport != null)
                {
                    transport.IsActive = false;
                    await _ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = transport.Id });
                }

                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> Reactivate(Guid id)
        {
            var transport = await _ctx.FindTransportAsync(id);
            if (transport != null)
            {
                return View(new ActivationViewModel { Id = id, Name = transport.Name });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(ActivationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var transport = await _ctx.FindTransportAsync(model.Id);
                if (transport != null)
                {
                    transport.IsActive = true;
                    await _ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = transport.Id });
                }

                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> Test(Guid id)
        {
            var transport = await _ctx.FindTransportAsync(id);
            if (transport != null)
            {
                return View(new TestTransportViewModel
                {
                    TransportId = transport.Id,
                    TransportName = transport.Name
                });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Test(
            Guid id,
            TestTransportViewModel model,
            [FromServices] IEmailTransportFactory factory)
        {
            if (ModelState.IsValid)
            {
                var transport = await _ctx.FindTransportAsync(id);
                if (transport != null)
                {
                    var impl = factory.CreateTransport(transport);
                    await impl.SendAsync(new Core.SenderParams
                    {
                        Subject = $"Test email using transport {id}",
                        Body = model.Message,
                        To = new List<string> { model.EmailAddress }
                    });

                    return RedirectToAction(nameof(Details), new { id });
                }

                return NotFound();
            }

            return View(model);
        }
    }
}
