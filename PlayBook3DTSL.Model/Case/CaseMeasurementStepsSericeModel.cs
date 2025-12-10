namespace PlayBook3DTSL.Model.Case
{
    public class CaseMeasurementSubStepServiceModel
    {
        public Guid Id { get; set; }
        public Guid CaseMeasurementStepsId { get; set; }
        public string StepName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CaseMeasurementStepsSericeModel
    {
        public Guid Id { get; set; }
        public Guid? CaseId { get; set; }
        public string StepName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageViewType { get; set; }
        public bool IsCompleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public bool IsLEVDropDown { get; set; }
        public bool IsLILDropDown { get; set; }
        public int? SortOrder { get; set; }

        // Nested substeps
        public List<CaseMeasurementSubStepServiceModel> SubSteps { get; set; } = new List<CaseMeasurementSubStepServiceModel>();
    }
}
