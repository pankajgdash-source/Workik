using Microsoft.AspNetCore.Mvc;
using WorkikDemo.Models;
using WorkikDemo.Services;

namespace WorkikDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        // For demo, using a hardcoded user list; replace with database in real app
        private readonly List<User> _users = new()
        {
            new User { Username = "testuser", Password = "password", Role = "User" },
            new User { Username = "adminuser", Password = "adminpass", Role = "Admin" }
        };

        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public ActionResult<UserResponseDto> Login(UserLoginDto loginDto)
        {
            var user = _users.SingleOrDefault(u => 
                u.Username == loginDto.Username && u.Password == loginDto.Password);

            if (user == null) return Unauthorized("Invalid username or password");

            var token = _tokenService.CreateToken(user);

            return Ok(new UserResponseDto
            {
                Username = user.Username,
                Token = token
            });
        }
    }
}