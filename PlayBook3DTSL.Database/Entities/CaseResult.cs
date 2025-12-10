using System;
using System.Collections.Generic;

namespace PlayBook3DTSL.Database.Entities;

public partial class CaseResult : BaseEntity
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public decimal? CspineLength { get; set; }

    public decimal? SspineLength { get; set; }

    public decimal? ThreeDtsl { get; set; }

    public decimal? Ct1s1height { get; set; }

    public decimal? St1s1height { get; set; }

    public decimal? Ct1t12height { get; set; }

    public decimal? St1t12height { get; set; }

    public decimal? Cslt1l1 { get; set; }

    public decimal? Sslt1l1 { get; set; }

    public decimal? T1l13dtsl { get; set; }


    public decimal? Cilmm { get; set; }

    public decimal? Silmm { get; set; }

    public decimal? ThreeDitsl { get; set; }

    public decimal? AppixelSize { get; set; }

    public decimal? LateralPixelSize { get; set; }

    public string? Uil { get; set; }

    public string? Lil { get; set; }

    public string? Period { get; set; }
    public string? CaseResultName { get; set; }= string.Empty;
    public string? LabelingConfig { get; set; }

    public virtual Case Case { get; set; } = null!;
}
