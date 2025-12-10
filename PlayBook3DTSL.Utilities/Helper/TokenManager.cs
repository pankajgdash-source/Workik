using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlayBook3DTSL.Utilities.ServiceModel;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PlayBook3DTSL.Utilities.Helpers
{
    [ExcludeFromCodeCoverage]
    public class TokenManager
    {
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;

        
        public TokenManager()
        {
            // This constructor is used for N-Unit test case to assign Appsetting value.
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                       .AddJsonFile("appsettings.json")
                                                       .Build();
         // _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            var configSection = _configuration.GetSection("AppSettings");

        }
        public TokenManager(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public string BuildToken(GenerateTokenServiceModel generateTokenServiceModel)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // Here you  can fill claim information from database for the users as well
            var claims = new[] {
                new Claim("UserName", generateTokenServiceModel.UserName),
                new Claim("Id",Convert.ToString(generateTokenServiceModel.Id)),
                new Claim("Role",generateTokenServiceModel.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("SurgeonUserId", generateTokenServiceModel.SurgeonUserId > 0? Convert.ToString(generateTokenServiceModel.SurgeonUserId.Value) : "0"),
                new Claim("IsMirrorMode",  Convert.ToBoolean(generateTokenServiceModel.IsMirrorMode) ? Convert.ToString(generateTokenServiceModel.IsMirrorMode) : "false")
                };

            if (generateTokenServiceModel.HospitalId.HasValue)
            {
                claims = claims.Append(new Claim("HospitalId", Convert.ToString(generateTokenServiceModel.HospitalId))).ToArray();
            }
            var token = new JwtSecurityToken(
                _appSettings.JwtIssuer,
                _appSettings.JwtAudience,
                claims,
                expires: DateTime.Now.AddMinutes(_appSettings.TokenExpiryTimeInMinutes),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string RefreshToken(HttpContext context)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(context.Request.Headers["Authorization"].First()?.Split(" ").Last());
            var generateTokenServiceModel = new GenerateTokenServiceModel()
            {
                HospitalId = securityToken.Claims.Any(c => c.Type == "HospitalId") ? Guid.Parse(securityToken.Claims.First(c => c.Type == "HospitalId").Value) : Guid.Parse(ApplicationHelpers.NewID),
                Role = securityToken.Claims.First(c => c.Type == "Role").Value,
                UserName = securityToken.Claims.First(c => c.Type == "UserName").Value,
                Id = Convert.ToInt64(securityToken.Claims.First(c => c.Type == "Id").Value),
                SurgeonUserId = Convert.ToInt64(securityToken.Claims.FirstOrDefault(c => c.Type == "SurgeonUserId").Value),
                IsMirrorMode = Convert.ToBoolean(securityToken.Claims.FirstOrDefault(c => c.Type == "IsMirrorMode").Value),
            };
            return BuildToken(generateTokenServiceModel);
        }
    }
}