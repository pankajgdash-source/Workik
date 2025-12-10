
using System.Reflection;

namespace PlayBook3DTSL.API.ServiceInstallers
{
    public static class InstallerExtensions
    {
        public static void LernenderServices(this IServiceCollection services, IConfiguration configuration)
        {
            //
            // Register Mapper Profiles
            //

            #region Register Mapper Profiles
            //
            // Register AutoMapper Profiles
            //
            services.AddAutoMapper((Assembly[]?)AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("PlayBook3DTSL.Model", StringComparison.CurrentCulture)).ToArray());
            #endregion
           
        }
    }
}
