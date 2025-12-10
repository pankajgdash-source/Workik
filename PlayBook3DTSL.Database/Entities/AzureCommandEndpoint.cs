namespace PlayBook3DTSL.Database.Entities
{
    public partial class AzureCommandEndpoint : BaseEntity
    {
        public Guid Id { get; set; }
        public string EndpointId { get; set; } = null!;
    }
}
