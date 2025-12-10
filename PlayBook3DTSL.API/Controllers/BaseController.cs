using Microsoft.AspNetCore.Mvc;
using System.Net;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;

namespace PlayBook3DTSL.API.Controllers
{
    [Route("api/{version}/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public IActionResult GenerateResponse<T>(ServiceResponseGeneric<T> serviceResponse)
        {
            if (serviceResponse.Success)
            {
                return Ok(serviceResponse);
            }
            return HandleHttpStatusCodes(serviceResponse, HttpStatusCode.BadRequest);
        }
      
        protected IActionResult GenerateResponse(ExecutionResult result, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (result.Success)
            {
                return Ok(result);
            }
            return HandleHttpStatusCodes(result, statusCode);
        }

        /// <summary>
        /// This method is implemented to get DRY from two method FromExecutionResult
        /// </summary>
        private IActionResult HandleHttpStatusCodes(ExecutionResult result, HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => Unauthorized(result),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest(result.Errors),
            };
        }

        private IActionResult HandleHttpStatusCodes<T>(ServiceResponseGeneric<T> result, HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => Unauthorized(result),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest(result.Errors),
            };
        }
    }
}
