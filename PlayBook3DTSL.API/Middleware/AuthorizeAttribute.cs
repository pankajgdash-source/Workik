using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlayBook3DTSL.Utilities.Helpers;

namespace PlayBook3DTSL.API.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Items["UserName"];
            var Id = context.HttpContext.Items["Id"];
            var Role = context?.HttpContext?.Items["Role"]?.ToString();
            ApplicationHelpers.IsMirrorModeOn = Convert.ToBoolean(context?.HttpContext.Items["IsMirrorMode"]);
            ApplicationHelpers.LoggedInUserId = ApplicationHelpers.IsMirrorModeOn ? Convert.ToInt64(context.HttpContext.Items["SurgeonUserId"])
                                                                    : Convert.ToInt64(context.HttpContext.Items["Id"]);
            ApplicationHelpers.LoggedinUserRole = Role;
            ApplicationHelpers.UserName = Convert.ToString(user);
            if (context.HttpContext.Items["HospitalId"] != null)
            {
                ApplicationHelpers.LoggedInHospitalId = Guid.Parse(context.HttpContext.Items["HospitalId"].ToString());
            }
            if (user == null || Id == null || Role == null)
            {
                // not logged in
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
