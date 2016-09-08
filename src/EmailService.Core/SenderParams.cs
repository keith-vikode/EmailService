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

        public IList<string> Bcc { get; set; } = new List<string>();

        public string SenderAddress { get; set; }

        public string SenderName { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public IEnumerable<RecipientInfo> GetRecipients()
        {
            foreach (var address in To)
            {
                yield return new RecipientInfo(address);
            }

            foreach (var address in CC)
            {
                yield return new RecipientInfo(address, RecipientType.CC);
            }

            foreach (var address in Bcc)
            {
                yield return new RecipientInfo(address, RecipientType.Bcc);
            }
        }
    }
}
