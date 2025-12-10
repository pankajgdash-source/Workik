using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;

namespace PlayBook3DTSL.Services.Interfaces
{
    public interface ICRUDInterface<T>
    {
        ServiceResponseGeneric<bool> Create(T serviceModel);
        ServiceResponseGeneric<bool> Update(T serviceModel);
        ServiceResponseGeneric<T> GetById(Guid id);
        ServiceResponseGeneric<List<T>> GetAll();
        ServiceResponseGeneric<bool> Delete(Guid id);
    }
}
