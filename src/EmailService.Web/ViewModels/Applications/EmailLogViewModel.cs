using EmailService.Core;
using System;
using System.Collections.Generic;

namespace EmailService.Web.ViewModels.Applications
{
    public class EmailLogViewModel
    {
        private const int DefaultWindowInDays = 7;

        public EmailLogViewModel()
        {
            // set up default range
            RangeStart = DateTime.Today.AddDays(-DefaultWindowInDays);
            RangeEnd = DateTime.Today.AddDays(1); // end of today
        }

        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public DateTime RangeStart { get; set; }

        public DateTime RangeEnd { get; set; }

        // TODO: implement a VM instead of the core interface
        public IEnumerable<ISentEmailInfo> Results { get; set; } = new List<ISentEmailInfo>();
    }
}
