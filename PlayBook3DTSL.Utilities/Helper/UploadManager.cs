using Azure.Storage.Blobs;
using System.Diagnostics.CodeAnalysis;


namespace PlayBook3DTSL.Utilities.Helpers
{
    [ExcludeFromCodeCoverage]
    public class UploadManager
    {
        private readonly BlobContainerClient _blobcontainer;
        private readonly AppConfiguration _appconfiguration;

        public UploadManager(AppConfiguration appconfiguration)
        {
            _appconfiguration = appconfiguration;
            _blobcontainer = new BlobContainerClient(_appconfiguration.BlobStorageConnection, _appconfiguration.BlobStorageCotainer);
        }
        
        public async Task<(Stream, string)> GetVideoStream(string filePath)
        {
            BlobClient blobClient = _blobcontainer.GetBlobClient($"{ApplicationHelpers.GetDocuemnt}{filePath}");
            var properties = await blobClient.GetPropertiesAsync();
            string contentType = properties.Value.ContentType;
            Stream stream = await blobClient.OpenReadAsync();
            return (stream, contentType);
        }

    }
}
