using AzureFileAPI.Data.AzureFileStorage;
using AzureFileAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFileAPI.Configuration
{
    public static class IoCConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // IoC configuration
            services.AddTransient<IAzureFileService, AzureFileService>();
            services.AddTransient<IAzureFileStorageManager, AzureFileStorageManager>();
            services.AddTransient<IBlobFileUploadUtility, BlobFileUploadUtility>();
        }
    }
}
