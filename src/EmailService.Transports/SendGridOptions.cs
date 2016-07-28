namespace EmailService.Transports
{
    public class SendGridOptions : TransportOptions
    {
        public string ApiKey { get; set; }

        public string Template { get; set; }
    }
}
