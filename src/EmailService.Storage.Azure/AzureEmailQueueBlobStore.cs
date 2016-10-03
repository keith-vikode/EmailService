using EmailService.Core;
using EmailService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Storage.Azure
{
    public class AzureEmailQueueBlobStore : IEmailQueueBlobStore
    {
        private readonly CloudStorageAccount _account;
        private readonly ILogger _logger;

        private bool _initialized;

        private Lazy<CloudBlobContainer> _container;
        private Lazy<CloudBlobContainer> _poisonContainer;

        public AzureEmailQueueBlobStore(IOptions<AzureStorageOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AzureEmailQueueBlobStore>();

            // let this throw an exception if it fails, we'll get better information
            // from the core class than wrapping it in our own error
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);

            _container = SetupContainer(_account, options.Value.PendingQueueStorageContainerName);
            _poisonContainer = SetupContainer(_account, options.Value.PendingPoisonQueueStorageContainerName);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _container.Value.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, cancellationToken);
            await _poisonContainer.Value.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, cancellationToken);
            _initialized = true;
        }

        public async Task AddAsync(EmailQueueToken token, EmailMessageParams message, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Adding new blob to store for token {0}", token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var serialized = EmailMessageParams.ToJson(message);
            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            await blob.UploadTextAsync(serialized, Encoding.UTF8, null, null, null, cancellationToken);
        }

        public async Task<EmailMessageParams> GetAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Getting blob from store for token {0}", token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            if (await blob.ExistsAsync())
            {
                var json = await blob.DownloadTextAsync(Encoding.UTF8, null, null, null, cancellationToken);
                return EmailMessageParams.FromJson(json);
            }

            return null;
        }

        public async Task MoveToPoisonStoreAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Moving blob to poison store for token {0}", token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            if (await blob.ExistsAsync())
            {
                // create a new blob in the poison store
                var newBlob = _poisonContainer.Value.GetBlockBlobReference(blobName);

                // upload the blob from the old blob text
                var text = await blob.DownloadTextAsync(Encoding.UTF8, null, null, null, cancellationToken);
                await newBlob.UploadTextAsync(text, Encoding.UTF8, null, null, null, cancellationToken);

                // clean up the old blob
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken);
            }
        }

        public async Task RemoveAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Removing blob from store for token {0}", token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken);
        }

        private static string GetBlobName(EmailQueueToken token)
        {
            return $"{token.TimeStamp:yy/MM/dd}/{token.RequestId}.json";
        }

        private static Lazy<CloudBlobContainer> SetupContainer(CloudStorageAccount account, string containerName)
        {
            return new Lazy<CloudBlobContainer>(() =>
            {
                var client = account.CreateCloudBlobClient();
                return client.GetContainerReference(containerName);
            }, true);
        }
    }
}
