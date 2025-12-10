using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseResultJsonModel
    {
        [JsonProperty("Case")]
        public string CaseResultName { get; set; }

        [JsonProperty("Case Period Created On")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("Coronal Spine Length (mm)")]
        public decimal? CspineLength { get; set; }

        [JsonProperty("Sagittal Spine Length SSL")]
        public decimal? SspineLength { get; set; }

        [JsonProperty("3D-TSL")]
        public decimal? ThreeDTSL { get; set; }

        [JsonProperty("Coronal T1-S1 height (mm)")]
        public decimal? Ct1s1height { get; set; }

        [JsonProperty("Sagittal T1-S1 height (mm)")]
        public decimal? St1s1height { get; set; }

        [JsonProperty("Coronal T1-T12 height")]
        public decimal? Ct1t12height { get; set; }

        [JsonProperty("CSL T1-L1 (mm)")]
        public decimal? Cslt1l1 { get; set; }

        [JsonProperty("SSL T1-L1 (mm)")]
        public decimal? Sslt1l1 { get; set; }

        [JsonProperty("T1-L1 3DTSL (mm)")]
        public decimal? T1l13dtsl { get; set; }

        [JsonProperty("Coronal Instrumented Length (mm)")]
        public decimal? CILmm { get; set; }

        [JsonProperty("Sagittal Instrumented Length (mm)")]
        public decimal? SILmm { get; set; }

        [JsonProperty("3D Instrument TSL")]
        public decimal? ThreeDitsl { get; set; }

        [JsonProperty("AP Pixel Size")]
        public decimal? APPixelSize { get; set; }
        [JsonProperty("Lateral Pixel Size")]
        public decimal? LateralPixelSize { get; set; }

        [JsonProperty("UIL")]
        public string? UIL { get; set; }

        [JsonProperty("LIL")]
        public string? LIL { get; set; }
    }
}
