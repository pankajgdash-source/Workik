
using PlayBook3DTSL.API.ServiceInstallers.Base;
using PlayBook3DTSL.Repository.Interfaces.Case;
using PlayBook3DTSL.Repository.Interfaces.CloudStorage;
using PlayBook3DTSL.Repository.Repository.Azure;
using PlayBook3DTSL.Repository.Repository.Case;
using PlayBook3DTSL.Service.DOMMapper;
using PlayBook3DTSL.Services.Interfaces.Case;
using PlayBook3DTSL.Services.Services.Case;
using PlayBook3DTSL.Utilities.Helpers;

namespace PlayBook3DTSL.API.ServiceInstallers
{
    public class RepositoriesInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            #region Application
            services.AddTransient<TokenManager>();
            services.AddSingleton<UploadManager>();
            services.AddTransient<ConcurrencyHelpers>();
            services.AddTransient<AppConfiguration>();
            services.AddAutoMapper(typeof(MappingObjects));
            services.AddScoped<CloudStorageFactory>();
            services.AddSingleton<AzureStorageService>()
              .AddScoped<ICloudStorageService, AzureStorageService>(s => s.GetService<AzureStorageService>());
            #endregion
            //
            // Add Scoped Services
            //
            services.AddScoped<ICaseService, CaseService>(); // Register CaseService


            //
            // Add Scoped Repositories
            //
            services.AddScoped<ICaseRepository, CaseRepository>(); // Register CaseRepository

        }
    }
}
