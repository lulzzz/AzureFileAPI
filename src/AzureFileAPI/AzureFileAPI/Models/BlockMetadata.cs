namespace AzureFileAPI.Models
{
    public class BlockMetadata
    {
        /// <summary>
        /// Gets or sets the block index
        /// (offset of the source BLOB file)
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the block hash id.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        public string BlockId { get; set; }

        /// <summary>
        /// Gets or sets the block file length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; set; }
    }
}
