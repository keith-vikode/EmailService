﻿using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Web.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;
using System.IO;
using static EmailService.Core.Constants;

namespace EmailService.Web.Api
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
                builder.AddUserSecrets("sbsemailwebapi");
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Configuration = builder.Build();
            HostingEnv = env;
        }

        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment HostingEnv { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            ConfigureDatabase(services);
            ConfigureStorage(services);

            // add custom services
            services.AddTransient<IApplicationKeyStore, DbApplicationKeyStore>();
            services.AddSingleton<ICryptoServices>(RsaCryptoServices.Instance);

            // configure Kestrel HTTPS
            services.Configure<KestrelServerOptions>(options =>
            {
                if (HostingEnv.IsDevelopment())
                {
                    options.UseHttps("localhost.pfx", "0dinpa55");
                }
            });

            // set up AI telemetry
            services.AddApplicationInsightsTelemetry(Configuration);

            // Add framework services.
            services.AddMvc(options =>
            {
                if (HostingEnv.IsDevelopment())
                {
                    options.SslPort = 44321;
                }

                options.Filters.Add(new RequireHttpsAttribute());
            });

            // set up basic authentication options
            services.Configure<BasicAuthenticationOptions>(options =>
            {
                options.AutomaticAuthenticate = true;
                options.Realm = "SBS Email Service";
            });

            // Add auto-documentation services
            services.AddSwaggerGen(options =>
            {
                // TODO: get this from config if possible
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

                options.AddSecurityDefinition("basic", new BasicAuthScheme());

                var xmlPath = GetXmlCommentsPath();
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                options.OperationFilter<Filters.SwaggerRemoveCancellationTokenParameterFilter>();
                options.DescribeAllEnumsAsStrings();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureLogging(loggerFactory);

            app.UseBasicAuthentication();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi("swagger/ui");
        }

        protected virtual void ConfigureDatabase(IServiceCollection services)
        {
            // set up our EF data context
            var sqlConnection = Configuration.GetConnectionString(ConnectionStrings.SqlServer);
            services.AddDbContext<EmailServiceContext>(options =>
            {
                // cache all the things and trust that EF knows what it's doing;
                // this will probably be more reliable that rolling our own!
                options.UseMemoryCache(null);
                options.UseSqlServer(sqlConnection, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("EmailService.Web");
                    sqlOptions.EnableRetryOnFailure();
                });
            });
        }

        protected virtual void ConfigureStorage(IServiceCollection services)
        {
            var storageConnection = Configuration.GetConnectionString(ConnectionStrings.Storage);
            services.AddAzureStorageServices(options =>
            {
                options.ConnectionString = storageConnection;
            });
        }

        protected virtual void ConfigureLogging(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
        }

        private string GetXmlCommentsPath()
        {
            var app = PlatformServices.Default.Application;
            return Path.Combine(app.ApplicationBasePath, Path.ChangeExtension(app.ApplicationName, "Api.xml"));
        }
    }
}
