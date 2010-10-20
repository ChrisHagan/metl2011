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
        public static MeTLServerAddress server;
        public static ClientConnection Connection(MeTLServerAddress serverAddress)
        {
            server = serverAddress;
            var typeofServer = server.GetType();
            kernel.Bind(typeof(MeTLServerAddress)).To(typeofServer).InSingletonScope();
            return kernel.Get<ClientConnection>();
        }
        public static ClientConnection Connection()
        {
            if (server == null)
            {
                server = new MadamServerAddress();
                var typeofServer = server.GetType();
                kernel.Bind(typeof(MeTLServerAddress)).To(typeofServer).InSingletonScope();
            }
            //kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            return kernel.Get<ClientConnection>();
        }
    }
}
