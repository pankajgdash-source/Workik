
using PlayBook3DTSL.Repository.Interfaces.PacsServer;
using static PlayBook3DTSL.Model.PacsServer.PacsServerModel;

namespace PlayBook3DTSL.Repository.PacsServer.PacsServerFactory
{
    public class PacsServerFactoryService
    {
        private readonly IServiceProvider _serviceProvider;

        public PacsServerFactoryService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPacsServer GetService(PACSRequest pacsRequest)
        {
            return (IPacsServer)_serviceProvider.GetService(typeof(PacsServerCommonServices));
           
        }
    }
}
