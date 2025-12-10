using PlayBook3DTSL.Model.Azure;
using PlayBook3DTSL.Services.Interfaces;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;

namespace Playbook.Service.Interfaces.Azure
{
    public interface IAzureCommandService : ICRUDInterface<AzureCommandServiceModel>
    {
        ServiceResponseGeneric<AzureCommandServiceModel> GetEndpoint();
        
    }
}
