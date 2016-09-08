namespace EmailService.Storage.Azure
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; }

        public string AuditTableName { get; set; } = "emlsent";

        public string PendingQueueName { get; set; } = "emlpending";

        public string PendingPoisonQueueName { get; set; } = "emlpending-poison";

        public string PendingQueueStorageContainerName { get; set; } = "emlpending";

        public string PendingPoisonQueueStorageContainerName { get; set; } = "emlpending-poison";
    }
}
