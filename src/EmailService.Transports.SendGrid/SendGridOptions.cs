namespace EmailService.Transports.SendGrid
{
    public class SendGridOptions
    {
        public bool Enabled { get; set; }

        public string ApiKey { get; set; }

        public string SenderAddress { get; set; }

        public string SenderName { get; set; }

        public string RedirectTo { get; set; }

        public string Template { get; set; }
    }
}
