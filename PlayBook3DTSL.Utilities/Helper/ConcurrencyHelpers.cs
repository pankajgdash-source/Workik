
using PlayBook3DTSL.Database.Entities;
using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Utilities.Helpers
{
    [ExcludeFromCodeCoverage]
    public class ConcurrencyHelpers
    {
        public void SetDefaultValueInsert(BaseEntity entity)
        {
            entity.CreatedBy =  GetLoggedInUserId();
            entity.CreatedOn = DateTime.UtcNow;
            entity.LastUpdatedBy =  GetLoggedInUserId();
            entity.LastUpdatedOn = DateTime.UtcNow;
        }
        public long GetLoggedInUserId()
        {
            return ApplicationHelpers.LoggedInUserId > 0 ? ApplicationHelpers.LoggedInUserId : -1;
        }
        public void SetDefaultValueUpdate(BaseEntity entity)
        {
            entity.LastUpdatedBy = -1;
            entity.LastUpdatedOn = DateTime.UtcNow;
        }
    }
}
