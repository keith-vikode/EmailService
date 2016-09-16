namespace EmailService.Core
{
    /// <summary>
    /// Common constant values for the application.
    /// </summary>
    public static class Constants
    {
        public const int NameFieldMaxLength = 50;
        public const int DescriptionFieldMaxLength = 2000;
        public const int SubjectFieldMaxLength = 255;
        public const int HostnameFieldMaxLength = 255;
        public const int SenderAddressMaxLength = 255;
        public const int SenderNameMaxLength = 50;
        public const string ApiKey = nameof(ApiKey);
        public const string Authorization = nameof(Authorization);
        public const string ApplicationId = nameof(ApplicationId);

        public static class ConnectionStrings
        {
            public const string SqlServer = nameof(SqlServer);
            public const string Storage = nameof(Storage);
        }
    }
}
