using System;
using System.IO;
using AzureFileAPI.Data.AzureFileStorage;
using AzureFileAPI.Models.Configuration;
using AzureFileAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace AzureFileAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AzureStorageSettings>(this.Configuration.GetSection("ConnectionStrings:StorageConnection"));

            // Swagger API documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Azure File Api",
                        Version = "v1.0",
                        Description = "Dotnet core multi tenant application",
                        TermsOfService = "TODO: Add Terms of service",
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
            });

            // IoC configuration
            services.AddTransient<IAzureFileService, AzureFileService>();
            services.AddTransient<IAzureFileStorageManager, AzureFileStorageManager>();
            services.AddTransient<IBlobFileUploadUtility, BlobFileUploadUtility>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Swagger API documentation
            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure File Api v1.0");
            });

            app.UseMvc();
        }
    }
}
