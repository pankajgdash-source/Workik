using static PlayBook3DTSL.Model.PacsServer.PacsServerModel;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;


namespace PlayBook3DTSL.Repository.Interfaces.PacsServer
{
    public interface IPacsServerService
    {
        ServiceResponseGeneric<Task<List<PacsPatientDetail>>> GetPatientDetails(CFindRequestServiceModel cFindRequestServiceModel);

        ServiceResponseGeneric<Task<CFindPatientResponse>> GetPatientDetails(CFindRequestModel cFindRequestServiceModel);
    }
}
