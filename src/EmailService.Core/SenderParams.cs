using System.Collections.Generic;

namespace EmailService.Core
{
    /// <summary>
    /// Parameters required to send an email.
    /// </summary>
    public class SenderParams
    {
        public IList<string> To { get; set; } = new List<string>();

        public IList<string> CC { get; set;  } = new List<string>();

        public string SenderEmail { get; set; }

        public string SenderName { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
