using PlayBook3DTSL.Utilities.Enum;
using System;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseUpdateConfigServiceModel
    {

        public Guid CaseId { get; set; }
        public Guid CaseResultId { get; set; }
        public ImageViewType ImageViewType { get; set; }
        public ConfigType ConfigType { get; set; }
        public string ConfigData { get; set; } = string.Empty;
        public string RotateState { get; set; } = null!;
        public string ToolState { get; set; } = null!;
        public List<string> AddedSubStepsLabel { get; set; } = new();
        public List<string> RemovedSubStepsLabel { get; set; } = new();
        public string casePeriod { get; set; } = string.Empty;

    }
}
