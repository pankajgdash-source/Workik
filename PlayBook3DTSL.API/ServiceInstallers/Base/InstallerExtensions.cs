using FluentValidation.AspNetCore;
using PlayBook3DTSL.API.ServiceInstallers.Base;

namespace PlayBook3DTSL.API.ServiceInstallers.Base
{
    public static class InstallerExtensions
    {
        public static void InstallServices(this IServiceCollection services, IConfiguration configuration)
        {
            // get list of all non absstract classes which implements IServiceInstaller interface
            List<IServiceInstaller> installers = typeof(Program).Assembly.ExportedTypes
                .Where(x => typeof(IServiceInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IServiceInstaller>()
                .ToList();
            // loop throught all installers and call install service method from them which in tern install services to IServiceCollection
            installers.ForEach(installer => installer.InstallServices(services, configuration));
            services.AddFluentValidationAutoValidation(config =>
            {
                config.DisableDataAnnotationsValidation = true;
            });
        }
    }
}
