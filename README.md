
# ![alt text](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/azure-storage-icon-small.png  "Azure Files Logo") Azure Storage API

Web API Solution demonstrates file management (CRUD) based on Azure Storage.

- Works with Azure Blob/File Storage
- Contains Upload/Download/Delete/Get metadata functions
- Uses chunks file upload mechanism
- Has multi-threads file upload (uses several threads to upload file chunks, this option configurable)
- Contains file retry upload when connection lost

# Architecture overview

![alt text](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/solution-diagram.png  "Azure Files Logo")

**File upload overview**

![solution-diagram-uml](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/solution-diagram-uml.png  "solution-diagram-uml")

## Installation

1. Clone repository
2. Create file storage account in Azure ([Follow this example](https://docs.microsoft.com/en-us/azure/storage/common/storage-quickstart-create-account?tabs=portal)) or [setup azure storage emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)
3. Update Azure storage configuration in _StorageConnection_ (`appsettings.json`) 
```javascript
"ConnectionStrings": {
        "StorageConnection": {
            "ContainerName": "[Azure Storage Container Name]",
            "ConnectionString": "[Storage ConnectionString]"
        }
},
```
4. Run UnitTests.(in progress)
5. Build / Run.

## Overview

The API, visually, can be tested via Swagger UI and with [Azure Storage Explorer APP ](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-storage-explorer).
Example below:

![api-file-upload](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/api-file-upload.png  "api-file-upload")

![azure-storage-expl](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/azure-storage-explorer.png  "azure-storage-expl")

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

# References / Tools

https://docs.microsoft.com/en-gb/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=windows 
https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy