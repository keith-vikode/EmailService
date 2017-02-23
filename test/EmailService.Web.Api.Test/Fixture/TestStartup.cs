using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EmailService.Core.Entities;

namespace EmailService.Web.Api.Test
{
    public class TestStartup : Startup
    {
        public TestStartup(IHostingEnvironment env)
            : base(env)
        {
        }

        protected override void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<EmailServiceContext>(builder =>
                builder.UseInMemoryDatabase()
                    .UseMemoryCache(null));
        }

        protected override void ConfigureStorage(IServiceCollection services)
        {
            services.AddInMemoryStorageServices();
        }
    }
}
