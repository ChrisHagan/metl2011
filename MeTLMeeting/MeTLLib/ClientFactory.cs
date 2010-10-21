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
        public static MeTLServerAddress.serverMode mode;  
        public static ClientConnection Connection(MeTLServerAddress.serverMode serverMode)
        {
            if (mode == MeTLServerAddress.serverMode.NOTSET || mode == serverMode) mode = serverMode;
            else throw new InvalidOperationException("serverMode has already been set");
            kernel.Get<MeTLServerAddress>().setMode(mode);
            return kernel.Get<ClientConnection>();
        }
        public static ClientConnection Connection()
        {
            if (mode == MeTLServerAddress.serverMode.NOTSET) throw new NotSetException("serverMode has not been configured");
            kernel.Get<MeTLServerAddress>().setMode(mode);
            return kernel.Get<ClientConnection>();
        }
        public static void Reset()
        {
            kernel = new StandardKernel(new BaseModule(), new ProductionModule());
            mode = MeTLServerAddress.serverMode.NOTSET;
        }
    }
}
