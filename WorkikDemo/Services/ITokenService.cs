using WorkikDemo.Models;

namespace WorkikDemo.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}