using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace MeTLLib
{
    public class ClientFactory
    {
        public static StandardKernel kernel = new StandardKernel(new BaseModule(), new ProductionModule());

        public static ClientConnection Connection(MeTLServerAddress.serverMode serverMode, MeTLGenericAddress searchAddress)
        {
            kernel.Get<MeTLServerAddress>().setMode(serverMode);
            Console.WriteLine("ClientFactory::Connection {0}", serverMode);
            kernel.Unbind<MeTLServerAddress>();
            switch (serverMode)
            {
                case MeTLServerAddress.serverMode.PRODUCTION:
                    kernel.Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
                    break;
                case MeTLServerAddress.serverMode.STAGING:
                    kernel.Bind<MeTLServerAddress>().To<StagingServerAddress>().InSingletonScope();
                    break;
                case MeTLServerAddress.serverMode.EXTERNAL:
                    kernel.Bind<MeTLServerAddress>().To<ExternalServerAddress>().InSingletonScope();
                    break;
            }

            kernel.Unbind<MeTLGenericAddress>();
            kernel.Bind<MeTLGenericAddress>().To(searchAddress.GetType()).InSingletonScope();

            return kernel.Get<ClientConnection>();
        }
        public static ClientConnection Connection()
        {
            return kernel.Get<ClientConnection>();
        }
    }
}
