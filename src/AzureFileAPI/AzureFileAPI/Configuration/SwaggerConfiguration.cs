using AzureFileAPI.ApiFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace AzureFileAPI.Configuration
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Swagger API documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Azure File Api",
                        Version = "v1.0",
                        Description = "Azure BLOB File Management",
                        Contact = new Contact
                        {
                            Name = "Boris Zaikin"
                        },
                        License = new License
                        {
                            Name = "MIT License",
                            Url = "https://opensource.org/licenses/MIT"
                        }
                    });

                c.OperationFilter<FileOperationFilter>();
            });
        }

        public static void Configure(IApplicationBuilder app)
        {
            // Swagger API documentation
            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure File Api v1.0");
            });
        }
    }
}
