using AutoMapper;
using Playbook.Service.Interfaces.Azure;
using System.Net;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PlayBook3DTSL.Model.Azure;
using PlayBook3DTSL.Repository.Interfaces.Azure;

namespace Playbook.Services.Services.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureCommandService : IAzureCommandService
    {
        private readonly IAzureCommandRepository _azureRepository;

        public AzureCommandService(IAzureCommandRepository azureRepository)
        {
            _azureRepository = azureRepository;
        }
        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<bool> Create(AzureCommandServiceModel azureCommandServiceModel)
        {
            return _azureRepository.Create(azureCommandServiceModel);
        }

        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<bool> Delete(Guid id)
        {
            throw new NotImplementedException();
        }
        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<List<AzureCommandServiceModel>> GetAll()
        {
            throw new NotImplementedException();
        }
        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<AzureCommandServiceModel> GetEndpoint()
        {
            return _azureRepository.GetEndpoint();            
        }

        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<AzureCommandServiceModel> GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<bool> Update(AzureCommandServiceModel azureCommandServiceModel)
        {
            return _azureRepository.Update(azureCommandServiceModel);
            
        }
    }
}
