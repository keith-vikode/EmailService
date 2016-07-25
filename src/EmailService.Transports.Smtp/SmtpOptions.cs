namespace EmailService.Transports.Smtp
{
    public class SmtpOptions
    {
        public static readonly SmtpOptions Localhost = new SmtpOptions
        {
            Host = "localhost",
            Port = 25,
            SenderAddress = "no-reply@localhost",
            SenderName = "Localhost"
        };

        public string Host { get; set; }

        public ushort Port { get; set; }

        public bool UseEncryption { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SenderAddress { get; set; }

        public string SenderName { get; set; }
    }
}
