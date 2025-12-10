using PlayBook3DTSL.Utilities.Enum;
using PlayBook3DTSL.Repository.Repository.Azure;

namespace PlayBook3DTSL.Repository.Interfaces.CloudStorage
{
    public class CloudStorageFactory
    {
        private readonly IServiceProvider serviceProvider;
        public CloudStorageFactory()
        {
                
        }
        public CloudStorageFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public ICloudStorageService GetCloudService(CloudStorageName cloudStorageName)
        {
            if (cloudStorageName == CloudStorageName.Azure)
            {
                return (ICloudStorageService)serviceProvider.GetService(typeof(AzureStorageService));
            }
            return (ICloudStorageService)serviceProvider.GetService(typeof(AzureStorageService));
        }
    }
}
