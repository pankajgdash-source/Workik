
using PlayBook3DTSL.Model.Hospital;
using static PlayBook3DTSL.Model.PacsServer.PacsServerModel;

namespace PlayBook3DTSL.Repository.Interfaces.PacsServer
{
    public interface IPacsServer
    {
        Task<List<T>> CFind<T>(CFindRequestServiceModel cFindRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration);
        Task<List<T>> CStore<T>();
        Task<List<T>> GetDetails<T>(CGetRequestServiceModel cGetRequestServiceModel, Func<Guid, HospitalModel> getPacsConfiguration);
    }
}
