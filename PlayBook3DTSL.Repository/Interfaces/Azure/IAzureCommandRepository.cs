using PlayBook3DTSL.Model.Azure;

using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;
namespace PlayBook3DTSL.Repository.Interfaces.Azure
{
    public interface IAzureCommandRepository : ICRUDInterface<AzureCommandServiceModel>
    {
        ServiceResponseGeneric<AzureCommandServiceModel> GetEndpoint();
        
    }
}
