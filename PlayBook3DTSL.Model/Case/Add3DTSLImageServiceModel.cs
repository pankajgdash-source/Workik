using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{

    public class Add3DTSLImageNameServiceModel
    {
        public string ImageName { get; set; }
        public string ImageViewType { get; set; }
    }
    public class Add3DTSLImageServiceModel
    {
        public Guid CaseId { get; set; }
        public Guid CaseResultId { get; set; }
        public string? CasePeriod { get; set; }
        public List<IFormFile> FilePath { get; set; } = new List<IFormFile>();
        public List<CaseImagesNameServiceModel> ImageList { get; set; } = new List<CaseImagesNameServiceModel>();
    }
}
