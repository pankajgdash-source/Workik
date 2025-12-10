using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseResultDetailServiceModel
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public decimal? CSpineLength { get; set; }
        public decimal? SSpineLength { get; set; }
        public decimal? ThreeDTSL { get; set; } 
        public decimal? CT1S1Height { get; set; }
        public decimal? ST1S1Height { get; set; }
        public decimal? CT1T12Height { get; set; }
        public decimal? ST1T12Height { get; set; }
        public decimal? CSLT1L1 { get; set; }
        public decimal? SSLT1L1 { get; set; }
        public decimal? T1L13DTSL { get; set; }
        public long? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string Period { get; set; }=string.Empty;
        public string CaseResultName { get; set; } = string.Empty;
    }
}
