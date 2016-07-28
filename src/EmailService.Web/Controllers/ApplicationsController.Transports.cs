using EmailService.Core.Entities;
using EmailService.Web.ViewModels.Applications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.Controllers
{
    public partial class ApplicationsController
    {
        public async Task<IActionResult> AddTransport(Guid id)
        {
            var app = await _ctx.FindApplicationAsync(id);
            if (app != null)
            {
                return View(new AddTransportViewModel
                {
                    ApplicationId = app.Id,
                    ApplicationName = app.Name,
                    Priority = app.Transports.Count + 1,
                    Transports = await GetAvailableTransportsAsync(id)
                });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddTransport(Guid id, AddTransportViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id });
            }

            model.Transports = await GetAvailableTransportsAsync(id);
            return View(model);
        }

        public async Task<IActionResult> PrioritiseTransports(Guid id)
        {
            var model = await PrioritiseTransportsViewModel.LoadAsync(_ctx, id);
            if (model != null)
            {
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> PrioritiseTransports(Guid id, PrioritiseTransportsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await model.SaveChangesAsync(_ctx);
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }

        private async Task<List<SelectListItem>> GetAvailableTransportsAsync(Guid applicationId)
        {
            var availableTransports = await _ctx.Transports
                .Where(t => !t.Applications.Any(a => a.ApplicationId == applicationId))
                .ToListAsync();

            var smtpGroup = new SelectListGroup { Name = "SMTP" };
            var sendGridGroup = new SelectListGroup { Name = "SendGrid" };
            return availableTransports.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Group = t.Type == TransportType.SendGrid ? sendGridGroup : smtpGroup
            }).ToList();
        }
    }
}
