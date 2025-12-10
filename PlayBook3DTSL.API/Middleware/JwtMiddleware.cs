using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlayBook3DTSL.Utilities.Helpers;
using PlayBook3DTSL.Utilities.ServiceModel;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PlayBook3DTSL.API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly TokenManager _tokenManager;
        private readonly ILogger _logger;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings, TokenManager tokenManager)
        {
            _next = next;
            _appSettings = appSettings.Value;
            _tokenManager = tokenManager;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token != null)
                {
                    var (isTokenValid, jwtToken) = ValidateToken(token);
                    if (isTokenValid)
                    {
                        context.Response.Headers["Authorization"] = "Bearer " + _tokenManager.RefreshToken(context);
                        attachUserToContext(context, jwtToken);
                    }
                }
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
            
        }
        private (bool, JwtSecurityToken?) ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Key));
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = key,
                    ValidIssuer = _appSettings.JwtIssuer,
                    ValidAudience = _appSettings.JwtAudience,
                    // set clockskew to zero so tokens expire exactly at token expiration time.
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                return (true, jwtToken);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }
        private void attachUserToContext(HttpContext context, JwtSecurityToken jwtToken)
        {
            context.Items["UserName"] = jwtToken?.Claims?.FirstOrDefault(a => a.Type == "UserName")?.Value;
            context.Items["Id"] = jwtToken?.Claims?.FirstOrDefault(a => a.Type == "Id")?.Value;
            context.Items["Role"] = jwtToken?.Claims?.FirstOrDefault(a => a.Type == "Role")?.Value;
            context.Items["HospitalId"] = jwtToken?.Claims?.FirstOrDefault(a => a.Type == "HospitalId")?.Value;
            context.Items["IsMirrorMode"] = jwtToken?.Claims?.FirstOrDefault(a => a.Type == "IsMirrorMode")?.Value;
            context.Items["SurgeonUserId"] = jwtToken?.Claims?.FirstOrDefault(a => a.Type == "SurgeonUserId")?.Value;
        }
    }
}