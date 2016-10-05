using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Transports;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;
using static EmailService.Core.Constants;

namespace EmailService.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
            HostingEnv = env;
        }

        public IHostingEnvironment HostingEnv { get; set; }

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
            services.AddTransient<IApplicationKeyStore, DbApplicationKeyStore>();
            services.AddSingleton<ICryptoServices>(RsaCryptoServices.Instance);
            services.AddSingleton<IEmailTransportFactory>(EmailTransportFactory.Instance);
            services.AddAzureStorageServices(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString(ConnectionStrings.Storage);
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                if (HostingEnv.IsDevelopment())
                {
                    options.UseHttps("localhost.pfx", "0dinpa55");
                }
            });

            // Add framework services.
            services.AddMvc(options =>
            {
                if (HostingEnv.IsDevelopment())
                {
                    options.SslPort = 44320;
                }

                options.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // initiate the database
            var db = app.ApplicationServices.GetService<EmailServiceContext>();
            db.Database.Migrate();

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

            app.UseCookieAuthentication();
            
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                ClientSecret = Configuration["Authentication:AzureAd:ClientSecret"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"],
                ResponseType = OpenIdConnectResponseType.IdToken,
                Scope = { "openid", "groups" }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}/{language?}");
            });
        }

        // Handle sign-in errors differently than generic errors.
        private Task OnAuthenticationFailed(FailureContext context)
        {
            context.HandleResponse();
            context.Response.StatusCode = 401; // this should trigger the status code error pages
            return Task.FromResult(0);
        }
    }
}
