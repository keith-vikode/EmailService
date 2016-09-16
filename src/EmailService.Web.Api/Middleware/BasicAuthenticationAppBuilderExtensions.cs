using EmailService.Web.Api.Middleware;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class BasicAuthenticationAppBuilderExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}
