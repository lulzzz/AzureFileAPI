using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AzureFileAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFileAPI.Data.AzureFileStorage
{
    public class BlobFileUploadUtility : IBlobFileUploadUtility
    {
        /// <summary>
        /// A block may be up to 4 MB in size.
        /// </summary>
        private const int NumBytesPerBlock = 4194304;

        /// <inheritdoc/>
        public async Task UploadAsync(byte[] blobFileArray, CloudBlockBlob blockBlob, int parallelUploadThreads = FileStorageConstants.DefaultParallelUploadThreads)
        {
            // Step 1. Create blocks files/chunks from BLOB file
            List<BlockMetadata> allBlockInFile = this.CreateBlocksFromBlobFile(blobFileArray.Length);

            // Step 2. Check which blocks are already uploaded
            List<BlockMetadata> missingBlocks = await this.RetrieveMissingBlocks(blockBlob, allBlockInFile);

            // Step 3. Defining a function to get raw blocks data from requester and Upload blocks to Azure
            bool allBlocksHaveFileData = await this.StartUploadingBlocksToAzure(blobFileArray, blockBlob, parallelUploadThreads, missingBlocks);

            // Step 4. Compile all the blocks within Azure to produce a single file
            await this.CompileAllBlocksToSingleFile(blockBlob, allBlocksHaveFileData, allBlockInFile);
        }

        private byte[] CreateBlockDataArray(byte[] inputArray, int blockDataIndex, int blockDataLength)
        {
            byte[] result = new byte[blockDataLength];

            Array.Copy(inputArray, blockDataIndex, result, 0, blockDataLength);

            return result;
        }

        private async Task<bool> StartUploadingBlocksToAzure(byte[] blobFileArray, CloudBlockBlob blockBlob, int parallelUploadThreads, List<BlockMetadata> missingBlocks)
        {
            bool allBlocksHaveFileData = true;

            // Defining a function to get raw blocks' data from requester and upload to Azure
            async Task UploadBlockAsync(BlockMetadata block)
            {
                byte[] blockData = this.CreateBlockDataArray(blobFileArray, block.Index, block.Length);

                if (blockData.Length <= 0)
                {
                    // Skip this block as there is no data yet
                    allBlocksHaveFileData = false;
                    return;
                }

                // Generate content hash
                string contentHash = Convert.ToBase64String(MD5.Create().ComputeHash(blockData));

                // Starts Upload (with retry mechanism) data to Azure
                await this.RetryIfErrorAsync(async () =>
                {
                    // Upload/Put one file-block to Azure
                    await blockBlob.PutBlockAsync(
                        blockId: block.BlockId,
                        blockData: new MemoryStream(blockData, true),
                        contentMD5: contentHash,
                        accessCondition: AccessCondition.GenerateEmptyCondition(),
                        options: new BlobRequestOptions { StoreBlobContentMD5 = true, UseTransactionalMD5 = true },
                        operationContext: new OperationContext());
                });
            }

            // Execute the above defined function to upload blocks/chunks data if available
            await this.ForEachAsync(missingBlocks, parallelUploadThreads, UploadBlockAsync);

            return allBlocksHaveFileData;
        }

        // Compile all the blocks on Azure to produce a single file.(Step 3)
        private async Task CompileAllBlocksToSingleFile(CloudBlockBlob blockBlob, bool allBlocksHaveFileData, List<BlockMetadata> allBlockInFile)
        {
            List<string> blockIdList = allBlockInFile.Select(fileBlock => fileBlock.BlockId).ToList();

            await this.RetryIfErrorAsync(async () =>
            {
                // Are all the blocks valid and have data?
                if (allBlocksHaveFileData)
                {
                    await blockBlob.PutBlockListAsync(blockIdList);
                }
            });
        }

        /// <summary>
        /// Checks and gets all not uploaded file blocks
        /// NOTICE: Function intended to continue upload process from not uploaded file blocks.
        /// </summary>
        /// <param name="blockBlob">The block BLOB.</param>
        /// <param name="allBlockInFile">All blocks in file.</param>
        /// <returns>The List of Block Metadata</returns>
        private async Task<List<BlockMetadata>> RetrieveMissingBlocks(CloudBlockBlob blockBlob, List<BlockMetadata> allBlockInFile)
        {
            List<BlockMetadata> missingBlocks;

            try
            {
                // Download all uploaded blocks
                IEnumerable<ListBlockItem> downloadedBlockItems = await blockBlob.DownloadBlockListAsync(
                    BlockListingFilter.Uncommitted,
                    AccessCondition.GenerateEmptyCondition(),
                    new BlobRequestOptions(),
                    new OperationContext());

                // Filter uploaded blocks by length
                List<ListBlockItem> existingBlocks = downloadedBlockItems
                    .Where(listBlockItem => listBlockItem.Length == NumBytesPerBlock).ToList();

                // Retrieve missing blocks (from already uploaded blocks)
                missingBlocks = allBlockInFile.Where(blockInFile => !existingBlocks.Any(existingBlock =>
                    existingBlock.Name == blockInFile.BlockId &&
                    existingBlock.Length == blockInFile.Length)).ToList();
            }
            catch (StorageException)
            {
                missingBlocks = allBlockInFile;
            }

            return missingBlocks;
        }

        private List<BlockMetadata> CreateBlocksFromBlobFile(int blobLength)
        {
            // Which blocks exist in the file
            List<BlockMetadata> allBlockInFile = Enumerable
                .Range(0, 1 + (blobLength / NumBytesPerBlock))
                .Select(blockId => CreateBlockMetadata(blockId, blobLength, NumBytesPerBlock))
                .Where(block => block.Length > 0)
                .ToList();

            // Creates file block metadata item
            BlockMetadata CreateBlockMetadata(int id, int length, int bytesPerChunk)
            {
                int index = id * bytesPerChunk;
                int remainingBytesInFile = length - index;

                return new BlockMetadata
                {
                    BlockId = Convert.ToBase64String(BitConverter.GetBytes(id)),
                    Index = index,
                    Length = Math.Min(remainingBytesInFile, bytesPerChunk)
                };
            }

            return allBlockInFile;
        }

        /// <summary>
        /// TODO: Move to extension within shared lib after task TELE-1677 will be done
        /// Retries/Executes block upload several times in case of error
        /// NOTICE: Retries action callback (Transient Faults pattern https://msdn.microsoft.com/en-us/library/hh680901(v=pandp.50).aspx),
        /// configures via 'MaximumRetries' constant
        /// </summary>
        /// <param name="action">The Action callback method.</param>
        /// <returns>The Task.</returns>
        private async Task RetryIfErrorAsync(Func<Task> action)
        {
            var currentExecuteUntilSuccessRetries = 0;
            Exception error = null;

            // Will retry (until MaximumRetries will be reached) action operation if error(s) occurs
            while (currentExecuteUntilSuccessRetries < FileStorageConstants.MaximumRetries)
            {
                currentExecuteUntilSuccessRetries++;

                try
                {
                    await action();
                    return;
                }
                catch (Exception e)
                {
                    error = e;
                }
            }

            if (error is null)
            {
                error = new Exception("Unknown error occurred while executing upload to Azure");
            }

            throw error;
        }

        /// <summary>
        /// TODO: Move to collection extension within shared lib after task TELE-1677 will be done
        /// Multi-thread implementation of ForEach based on Partitioner
        /// </summary>
        /// <returns>The each async.</returns>
        /// <param name="source">Source.</param>
        /// <param name="parallelUploadThreads">Parallel uploads.</param>
        /// <param name="callback">The Callback method</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private Task ForEachAsync<T>(IEnumerable<T> source, int parallelUploadThreads, Func<T, Task> callback)
        {
            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(parallelUploadThreads)
                    .Select(partition => Task.Run(async () =>
                    {
                        using (partition)
                        {
                            while (partition.MoveNext())
                            {
                                await callback(partition.Current);
                            }
                        }
                    })));
        }
    }
}
