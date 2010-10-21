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
        public static ClientConnection Connection(MeTLServerAddress serverAddress)
        {
            if (serverAddress.productionUri == null || serverAddress.stagingUri == null) throw new ArgumentNullException("uri", "Neither productionUri nor stagingUri may be null.");
            kernel.Unbind<MeTLServerAddress>();
            kernel.Bind<MeTLServerAddress>().To(serverAddress.GetType()).InSingletonScope();
            return kernel.Get<ClientConnection>();
        }
        public static ClientConnection Connection(MeTLServerAddress.serverMode serverMode)
        {
            kernel.Get<MeTLServerAddress>().setMode(serverMode);
            return kernel.Get<ClientConnection>();
        }
        public static ClientConnection Connection()
        {
            return kernel.Get<ClientConnection>();
        }
    }
}
