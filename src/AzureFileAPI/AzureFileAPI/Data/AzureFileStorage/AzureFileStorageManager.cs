using System;
using System.Globalization;
using System.Threading.Tasks;
using AzureFileAPI.Models;
using AzureFileAPI.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFileAPI.Data.AzureFileStorage
{
    public class AzureFileStorageManager : IAzureFileStorageManager
    {
        private readonly IOptions<AzureStorageSettings> configurationOption;
        private readonly CloudBlobClient cloudBlobClient;
        private readonly IBlobFileUploadUtility blobFileUploadUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileStorageManager" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="blobFileUploadUtility">The BLOB file upload utility.</param>
        public AzureFileStorageManager(
            IOptions<AzureStorageSettings> options,
            IBlobFileUploadUtility blobFileUploadUtility)
        {
            // Set configuration options
            this.configurationOption = options;
            this.blobFileUploadUtility = blobFileUploadUtility;

            // Connect to cloud and create Blob client
            var storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            this.cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }

        /// <inheritdoc />
        public async Task<bool> Add(byte[] inputArray, FileMetadata uploadFileViewModel)
        {
            CloudBlockBlob blockBlobReference = await this.GetCloudBlockBlob(uploadFileViewModel);

            // Set attributes and file meta-data
            AddPropertiesAndMetadataToFile(blockBlobReference, uploadFileViewModel);

            // Upload file to blob storage
            await this.blobFileUploadUtility.UploadAsync(inputArray, blockBlobReference);

            return await blockBlobReference.ExistsAsync();
        }

        /// <inheritdoc />
        public async Task<(CloudBlob, byte[])> GetFile(Guid fileId)
        {
            CloudBlob cloudBlob = await this.GetCloudBlob(fileId.ToString());

            if (await cloudBlob.ExistsAsync())
            {
                byte[] blobBytes = new byte[cloudBlob.Properties.Length];

                // Fetch attributes and meta-data
                await cloudBlob.FetchAttributesAsync();

                // Download file binary content and set bite array
                await cloudBlob.DownloadToByteArrayAsync(blobBytes, 0);

                return (cloudBlob, blobBytes);
            }

            return (null, null);
        }

        /// <inheritdoc />
        public async Task<CloudBlob> GetMetadata(Guid fileId)
        {
            CloudBlob cloudBlob = await this.GetCloudBlob(fileId.ToString());

            if (await cloudBlob.ExistsAsync())
            {
                return cloudBlob;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<bool> Delete(Guid fileId)
        {
            var cloudBlob = await this.GetCloudBlob(fileId.ToString());

            return await cloudBlob.DeleteIfExistsAsync();
        }

        private static void AddPropertiesAndMetadataToFile(CloudBlockBlob blob, FileMetadata uploadFileViewModel)
        {
            // Set properties
            blob.Properties.ContentType = uploadFileViewModel.MimeType;

            // Set meta-data
            blob.Metadata.Add(FileStorageConstants.CloudMetadataOriginalFileNameKey, uploadFileViewModel.FileName);
            blob.Metadata.Add(FileStorageConstants.CloudMetadataUploadedDateTimeKey, uploadFileViewModel.Uploaded.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<CloudBlob> GetCloudBlob(string fileName)
        {
            var cloudBlobContainer = await this.GetContainer();
            return cloudBlobContainer.GetBlobReference(fileName);
        }

        private async Task<CloudBlockBlob> GetCloudBlockBlob(FileMetadata uploadFileViewModel)
        {
            var cloudBlobContainer = await this.GetContainer();
            return cloudBlobContainer.GetBlockBlobReference(uploadFileViewModel.Id.ToString());
        }

        private async Task<CloudBlobContainer> GetContainer()
        {
            var blobContainer = this.cloudBlobClient.GetContainerReference(this.configurationOption.Value.ContainerName);

            // Create container if not exists
            await blobContainer.CreateIfNotExistsAsync();

            // Set container permission
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });

            return blobContainer;
        }
    }
}
