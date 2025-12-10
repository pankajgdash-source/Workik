using Azure;
using Dicom;
using Dicom.Network;
using Microsoft.Extensions.Hosting;
using PlayBook3DTSL.Model.Hospital;
using PlayBook3DTSL.Repository.Interfaces.PacsServer;
using PlayBook3DTSL.Utilities.Helpers;
using static PlayBook3DTSL.Model.PacsServer.PacsServerModel;

namespace PlayBook3DTSL.Repository.PacsServer.PacsServerFactory
{
    public class PacsServerImage : PacsConnector, IPacsServer
    {
        private static List<CFindPatientResponse> _cFindPatientResponse = new();
        private static List<CGetPatientResponse> _cGetPatientResponse = new();
        private static IHostEnvironment _environment;

        public PacsServerImage(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<T>> CFind<T>(CFindRequestServiceModel cFindRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            // TODO : refactor this code. Every time can't init
            _cFindPatientResponse = new List<CFindPatientResponse>();

            /////

            var client = await FindPatientStudyDetail(cFindRequestServiceModel, getPacsConfiguration);
            await client.SendAsync();
            return _cFindPatientResponse as List<T>;
        }

        public Task<List<T>> CStore<T>()
        {
            throw new NotImplementedException();
        }

        public async Task<List<T>> GetDetails<T>(CGetRequestServiceModel cGetRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            try
            {
                // TODO : refactor this code. Every time can't init
                _cGetPatientResponse = new List<CGetPatientResponse>();

                /////

                var client = await GetPatientStudyDetail(cGetRequestServiceModel, getPacsConfiguration);
                await client.SendAsync();
                return _cGetPatientResponse as List<T>;
            }
            catch (Exception ex)
            {
                if (ex.Message != "Association Abort [source: Unknown; reason: NotSpecified]")
                {
                    throw;
                }
                return _cGetPatientResponse as List<T>;
            }
        }

        public static async Task<Dicom.Network.Client.DicomClient> FindPatientStudyDetail(CFindRequestServiceModel cFindRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            var hospitalPACSDetail = getPacsConfiguration.Invoke(cFindRequestServiceModel.HospitalId);
            var cFindScuDicomClient = new Dicom.Network.Client.DicomClient(hospitalPACSDetail.Pacshost, hospitalPACSDetail.Pacsport.Value, false, hospitalPACSDetail.PacscallingAe, hospitalPACSDetail.PacscalledAe);
            cFindScuDicomClient.NegotiateAsyncOps();

            // Need to update based on the requirement
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);

            // To retrieve the attributes of data you are interested in
            // that must be returned in the result
            // you must specify them in advance with empty parameters like shown below
            var isPatientId = cFindRequestServiceModel.PatientName.Any(char.IsDigit);
            if (isPatientId)
            {
                request.Dataset.AddOrUpdate(DicomTag.PatientName, "");
                request.Dataset.AddOrUpdate(DicomTag.PatientID, cFindRequestServiceModel.IsExactMatch ? $"{cFindRequestServiceModel.PatientName}" : $"*{cFindRequestServiceModel.PatientName}*");
            }
            else
            {
                request.Dataset.AddOrUpdate(DicomTag.PatientName, $"*{cFindRequestServiceModel.PatientName}*");
                request.Dataset.AddOrUpdate(DicomTag.PatientID, "");
            }

            request.Dataset.AddOrUpdate(DicomTag.PatientBirthDate, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyDescription, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyDate, "");
            request.Dataset.AddOrUpdate(DicomTag.Modality, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfStudyRelatedSeries, "");
            request.Dataset.AddOrUpdate(DicomTag.AccessionNumber, "");
            request.Dataset.AddOrUpdate(DicomTag.PixelSpacing, "");


            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, string.IsNullOrWhiteSpace(cFindRequestServiceModel.StudyInstanceUID) ? "" : cFindRequestServiceModel.StudyInstanceUID);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, string.IsNullOrWhiteSpace(cFindRequestServiceModel.SeriesInstanceUID) ? "" : cFindRequestServiceModel.SeriesInstanceUID);
            request.Dataset.AddOrUpdate(DicomTag.SOPInstanceUID, string.IsNullOrWhiteSpace(cFindRequestServiceModel.SOPInstanceUID) ? "" : cFindRequestServiceModel.SOPInstanceUID);
            request.Dataset.AddOrUpdate(DicomTag.SeriesNumber, "");
            request.Dataset.AddOrUpdate(DicomTag.InstanceNumber, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfPatientRelatedStudies, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfFrames, "");
            request.Dataset.AddOrUpdate(DicomTag.AnatomicalOrientationType, "");
            //request.Dataset.AddOrUpdate(DicomTag.TrackSetAnatomicalTypeCodeSequence, "");

            request.Dataset.AddOrUpdate(DicomTag.StartAcquisitionDateTime, "");
            request.Dataset.AddOrUpdate(DicomTag.EndAcquisitionDateTime, "");



            request.Dataset.AddOrUpdate(DicomTag.StudyID, "");
            request.Dataset.AddOrUpdate(DicomTag.MediaStorageSOPClassUID, "");


            // Specify the patient name filter 
            //request.Dataset.AddOrUpdate(DicomTag.PatientName, patientName);

            // Specify the encoding of the retrieved results
            // here the character set is 'Latin alphabet No. 1'
            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");



            // Find a list of Studies

            request.OnResponseReceived += (req, response) =>
            {
                LogStudyResultsFoundToDebugConsole(response);
            };

            //add the request payload to the C FIND SCU Client
            await cFindScuDicomClient.AddRequestAsync(request);

            //Add a handler to be notified of any association rejections
            cFindScuDicomClient.AssociationRejected += OnAssociationRejected;

            //Add a handler to be notified of any association information on successful connections
            cFindScuDicomClient.AssociationAccepted += OnAssociationAccepted;

            //Add a handler to be notified when association is successfully released - this can be triggered by the remote peer as well
            cFindScuDicomClient.AssociationReleased += OnAssociationReleased;

            return cFindScuDicomClient;
        }

        public static void LogStudyResultsFoundToDebugConsole(DicomCFindResponse response)
        {
            if (response.Status == DicomStatus.Pending)
            {
                var patientName = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
                var patientID = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
                var studyDate = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, new DateTime());
                var studyInstanceUID = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
                var seriesInstanceUID = response.Dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
                var seriesNumber = response.Dataset.GetSingleValueOrDefault(DicomTag.SeriesNumber, string.Empty);
                var studyDescription = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDescription, string.Empty);
                var instanceNumber = response.Dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, string.Empty);
                var modality = response.Dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty);
                var numberOfSeriesRelatedInstances = response.Dataset.GetSingleValueOrDefault(DicomTag.NumberOfSeriesRelatedInstances, string.Empty);
                var numberOfStudyRelatedSeries = response.Dataset.GetSingleValueOrDefault(DicomTag.NumberOfStudyRelatedSeries, string.Empty);
                var numberOfPatientRelatedStudies = response.Dataset.GetSingleValueOrDefault(DicomTag.NumberOfPatientRelatedStudies, string.Empty);
                var numberOfFrames = response.Dataset.GetSingleValueOrDefault(DicomTag.NumberOfFrames, string.Empty);
                var accessionNumber = response.Dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, string.Empty);
                var pixelSpacingCount = response.Dataset.GetValueCount(DicomTag.PixelSpacing);
                List<string> pixelSpacingString = new List<string>();
                for (int i = 0; i < pixelSpacingCount; i++)
                {
                    var pixel = response.Dataset.GetValueOrDefault(DicomTag.PixelSpacing, i, string.Empty);
                    if (!string.IsNullOrWhiteSpace(pixel))
                    {
                        pixelSpacingString.Add(pixel);
                    }
                }
                string pixelSpacing = string.Empty;
                if (pixelSpacingString.Any())
                {
                    pixelSpacing = String.Join("\\", pixelSpacingString);
                }


                var anatomicalOrientationType = response.Dataset.GetSingleValueOrDefault(DicomTag.AnatomicalOrientationType, string.Empty);
                var trackSetAnatomicalTypeCodeSequence = response.Dataset.GetSingleValueOrDefault(DicomTag.TrackSetAnatomicalTypeCodeSequence, string.Empty);
                var startAcquisitionDateTime = response.Dataset.GetSingleValueOrDefault(DicomTag.StartAcquisitionDateTime, new DateTime());
                var endAcquisitionDateTime = response.Dataset.GetSingleValueOrDefault(DicomTag.EndAcquisitionDateTime, new DateTime());

                var patientBirthDate = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, new DateTime());

                var SOPInstanceUID = response.Dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty);

                _cFindPatientResponse.Add(new CFindPatientResponse
                {
                    PatientName = patientName,
                    PatientBirthDate = patientBirthDate,
                    InstanceNumber = instanceNumber,
                    Modality = modality,
                    NumberOfFrames = numberOfFrames,
                    NumberOfPatientRelatedStudies = numberOfPatientRelatedStudies,
                    NumberOfSeriesRelatedInstances = numberOfSeriesRelatedInstances,
                    NumberOfStudyRelatedSeries = numberOfStudyRelatedSeries,
                    PatientId = patientID,
                    SeriesInstanceUID = seriesInstanceUID,
                    SeriesNumber = seriesNumber,
                    SOPInstanceUID = SOPInstanceUID,
                    StudyDate = studyDate,
                    StudyDescription = studyDescription,
                    StudyInstanceUID = studyInstanceUID,
                    AccessionNumber = accessionNumber,
                    PixelSpacing = pixelSpacing,
                    AnatomicalOrientationType = anatomicalOrientationType,
                    TrackSetAnatomicalTypeCodeSequence = trackSetAnatomicalTypeCodeSequence,
                    StartAcquisitionDateTime = startAcquisitionDateTime,
                    EndAcquisitionDateTime = endAcquisitionDateTime
                });
            }

            if (response.Status == DicomStatus.Success)
            {
                LogToDebugConsole(response.Status.ToString());
            }
        }


        public static async Task<Dicom.Network.Client.DicomClient> GetPatientStudyDetail(CGetRequestServiceModel cGetRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            var hospitalPACSDetail = getPacsConfiguration.Invoke(cGetRequestServiceModel.HospitalId);
            var cGetScuDicomClient = new Dicom.Network.Client.DicomClient(hospitalPACSDetail.Pacshost, hospitalPACSDetail.Pacsport.Value, false, hospitalPACSDetail.PacscallingAe, hospitalPACSDetail.PacscalledAe);
            cGetScuDicomClient.NegotiateAsyncOps();

            var request = new DicomCGetRequest(cGetRequestServiceModel.StudyInstanceUID, cGetRequestServiceModel.SeriesInstanceUID, cGetRequestServiceModel.SOPInstanceUID, DicomPriority.High);
            // To retrieve the attributes of data you are interested in
            // that must be returned in the result
            // you must specify them in advance with empty parameters like shown below

            request.Dataset.AddOrUpdate(DicomTag.PatientID, cGetRequestServiceModel.PatientId);
            request.Dataset.AddOrUpdate(DicomTag.PatientName, cGetRequestServiceModel.PatientName);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, cGetRequestServiceModel.StudyInstanceUID);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, cGetRequestServiceModel.SeriesInstanceUID);
            request.Dataset.AddOrUpdate(DicomTag.SOPInstanceUID, cGetRequestServiceModel.SOPInstanceUID);

            //request.Dataset.AddOrUpdate(DicomTag.StudyID, "");

            // Specify the encoding of the retrieved results
            // here the character set is 'Latin alphabet No. 1'
            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

            cGetScuDicomClient.OnCStoreRequest += async (DicomCStoreRequest req) =>
            {
                SaveImage(req.Dataset);
                return new DicomCStoreResponse(req, DicomStatus.Success);
            };


            request.OnResponseReceived += (req, response) =>
            {
            };
            var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                                               DicomStorageCategory.Image,
                                               DicomTransferSyntax.ExplicitVRLittleEndian,
                                               DicomTransferSyntax.ImplicitVRLittleEndian,
                                               DicomTransferSyntax.ImplicitVRBigEndian);
            cGetScuDicomClient.AdditionalPresentationContexts.AddRange(pcs);

            //add the request payload to the C Move SCU Client
            await cGetScuDicomClient.AddRequestAsync(request);

            //// Set up the client's association parameters

            //await cGetScuDicomClient.AddRequestAsync(new DicomCEchoRequest());

            ////Add a handler to be notified of any association rejections
            ////Add a handler to be notified of any association rejections
            //cGetScuDicomClient.AssociationRejected += OnAssociationRejected;

            ////Add a handler to be notified of any association information on successful connections
            //cGetScuDicomClient.AssociationAccepted += OnAssociationAccepted;

            ////Add a handler to be notified when association is successfully released - this can be triggered by the remote peer as well
            //cGetScuDicomClient.AssociationReleased += OnAssociationReleased;

            return cGetScuDicomClient;
        }

        public static void SaveImage(DicomDataset dataset)
        {
            var studyUid = dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID).Trim();
            var instUid = dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID).Trim();
            var pixelSpacingCount = 0;
            try
            {
                pixelSpacingCount = dataset.GetValueCount(DicomTag.PixelSpacing);
            }
            catch (Exception)
            {}

            if (pixelSpacingCount == 0)
            {
                dataset.AddOrUpdate(DicomTag.PixelSpacing, new float[] { 0.30f, 0.25f }); // Used for Calibration Tool to decide the measurement unit
            }
            var path = $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{"DicomFiles"}"; //Path.GetFullPath($"{Directory.GetCurrentDirectory()}_{DateTime.Now.Ticks}_Sample");
            path = System.IO.Path.Combine(path, studyUid);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = System.IO.Path.Combine(path, instUid) + ".dcm";
            _cGetPatientResponse.Add(new CGetPatientResponse
            {
                SaveImagePath = path,
                SOPInstanceUID = instUid
            });
            new DicomFile(dataset).Save(path);
        }
    }
}
