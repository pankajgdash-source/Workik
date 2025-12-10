
using PlayBook3DTSL.Utilities.ServiceModel;

namespace PlayBook3DTSL.Service.User
{
    public class AuthorizeResult
    {
        public long Id { get; set; }
        public string Role { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public long RoleId { get; set; }
        public string UserName { get; set; } = null!;
        public string? ProfileImage { get; set; }
        public List<IdNameModel> UserRoles { get; set; } = null!;
        public bool IsPasswordExpired { get;set; }
    }
    
}
