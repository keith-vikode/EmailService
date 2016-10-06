using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Web.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<KeyValuePair<Guid, string>> Applications { get; set; }
        
        public static async Task<IndexViewModel> LoadAsync(EmailServiceContext ctx)
        {
            var model = new IndexViewModel();
            var apps = await ctx.Applications
                .Select(a => new KeyValuePair<Guid, string>(a.Id, a.Name))
                .ToListAsync();
            model.Applications = apps;
            return model;
        }
    }
}