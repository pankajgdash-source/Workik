using System;
using System.Collections.Generic;

namespace PlayBook3DTSL.Database.Entities;

public partial class CaseMeasurementStep
{
    public Guid Id { get; set; }

    public Guid? CaseId { get; set; }

    public string StepName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsCompleted { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public string? ImageViewType { get; set; }
    public bool IsLEVDropDown { get; set; }
    public bool IsLILDropDown { get; set; }
    public int? SortOrder { get; set; }
   public string? CasePeriod { get; set; }


    public virtual Case? Case { get; set; }

    public virtual ICollection<CaseMeasurementSubSteps> SubSteps { get; set; } = new List<CaseMeasurementSubSteps>();
}
