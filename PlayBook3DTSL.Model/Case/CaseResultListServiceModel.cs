using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Model.Case
{
    public class CaseResultListServiceModel
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public string? Period { get; set; }
        public string CaseResultName { get; set; }
        public string CaseName { get; set; }
        public bool IsLatest { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }


    }
}
