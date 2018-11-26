using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureFileAPI.Controllers;
using AzureFileAPI.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AzureFileAPI.Services;

namespace AzureFileApi.UnitTests
{
    public class FileApiControllerTests
    {
        private readonly Mock<IAzureFileService> mockFileService;

        public FileApiControllerTests()
        {
            var mockRepositoryObject = new MockRepository(MockBehavior.Strict);
            this.mockFileService = mockRepositoryObject.Create<IAzureFileService>();
        }

        [Fact]
        public async void UploadFile_WithProperName_ReturnsUploadedFileViewModel()
        {
            // Arrange
            string fileName = "image.png";
            string contenType = "image/png";
            Guid uploaderAccountId = Guid.NewGuid();
            DateTime uploadedDateTime = DateTime.UtcNow;

            var fileMock = new Mock<IFormFile>();

            // setup mock for IFormFile
            var content = "Content string";
            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);
                writer.Write(content);
                writer.Flush();
                memoryStream.Position = 0;
                fileMock.Setup(_ => _.FileName).Returns(fileName);
                fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);
                fileMock.Setup(_ => _.ContentType).Returns(contenType);

                this.mockFileService
                    .Setup(s => s.UploadStream(fileMock.Object))
                    .Returns(Task.FromResult(new FileMetadata
                    {
                        Id = Guid.NewGuid(),
                        FileName = fileName,
                        MimeType = contenType,
                        FileSize = memoryStream.Length,
                        Uploaded = uploadedDateTime
                    }));
            }

            // Act
            var actionResult = await this.FileApiController(uploaderAccountId).Post(fileMock.Object);

            // Assert
            actionResult.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async void UploadFile_WithEmptyFileContent_ReturnsUploadedFileViewModel()
        {
            // Arrange
            string fileName = String.Empty;
            string contenType = "image/png";
            Guid uploaderAccountId = Guid.NewGuid();
            DateTime uploadedDateTime = DateTime.UtcNow;

            var fileMock = new Mock<IFormFile>();

            // setup mock for IFormFile
            string content = String.Empty;
            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);
                writer.Write(content);
                writer.Flush();
                memoryStream.Position = 0;

                fileMock.Setup(_ => _.FileName).Returns(fileName);
                fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);
                fileMock.Setup(_ => _.ContentType).Returns(contenType);

                this.mockFileService
                    .Setup(s => s.UploadStream(fileMock.Object))
                    .Returns(Task.FromResult(new FileMetadata
                    {
                        Id = Guid.NewGuid(),
                        FileName = fileName,
                        MimeType = contenType,
                        FileSize = memoryStream.Length,
                        Uploaded = uploadedDateTime
                    }));
            }

            // Act
            var actionResult = await this.FileApiController().Post(fileMock.Object);

            // Assert
            actionResult.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async void UploadFile_WithEmptyFileMetadataObject_ReturnsServerError()
        {
            string fileName = "image.png";
            string contenType = "image/png";
            Guid uploaderAccountId = Guid.NewGuid();

            var fileMock = new Mock<IFormFile>();

            // setup mock for IFormFile
            var content = "Content string";
            using (Stream memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);
                writer.Write(content);
                writer.Flush();
                memoryStream.Position = 0;
                fileMock.Setup(_ => _.FileName).Returns(fileName);
                fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);
                fileMock.Setup(_ => _.ContentType).Returns(contenType);

                this.mockFileService
                    .Setup(s => s.UploadStream(fileMock.Object))
                    .Returns(Task.FromResult<FileMetadata>(null));
            }

            // Act
            var actionResult = await this.FileApiController(uploaderAccountId)
                .Post(fileMock.Object);

            // Assert
            actionResult.Should().BeOfType<StatusCodeResult>();
        }

        [Fact]
        public async void GetFile_WithName_ReturnsFileViewModel()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            string fileName = "image.png";
            string contenType = "image/png";


            var fileContent = new byte[10];
            new Random().NextBytes(fileContent);

            this.mockFileService
                .Setup(s => s.GetFile(id))
                .Returns(Task.FromResult(new FileViewModel
                {
                    FileMetadataViewModel = new FileMetadata
                    {
                        FileName = fileName,
                        MimeType = contenType,
                    },
                    Content = fileContent,
                }));

            // Act
            IActionResult actionResult = await this.FileApiController().Get(id);

            // Assert
            actionResult.Should().BeOfType<FileContentResult>();
            ((FileContentResult)actionResult).FileContents.Should().NotBeEmpty();
        }

        [Fact]
        public async void GetFile_WithEmptyFileId_ReturnsBadRequestResult()
        {
            // Arrange
            Guid id = Guid.Empty;
            string fileName = "file.png";
            string contenType = "image/png";

            var fileContent = new byte[10];
            new Random().NextBytes(fileContent);

            this.mockFileService
                .Setup(s => s.GetFile(id))
                .Returns(Task.FromResult(new FileViewModel
                {
                    FileMetadataViewModel = new FileMetadata
                    {
                        FileName = fileName,
                        MimeType = contenType,
                    },
                    Content = fileContent,
                }));

            // Act
            IActionResult actionResult = await this.FileApiController().Get(id);

            // Assert
            actionResult.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async void GetFile_WithEmptyFileViewModel_ReturnsFileViewModel()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            var fileContent = new byte[10];
            new Random().NextBytes(fileContent);

            this.mockFileService
                .Setup(s => s.GetFile(id))
                .Returns(Task.FromResult<FileViewModel>(null));

            // Act
            IActionResult actionResult = await this.FileApiController().Get(id);

            // Assert
            actionResult.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async void DeleteFile_WithName_ReturnsOkResult()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            var fileContent = new byte[10];
            new Random().NextBytes(fileContent);

            this.mockFileService
                .Setup(s => s.DeleteFile(id))
                .Returns(Task.FromResult(true));

            // Act
            IActionResult actionResult = await this.FileApiController().Delete(id);

            // Assert
            actionResult.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async void DeleteFile_WithServiceReturnsFalse_ReturnsBadRequestResult()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            var fileContent = new byte[10];
            new Random().NextBytes(fileContent);

            this.mockFileService
                .Setup(s => s.DeleteFile(id))
                .Returns(Task.FromResult(false));

            // Act
            IActionResult actionResult = await this.FileApiController().Delete(id);

            // Assert
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async void DeleteFile_WithEmptyFileName_ReturnsBadObjectRequestResult()
        {
            // Arrange
            Guid fileId = Guid.Empty;

            this.mockFileService
                .Setup(s => s.DeleteFile(fileId))
                .Returns(Task.FromResult(true));

            // Act
            IActionResult actionResult = await this.FileApiController().Delete(fileId);

            // Assert
            actionResult.Should().BeOfType<BadRequestResult>();
        }

        private FilesApiController FileApiController()
        {
            var contactsApiController = new FilesApiController(
                this.mockFileService.Object
            );

            return contactsApiController;
        }

        private FilesApiController FileApiController(Guid accountId)
        {
            var contactsApiController = new FilesApiController(
                this.mockFileService.Object
            );

            return contactsApiController;
        }
    }
}
