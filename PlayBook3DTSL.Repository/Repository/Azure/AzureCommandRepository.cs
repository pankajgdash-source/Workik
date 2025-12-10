using AutoMapper;
using System.Net;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;
using PlayBook3DTSL.Utilities.Enum;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PlayBook3DTSL.Repository.Interfaces.Azure;
using PlayBook3DTSL.Model.Azure;
using PlayBook3DTSL.Utilities.Helpers;
using PlayBook3DTSL.Utilities.Constant;
using PlayBook3DTSL.Database.DataContext;
using PlayBook3DTSL.Database.Entities;

namespace PlayBook3DTSL.Repository.Repository.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureCommandRepository : IAzureCommandRepository
    {
        private readonly PlayBook3DTSLDbContext _context;
        private readonly ConcurrencyHelpers _concurrencyHelpers;
        private readonly IMapper _mapper;

        public AzureCommandRepository(PlayBook3DTSLDbContext context, IMapper mapper, ConcurrencyHelpers concurrencyHelpers)
        {
            _context = context;
            _concurrencyHelpers = concurrencyHelpers;
            _mapper = mapper;
        }
        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<bool> Create(AzureCommandServiceModel azureCommandServiceModel)
        {
            return new ServiceResponseGeneric<bool>(() =>
            {
                if (_context.AzureCommandEndpoints.AsNoTracking().Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.AzureCommand.EndPoint, HttpStatusCode.PreconditionFailed);
                }
                var azureCommand = _mapper.Map<AzureCommandEndpoint>(azureCommandServiceModel);
                _concurrencyHelpers.SetDefaultValueInsert(azureCommand);
                _context.AzureCommandEndpoints.Add(azureCommand);
                _context.SaveChanges();
                return true;
            });
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
            return new ServiceResponseGeneric<AzureCommandServiceModel>(() =>
            {
                var endPointDetail = _context.AzureCommandEndpoints.AsNoTracking().FirstOrDefault();
                if (endPointDetail == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.AzureCommand.NotFount, HttpStatusCode.PreconditionFailed);
                }
                return _mapper.Map<AzureCommandServiceModel>(endPointDetail);
            });
        }

        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<AzureCommandServiceModel> GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public ServiceResponseGeneric<bool> Update(AzureCommandServiceModel azureCommandServiceModel)
        {
            return new ServiceResponseGeneric<bool>(() =>
            {
                var endPointDetails = _context.AzureCommandEndpoints.AsNoTracking().FirstOrDefault(d => d.Id.Equals(azureCommandServiceModel.Id));
                if (endPointDetails == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                _mapper.Map<AzureCommandServiceModel, Database.Entities.AzureCommandEndpoint>(azureCommandServiceModel, endPointDetails);
                _concurrencyHelpers.SetDefaultValueUpdate(endPointDetails);
                _context.AzureCommandEndpoints.Update(endPointDetails);
                _context.SaveChanges();
                return true;
            });
        }
    }
}
