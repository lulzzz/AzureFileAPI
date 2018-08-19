namespace AzureFileAPI.Models.Configuration
{
    public class AzureStorageSettings
    {
        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        /// <value>
        /// The connection string
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the container
        /// </summary>
        /// <value>
        /// The name of the container
        /// </value>
        public string ContainerName { get; set; }
    }
}
