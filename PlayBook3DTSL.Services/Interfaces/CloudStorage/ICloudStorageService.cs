using PlayBook3DTSL.Model.Cloud;

namespace Playbook.Service.Interfaces.CloudStorageService
{
    public interface ICloudStorageService
    {
        Task UploadeFileAsync(List<CloudServiceModel> cloudServiceModel);
        Task DeleteFileAsync(List<CloudServiceModel> cloudServiceModel);
        Task<(Stream, string)> StreamFileAsync(string filePath);
        Task CopyFilesToBlobStorageAsync(string rootFolder,string fileName);
    }
}
