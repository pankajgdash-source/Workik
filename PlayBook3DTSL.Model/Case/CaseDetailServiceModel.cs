using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PlayBook3DTSL.Model.Case
{
    public class CaseCreateServiceModel
    {
        public Guid Id { get; set; }
        public Guid CaseResultId { get; set; }
        public int CaseId { get; set; }
        public string CaseName { get; set; }
        public string Period { get; set; } = string.Empty;
        public Guid HospitalId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string PatientName { get; set; } = string.Empty;

        public CaseStudyServiceModel? CaseStudyMappingServiceModel { get; set; } = null;

    }

    public class UpdateCaseNameServiceModel
    {
        public Guid CaseId { get; set; }
        public string CaseName { get; set; }
    }


    public class CaseDetailServiceModel
    {
        public Guid Id { get; set; }
        public int CaseId { get; set; }
        public string CaseName { get; set; }
        public string Period { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CaseResultName { get; set; }
        public Guid NewResultId { get; set; }
    }
}
