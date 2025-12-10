using PlayBook3DTSL.Model.Case;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseCreateResultServiceModel
    {
        public Guid Id { get; set; }
        public Guid CaseResultId { get; set; }
        public int CaseId { get; set; }
        public string CaseName { get; set; }
        public string Period { get; set; } = string.Empty;
        public Guid HospitalId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string PatientName { get; set; } = string.Empty;

        public CaseStudyServiceModel? CaseStudyMappingServiceModel { get; set; } = null;
    }
}
