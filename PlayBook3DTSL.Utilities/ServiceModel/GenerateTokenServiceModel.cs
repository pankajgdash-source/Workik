namespace PlayBook3DTSL.Utilities.ServiceModel
{
    public class GenerateTokenServiceModel
    {
        public Guid? HospitalId { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public long Id { get; set; }
        public long? SurgeonUserId { get; set; }
        public bool IsMirrorMode { get; set; } = false;

    }
}
