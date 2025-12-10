using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayBook3DTSL.Database.Entities
{
    public partial class CaseMeasurementSubSteps
    {

        public Guid Id { get; set; }
        public Guid? CaseMeasurementStepsId { get; set; } // FK reference to CaseMeasurementSteps
        public string StepName { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public int? SortOrder { get; set; }

        public virtual CaseMeasurementStep CaseMeasurementStep { get; set; } = null!;

    }
}
