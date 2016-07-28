namespace EmailService.Transports
{
    public class TransportOptions
    {
        public bool Enabled { get; set; }

        public string SenderAddress { get; set; }

        public string SenderName { get; set; }
    }
}
