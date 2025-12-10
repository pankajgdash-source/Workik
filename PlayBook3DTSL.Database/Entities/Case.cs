using System;
using System.Collections.Generic;

namespace PlayBook3DTSL.Database.Entities;

public partial class Case
{
    public Guid Id { get; set; }

    public int CaseId { get; set; }

    public string CaseName { get; set; } = null!;

    public string Period { get; set; } = null!;

    public Guid HospitalId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public virtual ICollection<CaseImage> CaseImages { get; set; } = new List<CaseImage>();

    public virtual ICollection<CaseMeasurementStep> CaseMeasurementSteps { get; set; } = new List<CaseMeasurementStep>();

    public virtual ICollection<CaseResult> CaseResults { get; set; } = new List<CaseResult>();

    public virtual ICollection<CaseStudyMapping> CaseStudyMappings { get; set; } = new List<CaseStudyMapping>();
}
