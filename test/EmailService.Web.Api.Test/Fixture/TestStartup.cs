using EmailService.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddDbContext<EmailServiceContext>(builder => builder.UseInMemoryDatabase());
        }

        protected override void ConfigureStorage(IServiceCollection services)
        {
            services.AddInMemoryStorageServices();
        }

        protected override void ConfigureLogging(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug, true);
        }
    }
}
