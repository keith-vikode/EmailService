using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels
{
    public class StatusErrorViewModel
    {
        public static readonly StatusErrorViewModel Default = new StatusErrorViewModel
        {
            Code = HttpStatusCode.InternalServerError,
            Title = "Critical Error",
            Description = "Something went badly wrong in this application."
        };

        public static readonly StatusErrorViewModel Unauthorized = new StatusErrorViewModel
        {
            Code = HttpStatusCode.Unauthorized,
            Title = "Not authorized",
            Description = "Authentication failed."
        };

        public static StatusErrorViewModel BadRequst = new StatusErrorViewModel
        {
            Code = HttpStatusCode.BadRequest,
            Title = "Bad Request",
            Description = "Invalid parameters supplied (this one's on you)."
        };

        public static StatusErrorViewModel NotFound = new StatusErrorViewModel
        {
            Code = HttpStatusCode.NotFound,
            Title = "Not Found",
            Description = "This isn't the page you're looking for."
        };

        public static StatusErrorViewModel Gone = new StatusErrorViewModel
        {
            Code = HttpStatusCode.Gone,
            Title = "It's gone",
            Description = "This page used to exist, but has been deleted."
        };

        public HttpStatusCode Code { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
