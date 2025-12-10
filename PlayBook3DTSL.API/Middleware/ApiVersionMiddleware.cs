using Asp.Versioning;

namespace PlayBook3DTSL.API.Middleware
{
    public class ApiVersionMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiVersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                // Get all ApiVersionAttributes from class and method
                var classVersionAttributes = endpoint.Metadata.OfType<ApiVersionAttribute>().ToList();
                var methodVersionAttributes = endpoint.Metadata.GetMetadata<ApiVersionAttribute>();

                string? requiredVersion = "v1"; // Default to v1

                if (methodVersionAttributes != null)
                {
                    requiredVersion = methodVersionAttributes?.Version;
                }
                else if (classVersionAttributes.Any())
                {
                    // If method does not have ApiVersionAttribute, use the class-level version
                    requiredVersion = classVersionAttributes.First().Version;
                }

                var pathSegments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
                string requestedVersion = "v1"; // Default to v1 if version is not present in the URL

                if (pathSegments != null && pathSegments.Length > 1)
                {
                    requestedVersion = pathSegments[1]; // Extract version if present
                }

                if (!string.Equals(requestedVersion, requiredVersion, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    var errorResponse = new[]
                    {
                    new
                    {
                        statusCode = 400,
                        errorMessage = "API Version Not Supported: The provided version is incompatible with this endpoint"
                    }
                };

                    await context.Response.WriteAsJsonAsync(errorResponse);
                    return;
                }
            }

            await _next(context);
        }
    }


}
