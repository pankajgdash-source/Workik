namespace PlayBook3DTSL.Model.Hospital
{
    public class HospitalModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? DisplayAddress { get; set; }
        public bool IsActive { get; set; }
        public int OrRoom { get; set; }
        public PatientIdentifierType PatientIdentifierType { get; set; }
        public string? Pacshost { get; set; }
        public int? Pacsport { get; set; }
        public string? PacscallingAe { get; set; }
        public string? PacscalledAe { get; set; }
        public string? TimeZone { get; set; }
    }

    public enum PatientIdentifierType
    {
        MRN = 1,
        EMR = 2,
        EHR = 3,
    }
}
