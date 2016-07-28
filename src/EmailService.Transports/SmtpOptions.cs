namespace EmailService.Transports
{
    public class SmtpOptions : TransportOptions
    {
        public string Host { get; set; }

        public ushort Port { get; set; }

        public bool UseEncryption { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
