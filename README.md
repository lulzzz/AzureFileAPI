
# ![alt text](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/azure-storage-icon.png  "Azure Files Logo") Azure Storage API

Web API Solution demonstrates file management (CRUD) based on Azure Storage.

- Works with Azure Blob/File Storage
- Contains Upload/Download/Delete/Get metadata functions
- Uses chunks file upload mechanism 
- Has multithreads file upload (uses several threads to upload file chunks, this option configurable)
- Contains file retry upload when connection lost

# Architecture overview

![alt text](https://raw.githubusercontent.com/Boriszn/AzureFileAPI/develop/assets/img/solution-diagram.png  "Azure Files Logo")

## Installation

1. Clone repository
2. Update Azure storage configuration in _StorageConnection_ (`appsettings.json`) 
```javascript
"ConnectionStrings": {
        "StorageConnection": {
            "ContainerName": "[Azure Storage Container Name]",
            "ConnectionString": "[Storage ConnectionString]"
        }
},
```
3. Run UnitTests.(in progress)
4. Build / Run.

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

# References / Tools

https://docs.microsoft.com/en-gb/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=windows 
https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy