namespace EmailService.Storage.Azure
{
    public class AzureStorageOptions
    {
        public const string DefaultContainerName = "email";

        public string ConnectionString { get; set; }

        public string QueueName { get; set; } = DefaultContainerName;

        public string StorageContainerName { get; set; } = DefaultContainerName;
    }
}
