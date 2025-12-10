using Microsoft.AspNetCore.Http;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseStudyMappingServiceModel 
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public string StudyId { get; set; } = string.Empty;
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PatientId { get; set; } = string.Empty;

    }

    public class CaseStudyServiceModel
    {
        public Guid CaseId { get; set; }
        public List<string> StudyIds { get; set; } = null!;
        public string PatientId { get; set; } = string.Empty;
    }

    public class CaseImagesNameServiceModel
    {
        public string ImageName { get; set; }
        public string? ImageViewType { get; set; }
        public string? DICOMImage { get; set; }
    }
    public class CaseImagesServiceModel
    {
        public Guid CaseId { get; set; }
        public Guid CaseResultId { get; set; }
        public string? CasePeriod {  get; set; }
        public List<IFormFile> FilePath { get; set; } = new List<IFormFile>();
        public List<CaseImagesNameServiceModel> ImageList { get; set; } = new List<CaseImagesNameServiceModel>();

       
    }
}