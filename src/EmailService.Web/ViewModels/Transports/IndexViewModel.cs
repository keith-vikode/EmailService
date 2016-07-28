using EmailService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Transports
{
    public class IndexViewModel
    {
        public IList<TransportIndexViewModel> Transports { get; } = new List<TransportIndexViewModel>();

        public static async Task<IndexViewModel> LoadAsync(EmailServiceContext ctx)
        {
            var model = new IndexViewModel();
            await ctx.Transports
                .ToAsyncEnumerable()
                .ForEachAsync(t => model.Transports.Add(new TransportIndexViewModel(t)));
            return model;
        }
    }

    public class TransportIndexViewModel
    {
        public TransportIndexViewModel(Transport source)
        {
            Id = source?.Id ?? Guid.Empty;
            Name = source?.Name;
            Host = source?.Hostname;
            Type = source?.Type.ToString();
            IsActive = source?.IsActive ?? false;
        }

        public Guid Id { get; }

        public string Name { get; }

        public string Host { get; }

        public string Type { get; }

        public bool IsActive { get; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";
    }
}
