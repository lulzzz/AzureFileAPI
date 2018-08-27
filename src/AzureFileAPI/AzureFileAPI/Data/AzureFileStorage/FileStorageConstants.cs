namespace AzureFileAPI.Data.AzureFileStorage
{
    public class FileStorageConstants
    {
        /// <summary>
        /// The cloud metadata uploaded date time key
        /// </summary>
        public const string CloudMetadataUploadedDateTimeKey = "UploadedDateTime";

        /// <summary>
        /// The cloud metadata original file name key
        /// </summary>
        public const string CloudMetadataOriginalFileNameKey = "OriginalFileName";

        /// <summary>
        /// The maximum retries
        /// </summary>
        public const int MaximumRetries = 5;

        /// <summary>
        /// The default parallel upload threads
        /// </summary>
        public const int DefaultParallelUploadThreads = 20;
    }
}
