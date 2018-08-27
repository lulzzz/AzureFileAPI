using AzureFileAPI.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFileAPI.Configuration
{
    public static class ConfigSettingConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureStorageSettings>(configuration.GetSection("ConnectionStrings:StorageConnection"));
        }
    }
}
