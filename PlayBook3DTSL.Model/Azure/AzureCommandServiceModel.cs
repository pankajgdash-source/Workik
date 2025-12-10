using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Model.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureCommandServiceModel
    {
        public Guid Id { get; set; }
        public string EndpointId { get; set; } = null!;
    }
}
