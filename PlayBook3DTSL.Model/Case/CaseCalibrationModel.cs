namespace PlayBook3DTSL.Model.Case
{
    public class CaseCalibrationModel
    {
        public Guid Id { get; set; }
        public Guid? CaseResultId { get; set; }
        public string? ImageViewType { get; set; } = string.Empty;
        public string CalibrationValue { get; set; } = string.Empty;
    }
}
