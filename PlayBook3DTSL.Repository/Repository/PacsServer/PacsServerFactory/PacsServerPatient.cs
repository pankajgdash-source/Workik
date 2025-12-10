using Dicom;
using Dicom.Network;
using PlayBook3DTSL.Model.Hospital;
using PlayBook3DTSL.Repository.Interfaces.PacsServer;
using static PlayBook3DTSL.Model.PacsServer.PacsServerModel;

namespace PlayBook3DTSL.Repository.PacsServer.PacsServerFactory
{
    public class PacsServerPatient : PacsConnector, IPacsServer
    {
        private static List<CFindPatientResponse> _cFindPatientResponse = new();

        public async Task<List<T>> CFind<T>(CFindRequestServiceModel cFindRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            // TODO : refactor this code. Every time can't init
            _cFindPatientResponse = new List<CFindPatientResponse>();

            /////

            var client = await FindPatientDetail(cFindRequestServiceModel, getPacsConfiguration);
            await client.SendAsync();
            return _cFindPatientResponse as List<T>;
        }

        public Task<List<T>> CStore<T>()
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetDetails<T>(CGetRequestServiceModel cGetRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            throw new NotImplementedException();
        }

        public static async Task<Dicom.Network.Client.DicomClient> FindPatientDetail(CFindRequestServiceModel cFindRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration)
        {
            var hospitalPACSDetail = getPacsConfiguration.Invoke(cFindRequestServiceModel.HospitalId);
            var cFindScuDicomClient = new Dicom.Network.Client.DicomClient(hospitalPACSDetail.Pacshost, hospitalPACSDetail.Pacsport.Value, false, hospitalPACSDetail.PacscallingAe, hospitalPACSDetail.PacscalledAe);
            cFindScuDicomClient.NegotiateAsyncOps();

            // Need to update based on the requirement
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Patient);

            // To retrieve the attributes of data you are interested in
            // that must be returned in the result
            // you must specify them in advance with empty parameters like shown below

            var isPatientId = cFindRequestServiceModel.PatientId.Any(char.IsDigit);
            if (isPatientId)
            {
                request.Dataset.AddOrUpdate(DicomTag.PatientName, "");
                request.Dataset.AddOrUpdate(DicomTag.PatientID, $"{cFindRequestServiceModel.PatientId}");
            }
            else
            {
                request.Dataset.AddOrUpdate(DicomTag.PatientName, $"*{cFindRequestServiceModel.PatientName}*");
                request.Dataset.AddOrUpdate(DicomTag.PatientID, "");
            }

            //request.Dataset.AddOrUpdate(DicomTag.PatientName, string.IsNullOrWhiteSpace(cFindRequestServiceModel.PatientName) ? "" : $"*{cFindRequestServiceModel.PatientName}*");
            //request.Dataset.AddOrUpdate(DicomTag.PatientID, string.IsNullOrWhiteSpace(cFindRequestServiceModel.PatientId) ? "" : $"*{cFindRequestServiceModel.PatientId}*");
            request.Dataset.AddOrUpdate(DicomTag.PatientBirthDate, "");
            request.Dataset.AddOrUpdate(DicomTag.PatientSex, "");
            request.Dataset.AddOrUpdate(DicomTag.PatientAge, "");


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
                var patientBirthDate = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, new DateTime());
                var patientSex = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty);
                var patientAge = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientAge, string.Empty);


                _cFindPatientResponse.Add(new CFindPatientResponse
                {
                    PatientName = patientName,
                    PatientBirthDate = patientBirthDate,
                    PatientId = patientID,
                    PatientSex = patientSex,
                    PatientAge = patientAge
                });
            }

            if (response.Status == DicomStatus.Success)
            {
                LogToDebugConsole(response.Status.ToString());
            }
        }
    }
}
