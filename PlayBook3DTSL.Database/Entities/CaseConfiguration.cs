namespace PlayBook3DTSL.Database.Entities;

public partial class CaseConfiguration:BaseEntity
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public Guid CaseResultId { get; set; }

    public string ImageViewType { get; set; } = null!;

    public string? CaseResultImageDetail { get; set; }

    public string? CaseResultRotateState { get; set; }

    public string? CaseResultToolState { get; set; }

    public string? ImageName { get; set; }
    public string? DICOMImage { get; set; }

    public string? CalibrationValue { get; set; }
}
