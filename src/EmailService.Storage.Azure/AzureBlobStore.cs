using EmailService.Core;
using EmailService.Core.Services;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Storage.Azure
{
    public class AzureBlobStore : IEmailMessageBlobStore
    {
        private readonly CloudStorageAccount _account;

        private bool _initialized;

        private Lazy<CloudBlobContainer> _container;

        public AzureBlobStore(IOptions<AzureStorageOptions> options)
        {
            // let this throw an exception if it fails, we'll get better information
            //. from the core class than wrapping it in our own error
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);

            _container = new Lazy<CloudBlobContainer>(() =>
            {
                var client = _account.CreateCloudBlobClient();
                return client.GetContainerReference(options.Value.StorageContainerName);
            }, false);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _container.Value.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, cancellationToken);
            _initialized = true;
        }

        public async Task AddAsync(Guid token, EmailMessageParams message, CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var serialized = EmailMessageParams.ToJson(message);
            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            await blob.UploadTextAsync(serialized);
        }

        public async Task<EmailMessageParams> GetAsync(Guid token, CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            if (await blob.ExistsAsync())
            {
                var json = await blob.DownloadTextAsync();
                return EmailMessageParams.FromJson(json);
            }

            return null;
        }

        public async Task RemoveAsync(Guid token, CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var blobName = GetBlobName(token);
            var blob = _container.Value.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task ArchiveAsync(Guid token, CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var blobName = GetBlobName(token);
            var archiveName = GetArchiveBlobName(token);

            var blob = _container.Value.GetBlockBlobReference(blobName);
            if (await blob.ExistsAsync())
            {
                var archive = _container.Value.GetBlockBlobReference(archiveName);
                await archive.UploadTextAsync(await blob.DownloadTextAsync());
                await blob.DeleteAsync();
            }
        }

        private static string GetBlobName(Guid token)
        {
            return $"pending/{token}.json";
        }

        private static string GetArchiveBlobName(Guid token)
        {
            return $"archive/{DateTime.UtcNow:yyyy/MM}/{token}.json";
        }
    }
}
