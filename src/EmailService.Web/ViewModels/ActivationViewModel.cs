using EmailService.Core.Entities;
using System;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels
{
    public class ActivationViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public async Task SaveChangesAsync(EmailServiceContext ctx)
        {
            var app = await ctx.FindApplicationAsync(Id);
            if (app != null)
            {
                app.IsActive = IsActive;
                await ctx.SaveChangesAsync();
            }
        }
    }
}
