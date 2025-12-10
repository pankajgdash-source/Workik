using Microsoft.AspNetCore.Http;

namespace PlayBook3DTSL.Model.Common
{
    public class PDFToImageDetailServiceModel
    {
        public int Order { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string ImageFileName { get; set; } = string.Empty;
        public string DCMFileName { get; set; } = string.Empty;
        public Guid SopinstanceUID { get; set; }
    }
}
