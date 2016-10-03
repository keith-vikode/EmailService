namespace EmailService.Storage.Azure
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; }

        public string AdminLogTableName { get; set; } = "adminlog";

        public string AuditTableName { get; set; } = "emlsent";

        public string ProcessorLogTableName { get; set; } = "emlprocessorlog";

        public string PendingQueueName { get; set; } = "emlpending";

        public string PendingPoisonQueueName { get; set; } = "emlpending-poison";

        public string PendingQueueStorageContainerName { get; set; } = "emlpending";

        public string PendingPoisonQueueStorageContainerName { get; set; } = "emlpending-poison";
    }
}
