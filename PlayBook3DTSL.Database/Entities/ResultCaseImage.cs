using System;
using System.Collections.Generic;

namespace PlayBook3DTSL.Database.Entities;

public partial class ResultCaseImage
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public Guid CaseResultId { get; set; }

    public string? ImageName { get; set; }

    public string ImageViewType { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public long? CreatedBy { get; set; }
}
