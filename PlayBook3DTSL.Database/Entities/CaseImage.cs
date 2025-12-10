using System;
using System.Collections.Generic;

namespace PlayBook3DTSL.Database.Entities;

public partial class CaseImage
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public string FileName { get; set; } = null!;

    public string? StudyInstanceUid { get; set; }

    public DateTime? StudyDate { get; set; }

    public string? StudyDescription { get; set; }

    public string? SeriesInstanceUid { get; set; }

    public string? SopinstanceUid { get; set; }

    public string? SeriesName { get; set; }

    public string? PixelSpacing { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }
    public string? CasePeriod {  get; set; }

    public virtual Case Case { get; set; } = null!;
}
