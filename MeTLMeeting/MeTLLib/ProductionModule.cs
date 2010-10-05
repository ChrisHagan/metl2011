using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using MeTLLib.Providers.Connection;
using System.Net;

namespace MeTLLib
{
    class ProductionModule : NinjectModule
    {
        public override void Load()
        {
            Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            Bind<ClientConnection>().ToSelf().InSingletonScope();
            Bind<WebClientWithTimeout>().ToSelf();
            Bind<JabberWireFactory>().ToSelf().InSingletonScope();
            Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            Bind<ITimerFactory>().To<ProductionTimerFactory>().InSingletonScope();
        }
    }
}
