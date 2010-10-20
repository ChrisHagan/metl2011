using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using MeTLLib.Providers.Connection;
using System.Net;

namespace MeTLLib
{
    public class ProductionServerAddress : MeTLServerAddress
    {
        public ProductionServerAddress()
        {
            //This is pre-dependency injection.  That means I can't inject a more specific webclient, and I hope that this cheap standard webclient doesn't have side-effects that will hurt us later.
            var prodServer = System.Xml.Linq.XElement.Parse(new System.Net.WebClient().DownloadString("http://metl.adm.monash.edu.au/server.xml")).Value;
            productionUri = new Uri("http://" + prodServer, UriKind.Absolute);
            var stagingServer = System.Xml.Linq.XElement.Parse(new System.Net.WebClient().DownloadString("http://metl.adm.monash.edu.au/stagingServer.xml")).Value;
            stagingUri = new Uri("http://" + stagingServer, UriKind.Absolute);
        }
    }

    class ProductionModule : NinjectModule
    {
        public override void Load()
        {
            Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
            Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            Bind<ClientConnection>().ToSelf().InSingletonScope();
            Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            Bind<WebClientWithTimeout>().ToSelf();
            Bind<JabberWireFactory>().ToSelf().InSingletonScope();
            Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            Bind<ITimerFactory>().To<ProductionTimerFactory>().InSingletonScope();
            Bind<IReceiveEvents>().To<ProductionReceiveEvents>().InSingletonScope();
        }
    }
}
