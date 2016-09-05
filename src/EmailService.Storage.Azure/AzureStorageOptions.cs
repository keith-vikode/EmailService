namespace EmailService.Storage.Azure
{
    public class AzureStorageOptions
    {
        private const string DefaultPrefix = "email-";
        private const string PoisonSuffix = "-poison";
        private const string Pending = "pending";
        private const string PendingQueue = DefaultPrefix + Pending;
        private const string PendingQueuePoison = DefaultPrefix + Pending + PoisonSuffix;

        public string ConnectionString { get; set; }

        public string PendingQueueName { get; set; } = PendingQueue;

        public string PendingPoisonQueueName { get; set; } = PendingQueuePoison;

        public string PendingQueueStorageContainerName { get; set; } = PendingQueue;

        public string PendingPoisonQueueStorageContainerName { get; set; } = PendingQueuePoison;
    }
}
