using System;
using System.ComponentModel.DataAnnotations;

namespace AzureFileAPI.Models
{
    /// <summary>
    /// View model for file
    /// </summary>
    public class FileViewModel
    {
        /// <summary>
        /// Gets or sets the file metadata.
        /// </summary>
        /// <value>
        /// The file metadata.
        /// </value>
        public FileMetadata FileMetadataViewModel { get; set; }

        /// <summary>
        /// Gets or sets the file content
        /// </summary>
        /// <value>
        /// The Content of the file
        /// </value>
        public byte[] Content { get; set; }
    }

    /// <summary>
    /// File's metadata db entity
    /// </summary>
    public class FileMetadata
    {
        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        /// <value>
        /// The identifier
        /// </value>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the file
        /// </summary>
        /// <value>
        /// The name of the file
        /// </value>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets the type of the MIME
        /// </summary>
        /// <value>
        /// The type of the MIME
        /// </value>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the size of the file
        /// </summary>
        /// <value>
        /// The size of the file
        /// </value>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the uploaded
        /// </summary>
        /// <value>
        /// The uploaded.
        /// </value>
        public DateTime Uploaded { get; set; }

    }
}
