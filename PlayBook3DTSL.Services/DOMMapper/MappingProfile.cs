using AutoMapper;
using PlayBook3DTSL.Database.Entities;
using PlayBook3DTSL.Model.Azure;
using PlayBook3DTSL.Model.Case;
namespace PlayBook3DTSL.Service.DOMMapper
{
    public class MappingObjects : Profile
    {
        public MappingObjects()
        {
            #region Case
            CreateMap<ResultServiceModel, CaseResult>();
            CreateMap<CaseMeasurementStepsSericeModel, CaseMeasurementStep>().ReverseMap();
            CreateMap<CaseStudyMappingServiceModel, CaseMeasurementSubSteps>().ReverseMap();
            CreateMap<CaseResult, CaseResultJsonModel>().ReverseMap();
            CreateMap<CaseMeasurementStep, CaseMeasurementStepsSericeModel>().ReverseMap();
            CreateMap<CaseMeasurementSubSteps, CaseMeasurementSubStepServiceModel>().ReverseMap();




            #endregion

            #region AzureCommand
            CreateMap<AzureCommandEndpoint, AzureCommandServiceModel>().ReverseMap();
            #endregion
        }
    }
}
