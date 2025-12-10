
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using PlayBook3DTSL.Model.Cloud;
using PlayBook3DTSL.Repository.Interfaces.CloudStorage;
using PlayBook3DTSL.Utilities.Helpers;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Repository.Repository.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureStorageService : ICloudStorageService
    {
        private readonly BlobContainerClient _container;
        private readonly AppConfiguration _appconfiguration;
        public readonly UploadManager _uploadManager;
        private readonly IHostEnvironment _environment;

        public AzureStorageService(AppConfiguration appconfiguration, UploadManager uploadManager, IHostEnvironment environment)
        {
            _appconfiguration = appconfiguration;
            _uploadManager = uploadManager;
            _container = new BlobContainerClient(_appconfiguration.BlobStorageConnection, _appconfiguration.BlobStorageCotainer);
            _environment = environment;
        }

        public async Task UploadeFileAsync(List<CloudServiceModel> cloudServiceModel)
        {
            foreach (var file in cloudServiceModel)
            {
                if (file.IsFromSourcePath)
                {

                    if (!File.Exists(file.SourcePath))
                    {
                        continue;
                    }

                    try
                    {
                        var blobServiceClient = new BlobServiceClient(_appconfiguration.BlobStorageConnection);
                        var containerClient = blobServiceClient.GetBlobContainerClient(_appconfiguration.BlobStorageCotainer);
                        var blobClient = containerClient.GetBlobClient($"{file.CoudPath}/{file.FileName}");

                        using (FileStream fs = File.OpenRead(file.SourcePath))
                        {
                            await blobClient.UploadAsync(fs, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error uploading file: {file.SourcePath}. Error: {ex.Message}");
                    }
                }
                else
                {


                    try
                    {
                        var blob = _container.GetBlobClient($"{file.CoudPath}/{file.FileName}");
                        using (Stream stream = file.File.OpenReadStream())
                        {
                            await blob.UploadAsync(stream, true);

                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error uploading file: {file.FileName}. Error: {ex.Message}");
                    }
                }

                // Try to delete file after upload (if applicable)
                if (!string.IsNullOrWhiteSpace(file.SourcePath) && File.Exists(file.SourcePath) && !IsFileInUsed(file.SourcePath))
                {
                    try
                    {
                        File.Delete(file.SourcePath);

                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error deleting file: {file.SourcePath}. Error: {ex.Message}");
                    }
                }
            }

        }



        private bool IsFileInUsed(string filePath)
        {
            try
            {
                using (FileStream fs = File.OpenWrite(filePath))
                {
                    fs.Close();
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public async Task DeleteFileAsync(List<CloudServiceModel> cloudServiceModel)
        {
            foreach (var blob in cloudServiceModel)
            {
                BlobClient file = _container.GetBlobClient(blob.CoudPath + "/" + blob.FileName);
                await file.DeleteIfExistsAsync();
            }
        }
        public async Task<(Stream, string)> StreamFileAsync(string filePath)
        {
            return await _uploadManager.GetVideoStream(filePath);
        }
        static bool IsFolderEmpty(string folderPath)
        {
            return !Directory.EnumerateFileSystemEntries(folderPath).Any();
        }
        public async Task CopyFilesToBlobStorageAsync(string rootFolder, string newFileName)
        {
            try
            {
                bool response = false;
                Log.Error($"{"Start Time : "}{DateTime.Now}");
                string azcopyPath = $"{_environment.ContentRootPath}{@"\Azcopy\azcopy.exe"}";
                string destinationPath = $"{_appconfiguration.MediaPath}{"ProcedureSectionMedia"}";
                string[] files = Directory.GetFiles(rootFolder);
                Log.Error($"{"azcopyPath : "}{azcopyPath}");
                Log.Error($"{"destinationPath : "}{destinationPath}");
                Log.Error($"{"rootFolder : "}{rootFolder}");
                foreach (string fileName in files)
                {
                    FileInfo fileInfo = new FileInfo(fileName);
                    string arguments = $@"copy ""{rootFolder}{"/"}{fileInfo.Name}"" ""{destinationPath}{"/"}{fileInfo.Name}{_appconfiguration.sasToken}""";
                    Log.Error($"{"arguments : "}{arguments}");
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = azcopyPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false
                    };
                    Process process = new Process();
                    process.StartInfo = startInfo;
                    process.Start();
                    await process.WaitForExitAsync();
                    response = process.ExitCode == 0 || process.ExitCode == 1;
                    if (process.ExitCode.Equals(0))
                    {
                        File.Delete($"{rootFolder}//{newFileName}");
                        if (IsFolderEmpty(rootFolder))
                        {
                            Directory.Delete(rootFolder, true);
                        }
                    }
                    string output = process.StandardOutput.ReadToEnd(); // Retrieve the output after process exit
                    string err = process.StandardError.ReadToEnd();
                    // Close process by sending a close message to its main window.
                    process.CloseMainWindow();
                    // Free resources associated with process.
                    if (response)
                    {
                        Log.Logger.Write(LogEventLevel.Information, "RunProcess " + arguments + " successfully run.");
                    }
                    else
                    {
                        Log.Logger.Write(LogEventLevel.Warning, "RunProcess " + arguments + " Error." + process.ExitCode);
                    }
                    process.Close();
                    Log.Error(err);
                    Log.Error($"{"End Time : "}{DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.Message);
                throw;
            }

        }
    }
}
