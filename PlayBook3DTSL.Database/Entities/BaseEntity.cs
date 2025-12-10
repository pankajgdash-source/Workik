namespace PlayBook3DTSL.Database.Entities
{
    public class BaseEntity
    {
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}
