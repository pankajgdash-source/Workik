using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;
using PlayBook3DTSL.Model.Case;
using PlayBook3DTSL.Services.Interfaces.Case;
using PlayBook3DTSL.Repository.Interfaces.Case;
using Org.BouncyCastle.Crypto;

namespace PlayBook3DTSL.Services.Services.Case
{
    public class CaseService : ICaseService
    {
        private readonly ICaseRepository _caseRepository;
        public CaseService(ICaseRepository caseRepository)
        {
            _caseRepository = caseRepository;
        }

        public ServiceResponseGeneric<CaseResultListServiceModel> CreateCaseAsync(CaseCreateServiceModel caseCreateServiceModel)
        {
            return _caseRepository.CreateCaseAsync(caseCreateServiceModel);
        }


        // TODO - Add feature to remove the case study mapping
        public ServiceResponseGeneric<Task<bool>> AddCaseStudy(CaseStudyServiceModel caseStudyMappingServiceModel)
        {
            return _caseRepository.AddCaseStudy(caseStudyMappingServiceModel);
        }

        public ServiceResponseGeneric<Task<bool>> AddCaseImages(CaseImagesServiceModel caseImagesServiceModel)
        {
            return _caseRepository.AddCaseImages(caseImagesServiceModel);
        }

        public ServiceResponseGeneric<List<ResultServiceModel>> GetResultByCaseId(Guid caseId)
        {
            return _caseRepository.GetResultByCaseId(caseId);
        }

        public ServiceResponseGeneric<List<CaseResultListServiceModel>> GetResultListByCaseId(Guid caseId)
        {
            return _caseRepository.GetResultListByCaseId(caseId);
        }


        public async Task<ServiceResponseGeneric<List<CaseImageDetailResponse>>> GetCaseImages(GetCaseImageDetailServiceModel getCaseImageDetailServiceModel)
        {
            return await _caseRepository.GetCaseImages(getCaseImageDetailServiceModel);
        }

        #region Case Measurement Steps
        public ServiceResponseGeneric<List<CaseMeasurementStepsSericeModel>> GetCaseMeasurementStepsByCaseId(Guid caseId,string casePeriod)
        {
            return _caseRepository.GetCaseMeasurementStepsByCaseId(caseId,casePeriod);
        }

        public ServiceResponseGeneric<Task<bool>> UpdateCaseMeasurementStepsId(Guid caseMeasurementStepsId, bool isCompleted)
        {
            return _caseRepository.UpdateCaseMeasurementStepsId(caseMeasurementStepsId, isCompleted);
        }

        public ServiceResponseGeneric<Task<bool>> UpdateCaseMeasurementSubStepsId(Guid caseMeasurementSubStepsId, bool isCompleted)
        {
            return _caseRepository.UpdateCaseMeasurementSubStepsId(caseMeasurementSubStepsId, isCompleted);
        }


        public ServiceResponseGeneric<List<CaseStudyMappingServiceModel>> GetCaseStudy(Guid caseId)
        {
            return _caseRepository.GetCaseStudy(caseId);
        }
        #endregion Case Measurement Steps

        public ServiceResponseGeneric<Task<bool>> UpdateLabellingConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel)
        {
            return _caseRepository.UpdateLabellingConfigData(caseUpdateConfigServiceModel);
        }
        public ServiceResponseGeneric<Task<string>> GetLabellingConfigData(Guid caseResultId)
        {
            return _caseRepository.GetLabellingConfigData(caseResultId);
        }
        public ServiceResponseGeneric<Task<bool>> UpdateConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel)
        {
            return _caseRepository.UpdateConfigData(caseUpdateConfigServiceModel);
        }

        public ServiceResponseGeneric<Task<bool>> Check3DTSLDataExists(Guid caseResultId)
        {
            return _caseRepository.Check3DTSLDataExists(caseResultId);
        }



        public ServiceResponseGeneric<List<ConfigureViewModel>> GetConfigurationByCaseResultId(Guid caseResultId)
        {
            return _caseRepository.GetConfigurationByCaseResultId(caseResultId);
        }


        public ServiceResponseGeneric<CaseResultDetailServiceModel> GetCaseResultDetailById(Guid caseResultId)
        {
            return _caseRepository.GetCaseResultDetailById(caseResultId);
        }

        public ServiceResponseGeneric<Guid> Add3DTSLCalculation(ResultServiceModel resultServiceModel)
        {
            return _caseRepository.Add3DTSLCalculation(resultServiceModel);
        }

        public ServiceResponseGeneric<CaseDetailServiceModel> GetCaseDetailById(Guid caseId)
        {
            return _caseRepository.GetCaseDetailById(caseId);
        }

        public ServiceResponseGeneric<Task<bool>> Add3DTSLImages(Add3DTSLImageServiceModel add3DTSLImageServiceModel)
        {

            return _caseRepository.Add3DTSLImages(add3DTSLImageServiceModel);
        }


        public ServiceResponseGeneric<CaseResultListServiceModel> CreateResultByCaseIdAsync(Guid caseId, string period)
        {
            return _caseRepository.CreateResultByCaseIdAsync(caseId, period);
        }

        public ServiceResponseGeneric<List<CaseDetailServiceModel>> GetCasesByHospital()
        {
            return _caseRepository.GetCasesByHospital();
        }

        public ServiceResponseGeneric<string> GetCaseImageZip(List<Guid> caseResultIds)
        {
            return _caseRepository.GetCaseImageZip(caseResultIds);
        }

        public ServiceResponseGeneric<string> GetCaseResultsCsv(CaseImageDetailServiceModel caseImageDetailServiceModel)
        {
            return (_caseRepository.GetCaseResultsCsv(caseImageDetailServiceModel));
        }

        public ServiceResponseGeneric<List<CaseOverlayImagesServiceModel>> GetOverlayImages(List<Guid> caseResultIds)
        {
            return _caseRepository.GetOverlayImages(caseResultIds);
        }

        public ServiceResponseGeneric<Task<bool>> UpdateResultImage(Guid caseResultId, string imageViewType, string imageName)
        {
            return (_caseRepository.UpdateResultImage(caseResultId, imageViewType, imageName));
        }

        public ServiceResponseGeneric<string> GetCaseImagePDF(List<Guid> caseResultIds)
        {
            return (_caseRepository.GetCaseImagePDF(caseResultIds));
        }
        public ServiceResponseGeneric<Task<string>> GetCaseResultsJSON(List<Guid> ids)
        {
            return (_caseRepository.GetCaseResultsJSON(ids));
        }
        public ServiceResponseGeneric<Task<string>> GetCaseResultsZip(List<Guid> ids)
        {
            return (_caseRepository.GetCaseResultsZip(ids));
        }

        public ServiceResponseGeneric<List<CaseCalibrationModel>> GetCalibrationValue(Guid caseResultId)
        {
            return (_caseRepository.GetCalibrationValue(caseResultId));
        }

        public ServiceResponseGeneric<bool> UpdateCalibrationValue(CaseCalibrationModel caseCalibrationModel)
        {
            return (_caseRepository.UpdateCalibrationValue(caseCalibrationModel));
        }

        public ServiceResponseGeneric<bool> UpdateCaseName(UpdateCaseNameServiceModel updateCaseNameServiceModel)
        {
            return (_caseRepository.UpdateCaseName(updateCaseNameServiceModel));
        }
    }

}
