using PlayBook3DTSL.Model.Cloud;

namespace PlayBook3DTSL.Repository.Interfaces.CloudStorage
{
    public interface ICloudStorageService
    {
        Task UploadeFileAsync(List<CloudServiceModel> cloudServiceModel);
        Task DeleteFileAsync(List<CloudServiceModel> cloudServiceModel);
        Task<(Stream, string)> StreamFileAsync(string filePath);
        Task CopyFilesToBlobStorageAsync(string rootFolder,string fileName);
    }
}
