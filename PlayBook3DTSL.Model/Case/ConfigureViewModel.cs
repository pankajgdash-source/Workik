using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{
    public class ConfigureViewModel
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public Guid CaseResultId { get; set; }
        public string ConfigData { get; set; } = string.Empty;
        public string RotateState { get; set; } = string.Empty;
        public string ToolState { get; set; } = string.Empty;
        public string LabelingConfig { get; set; } = string.Empty ;
        public string ImageViewType { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;

    }
}
