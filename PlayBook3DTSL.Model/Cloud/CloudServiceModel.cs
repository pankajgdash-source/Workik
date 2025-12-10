using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Model.Cloud
{
    [ExcludeFromCodeCoverage]
    public class CloudServiceModel
    {
        public IFormFile File { get; set; }
        public string CoudPath { get; set; }
        public string FileName { get; set; }
        public string? SourcePath { get; set; }
        public bool IsFromSourcePath { get; set; }

    }
}
