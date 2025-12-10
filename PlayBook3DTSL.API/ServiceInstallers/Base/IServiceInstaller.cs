namespace PlayBook3DTSL.API.ServiceInstallers.Base
{
    public interface IServiceInstaller
    {
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }
}
