using EmailService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmailService.Web.ViewModels.Templates
{
    public class IndexViewModel
    {
        public Guid? ApplicationId { get; set; }

        public bool ShowDeactivated { get; set; }

        public IEnumerable<SelectListItem> Applications { get; private set; } = new List<SelectListItem>();

        public IList<TemplateIndexViewModel> Templates { get; } = new List<TemplateIndexViewModel>();

        public static async Task<IndexViewModel> LoadAsync(EmailServiceContext ctx, Guid? applicationId, bool showDeactivated)
        {
            var model = new IndexViewModel();
            var list = ctx.Templates
                .Include(t => t.Application)
                .OrderBy(t => t.Name)
                .AsQueryable();

            if (applicationId.HasValue)
            {
                list = list.Where(t => t.ApplicationId == applicationId.Value);
            }

            if (!showDeactivated)
            {
                list = list.Where(t => t.IsActive);
            }

            await list.ForEachAsync(t => model.Templates.Add(new TemplateIndexViewModel(t)));

            var apps = new List<SelectListItem>();
            await ctx.Applications.ForEachAsync(a => apps.Add(new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }));

            model.Applications = apps;
            model.ApplicationId = applicationId;
            model.ShowDeactivated = showDeactivated;

            return model;
        }
    }

    public class TemplateIndexViewModel
    {
        public TemplateIndexViewModel(Template source)
        {
            Id = source?.Id ?? Guid.Empty;
            Name = source?.Name;
            Description = source?.Description ?? "No description";
            ApplicationId = source?.ApplicationId ?? Guid.Empty;
            ApplicationName = source?.Application.Name;
            IsActive = source?.IsActive ?? false;
        }

        public Guid Id { get; }

        public string Name { get; }

        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";
    }
}
