using System;
using System.Threading.Tasks;
using AzureFileAPI.Models;
using Microsoft.AspNetCore.Http;

namespace AzureFileAPI.Services
{
    public interface IAzureFileService
    {
        /// <inheritdoc />
        Task<FileMetadata> UploadStream(IFormFile file);

        /// <inheritdoc />
        Task<FileViewModel> GetFile(Guid fileId);

        /// <inheritdoc />
        Task<bool> DeleteFile(Guid fileId);
    }
}