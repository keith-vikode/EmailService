namespace EmailService.Client
{
    /// <summary>
    /// The configuration for the <see cref="EmailClient"/> class.
    /// </summary>
    public class EmailClientOptions
    {
        /// <summary>
        /// Gets the application ID.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the root server URL (default if not set).
        /// </summary>
        public string ServerUrl { get; set; } = EmailClientDefaults.ServerUrl;

        /// <summary>
        /// Gets or sets the path to the send message API (default if not set).
        /// </summary>
        public string MessagesApi { get; set; } = string.Format(EmailClientDefaults.MessagesApi, EmailClientDefaults.ApiVersion);

        /// <summary>
        /// Gets or sets the path to the list templates API (default if not set).
        /// </summary>
        public string TemplatesApi { get; set; } = string.Format(EmailClientDefaults.TemplatesApi, EmailClientDefaults.ApiVersion);

        /// <summary>
        /// Gest or sets the version of the API to use (latest if not set).
        /// </summary>
        public int ApiVersion { get; set; } = EmailClientDefaults.ApiVersion;
    }
}
