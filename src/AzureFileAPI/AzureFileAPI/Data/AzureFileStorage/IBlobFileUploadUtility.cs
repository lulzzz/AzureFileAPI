using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFileAPI.Data.AzureFileStorage
{
    public interface IBlobFileUploadUtility
    {
        /// <summary>
        /// Uploads the Asynchronously.
        /// (Separate large file to blocks, uploads to azure and compile to large file in azure)
        /// </summary>
        /// <param name="blobFileArray">The BLOB file array.</param>
        /// <param name="blockBlob">The Block BLOB.</param>
        /// <param name="parallelUploadThreads">The parallel upload threads count.</param>
        /// <returns>
        /// The task
        /// </returns>
        Task UploadAsync(byte[] blobFileArray, CloudBlockBlob blockBlob, int parallelUploadThreads = FileStorageConstants.DefaultParallelUploadThreads);
    }
}
