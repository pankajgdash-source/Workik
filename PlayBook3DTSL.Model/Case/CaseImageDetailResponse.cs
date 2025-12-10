//using PlayBook3DTSL.Database.Entities;

using PlayBook3DTSL.Database.Entities;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseImageDetailResponse
    {
        public DateTime? StudyDate { get; set; }
        public string StudyDescription { get; set; } = null!;
        public int NumberOfInstance { get; set; }
        public List<CaseImageList> CaseStudyList { get; set; }
    }

    public class CaseImageList
    {
        public List<CaseImage> CaseImageSeries { get; set; }
    }

    public class GetCaseImageDetailServiceModel
    {
        public Guid CaseId { get; set; }
        public Guid HospitalId { get; set; }
        public string? CasePeriod { get; set; }
    }
}
