using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AzureFileAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFileAPI.Controllers
{
    /// <summary>
    /// Files web api controller
    /// </summary>
    /// <seealso cref="Controller" />
    [Route("/api/v1/files")]
    public class FilesApiController : Controller
    {
        private readonly IAzureFileService azureFileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesApiController" /> class.
        /// </summary>
        /// <param name="azureFileService">The file service.</param>
        public FilesApiController(
             IAzureFileService azureFileService)
        {
            this.azureFileService = azureFileService;
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        /// Upload file view model
        /// </returns>
        [HttpPost]
        [Authorize]
        [SwaggerOperation("UploadFile")]
        [SwaggerResponse(200, description: "Returns Upload File View Model with unique file name, Id etc")]
        [SwaggerResponse(400, description: "Returns when file has incorrect content")]
        [SwaggerResponse(500, description: "File already exists or was not uploaded correctly")]
        public async Task<IActionResult> Post([FromForm] IFormFile file)
        {
            if (file != null && file.Length <= 0)
            {
                return new BadRequestResult();
            }

            // Create view model
            var fileMetadata = await this.azureFileService.UploadStream(file);

            if (fileMetadata == null)
            {
                return new StatusCodeResult(500);
            }

            return new ObjectResult(fileMetadata);
        }

        /// <summary>
        /// Get file data
        /// </summary>
        /// <param name="fileId">Id of the file.</param>
        /// <returns>
        /// Object Result
        /// </returns>
        [HttpGet("{fileId}")]
        [Authorize]
        [SwaggerOperation("GetFile")]
        [SwaggerResponse(200, type: typeof(FileContentResult), description: "Returns FileContentResult object which contains file-content as byte array, file name and MIME type")]
        [SwaggerResponse(400, typeof(string), description: "Returns when file is empty or file doesn't exist")]
        public async Task<IActionResult> Get([Required] Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return new BadRequestResult();
            }

            var fileViewModel = await this.azureFileService.GetFile(fileId);

            if (fileViewModel == null)
            {
                return new NotFoundObjectResult($"File {fileId} doesn't exist.");
            }

            return new FileContentResult(fileViewModel.Content, fileViewModel.FileMetadataViewModel.MimeType)
            {
                FileDownloadName = fileViewModel.FileMetadataViewModel.FileName
            };
        }

        /// <summary>
        /// Deletes the specified file
        /// </summary>
        /// <param name="fileId">Id of the file</param>
        /// <returns>Is file removed</returns>
        [HttpDelete("{fileId}")]
        [SwaggerOperation("DeleteFile")]
        [SwaggerResponse(200, description: "Returns if file was removed")]
        [SwaggerResponse(400, typeof(string), "Returns an error message if filename was empty")]
        public async Task<IActionResult> Delete([Required] Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return new BadRequestResult();
            }

            bool isFileRemoved = await this.azureFileService.DeleteFile(fileId);

            if (isFileRemoved == false)
            {
                return new BadRequestObjectResult("The file wasn't removed");
            }

            return new OkResult();
        }
    }
}
