namespace PlayBook3DTSL.API.Middleware
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ApiVersionAttribute : Attribute
    {
        public string Version { get; }

        public ApiVersionAttribute(string version)
        {
            Version = version;
        }
    }

}
