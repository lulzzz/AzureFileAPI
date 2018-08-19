using System;
using System.Threading.Tasks;
using AzureFileAPI.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFileAPI.Data.AzureFileStorage
{
    public interface IAzureFileStorageManager
    {
        /// <summary>
        /// Adds the specified input array.
        /// </summary>
        /// <param name="inputArray">The input array.</param>
        /// <param name="fileMetadata">The file metadata.</param>
        /// <returns></returns>
        Task<bool> Add(byte[] inputArray, FileMetadata fileMetadata);

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        Task<(CloudBlob, byte[])> GetFile(Guid fileId);

        /// <summary>
        /// Deletes the specified file identifier.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        Task<bool> Delete(Guid fileId);
    }
}