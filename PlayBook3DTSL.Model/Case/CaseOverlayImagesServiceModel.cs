using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseOverlayImagesServiceModel
    {
        public Guid CaseId { get; set; }
        public Guid CaseResultId { get; set; }
        public string ImageName { get; set; }
        public string ImageViewType { get; set; }
        public string CasePeriod { get; set; }


    }
}