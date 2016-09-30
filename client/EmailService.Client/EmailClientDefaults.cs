namespace EmailService.Client
{
    /// <summary>
    /// Default values for <see cref="EmailClient"/>. 
    /// </summary>
    internal static class EmailClientDefaults
    {
        public const int ApiVersion = 1;
        public const string ServerUrl = "https://sbs-email-we.azurewebsites.net/";
        public const string MessagesApi = "/v{0}/messages";
        public const string TemplatesApi = "/v{0}/templates";
    }
}
