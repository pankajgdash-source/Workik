using System;
using System.Collections.Generic;

namespace PlayBook3DTSL.Database.Entities;

public partial class CaseStudyMapping
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public string StudyId { get; set; } = null!;

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? PatientId { get; set; }

    public virtual Case Case { get; set; } = null!;
}
