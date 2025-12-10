using PlayBook3DTSL.Model.Case;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;

namespace PlayBook3DTSL.Repository.Interfaces.Case
{
    public interface ICaseRepository
    {
        ServiceResponseGeneric<CaseResultListServiceModel> CreateCaseAsync(CaseCreateServiceModel caseDetailServiceModel);
        ServiceResponseGeneric<CaseResultListServiceModel> CreateResultByCaseIdAsync(Guid caseId, string period);
        ServiceResponseGeneric<List<ResultServiceModel>> GetResultByCaseId(Guid caseId);
        ServiceResponseGeneric<List<CaseDetailServiceModel>> GetCasesByHospital();
        Task<ServiceResponseGeneric<List<CaseImageDetailResponse>>> GetCaseImages(GetCaseImageDetailServiceModel getCaseImageDetailServiceModel);
        ServiceResponseGeneric<Task<bool>> AddCaseImages(CaseImagesServiceModel caseImagesServiceModel);
        ServiceResponseGeneric<Task<bool>> Add3DTSLImages(Add3DTSLImageServiceModel add3DTSLImageServiceModel);
        ServiceResponseGeneric<Task<bool>> AddCaseStudy(CaseStudyServiceModel caseStudyMappingServiceModel);
        ServiceResponseGeneric<List<CaseStudyMappingServiceModel>> GetCaseStudy(Guid caseId);
        ServiceResponseGeneric<List<CaseMeasurementStepsSericeModel>> GetCaseMeasurementStepsByCaseId(Guid caseId, string casePeriod);
        ServiceResponseGeneric<Task<bool>> UpdateCaseMeasurementStepsId(Guid caseMeasurementStepsId, bool isCompleted);
        ServiceResponseGeneric<Task<bool>> UpdateCaseMeasurementSubStepsId(Guid caseMeasurementSubStepsId, bool isCompleted);
        ServiceResponseGeneric<Task<bool>> UpdateConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel);

        ServiceResponseGeneric<List<ConfigureViewModel>> GetConfigurationByCaseResultId(Guid caseResultId);

        ServiceResponseGeneric<CaseResultDetailServiceModel> GetCaseResultDetailById(Guid caseResultId);

        ServiceResponseGeneric<Guid> Add3DTSLCalculation(ResultServiceModel resultServiceModel);

        ServiceResponseGeneric<Task<bool>> Check3DTSLDataExists(Guid resultId);

        ServiceResponseGeneric<List<CaseResultListServiceModel>> GetResultListByCaseId(Guid caseId);
        ServiceResponseGeneric<Task<bool>> UpdateLabellingConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel);

        ServiceResponseGeneric<Task<string>> GetLabellingConfigData(Guid caseResultId);
        ServiceResponseGeneric<string> GetCaseImageZip(List<Guid> caseResultIds);

        ServiceResponseGeneric<string> GetCaseResultsCsv(CaseImageDetailServiceModel caseImageDetailServiceModel);

        ServiceResponseGeneric<CaseDetailServiceModel> GetCaseDetailById(Guid caseId);

        ServiceResponseGeneric<List<CaseOverlayImagesServiceModel>> GetOverlayImages(List<Guid> caseResultIds);

        ServiceResponseGeneric<Task<bool>> UpdateResultImage(Guid caseResultId, string imageViewType, string imageName);
        ServiceResponseGeneric<string> GetCaseImagePDF(List<Guid> caseResultIds);

        ServiceResponseGeneric<Task<string>> GetCaseResultsJSON(List<Guid> ids);

        ServiceResponseGeneric<Task<string>> GetCaseResultsZip(List<Guid> ids);

        ServiceResponseGeneric<List<CaseCalibrationModel>> GetCalibrationValue(Guid caseResultId);

        ServiceResponseGeneric<bool> UpdateCalibrationValue(CaseCalibrationModel caseCalibrationModel);
        ServiceResponseGeneric<bool> UpdateCaseName(UpdateCaseNameServiceModel updateCaseNameServiceModel);


    }
}


