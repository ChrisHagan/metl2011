using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using System.Net;

namespace MeTLLib
{
    public class ClientFactory
    {
        protected static Dictionary<MetlConfiguration,ClientConnection> configCache = new Dictionary<MetlConfiguration,ClientConnection>();
        public static StandardKernel kernel = new StandardKernel(new BaseModule(), new ProductionModule());

        public static ClientConnection Connection(MetlConfiguration config)
        {
            var cc = new ClientConnection(config, new ProductionReceiveEvents());
            configCache.Remove(config);
            configCache.Add(config, cc);
            kernel.Unbind<IReceiveEvents>();
            kernel.Bind<IReceiveEvents>().ToConstant(cc.events);//<ProductionReceiveEvents>().InSingletonScope();
            kernel.Unbind<MetlConfiguration>();
            kernel.Bind<MetlConfiguration>().ToConstant(config);//<MetlConfiguration>().InSingletonScope();
            kernel.Unbind<ICredentials>();
            kernel.Bind<ICredentials>().ToConstant(new NetworkCredential(config.resourceUsername, config.resourcePassword));//<MeTLCredentials>().InSingletonScope();
            kernel.Unbind<ClientConnection>();
            kernel.Bind<ClientConnection>().ToConstant(cc);
            
            /*
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
            */
            //return cc;
            return kernel.Get<ClientConnection>();
        }
        public static ClientConnection Connection()
        {
            return kernel.Get<ClientConnection>();
        }
    }
}
