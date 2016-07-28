using EmailService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Applications
{
    public class IndexViewModel
    {
        public List<ApplicationIndexViewModel> Applications { get; } = new List<ApplicationIndexViewModel>();

        public static async Task<IndexViewModel> LoadAsync(EmailServiceContext _ctx)
        {
            var model = new IndexViewModel();
            await _ctx.Applications.ToAsyncEnumerable().ForEachAsync(a =>
            {
                model.Applications.Add(new ApplicationIndexViewModel(a));
            });
            return model;
        }
    }

    public class ApplicationIndexViewModel
    {
        public ApplicationIndexViewModel(Application source)
        {
            Id = source?.Id ?? Guid.Empty;
            Name = source?.Name;
            Description = source?.Description;
            IsActive = source?.IsActive ?? false;
        }

        public Guid Id { get; }

        public string Name { get; }

        public string Description { get; }

        public bool IsActive { get; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";
    }
}
