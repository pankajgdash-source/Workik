using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{
    public class ResultServiceModel
    {
        public Guid? Id { get; set; }
        public Guid CaseId { get; set; }
        public decimal? CspineLength { get; set; }  
        public decimal? SspineLength { get; set; }
        public decimal? ThreeDTSL { get; set; }
        public decimal? Ct1s1height { get; set; }
        public decimal? St1s1height { get; set; }
        public decimal? Ct1t12height { get; set; }
        public decimal? St1t12height { get; set; }
        public decimal? Cslt1l1 { get; set; }
        public decimal? Sslt1l1 { get; set; }
        public decimal? T1l13dtsl { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public decimal? CILmm { get; set; }
        public decimal? SILmm { get; set; }
        public decimal? APPixelSize { get; set; }
        public decimal? ThreeDitsl { get; set;}
        public decimal? LateralPixelSize { get; set; }
        public string? UIL { get; set; }
        public string? LIL { get; set; }
        public string? Period { get; set; }
        public string CaseResultName { get; set; }
        public string CaseName { get; set; }
        public bool IsLatest { get; set; }
    }
}
