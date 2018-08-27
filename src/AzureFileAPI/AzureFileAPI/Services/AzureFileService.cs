using System;
using System.IO;
using System.Threading.Tasks;
using AzureFileAPI.Data.AzureFileStorage;
using AzureFileAPI.Models;
using Microsoft.AspNetCore.Http;

namespace AzureFileAPI.Services
{
    public class AzureFileService : IAzureFileService
    {
        private readonly IAzureFileStorageManager azureFileStorageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="azureFileStorageManager" /> class
        /// </summary>
        /// <param name="azureFileStorageManager">The file storage manager.</param>
        public AzureFileService(
            IAzureFileStorageManager azureFileStorageManager)
        {
            this.azureFileStorageManager = azureFileStorageManager;
        }

        /// <inheritdoc />
        public async Task<FileMetadata> UploadStream(IFormFile file)
        {
            var fileMetadata = new FileMetadata
            {
                Id = Guid.NewGuid(),
                Uploaded = DateTime.UtcNow,
                FileName = file.FileName,
                MimeType = file.ContentType,
                FileSize = file.Length,
            };

            byte[] inputArray = ConvertFileToByteArray(file);

            // Upload file to blob storage
            bool isFileUploaded = await this.azureFileStorageManager.Add(inputArray, fileMetadata);

            if (!isFileUploaded)
            {
                return null;
            }

            return fileMetadata;
        }

        /// <inheritdoc />
        public async Task<FileViewModel> GetFile(Guid fileId)
        {
            (var cloudBlob, var fileContent) = await this.azureFileStorageManager.GetFile(fileId);

            if (cloudBlob != null)
            {
                // Set file view model
                var fileViewModel = new FileViewModel
                {
                    FileMetadataViewModel = new FileMetadata
                    {
                        FileName = cloudBlob.Metadata[FileStorageConstants.CloudMetadataOriginalFileNameKey],
                        MimeType = cloudBlob.Properties.ContentType,
                    },
                    Content = fileContent,
                };

                return fileViewModel;
            }

            return null;
        }


        /// <inheritdoc />
        public async Task<bool> DeleteFile(Guid fileId)
        {
            // Delete file from blob storage
            bool isRemovedFromBlob = await this.azureFileStorageManager.Delete(fileId);

            return isRemovedFromBlob;
        }

        private static byte[] ConvertFileToByteArray(IFormFile file)
        {
            byte[] fileBytes;

            using (var fileStream = file.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                fileStream.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return fileBytes;
        }
    }
}
