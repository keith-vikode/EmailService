using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Crypto;
using EmailService.Transports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;
using System.IO;

namespace EmailService.Web
{
    public class Startup
    {
        private static class ConnectionStrings
        {
            public const string SqlServer = nameof(SqlServer);
            public const string Storage = nameof(Storage);
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // set up our EF data context
            var connection = Configuration.GetConnectionString(ConnectionStrings.SqlServer);
            services.AddDbContext<EmailServiceContext>(options =>
            {
                options.UseSqlServer(connection, b => b.MigrationsAssembly("EmailService.Web"));
            });

            // add custom services
            services.AddSingleton<ICryptoServices>(RsaCryptoServices.Instance);
            services.AddSingleton<IEmailTransportFactory>(EmailTransportFactory.Instance);
            services.AddAzureStorageServices(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString(ConnectionStrings.Storage);
            });

            services.AddTransient<EmailSender>();

            // Add framework services.
            services.AddMvc();

            // Add auto-documentation services
            services.AddSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Sun Branding Solutions Common Email API",
                    Description = "A centralised API for sending and templating emails for SBS applications.",
                    Contact = new Contact
                    {
                        Email = "kwilliams@sunbrandingsolutions.com",
                        Name = "Keith Williams"
                    }
                });

                options.IncludeXmlComments(GetXmlCommentsPath());
                options.OperationFilter<Filters.SwaggerRemoveCancellationTokenParameterFilter>();
                options.DescribeAllEnumsAsStrings();
                
                options.AddSecurityDefinition("ApiKey", new ApiKeyScheme
                {
                    Name = "Authorization",
                    In = "header"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}/{language?}");
            });

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        private string GetXmlCommentsPath()
        {
            var app = PlatformServices.Default.Application;
            return Path.Combine(app.ApplicationBasePath, Path.ChangeExtension(app.ApplicationName, "Web.xml"));
        }
    }
}
