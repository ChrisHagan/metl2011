using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using MeTLLib.Providers.Connection;
using System.Net;
using MeTLLib.Providers;
using System.Xml.Linq;

namespace MeTLLib
{
    public class TriedToStartMeTLWithNoInternetException : Exception {
        public TriedToStartMeTLWithNoInternetException(Exception inner) : base("Couldn't connect to MeTL servers", inner) {}
    }

    public class ProductionServerAddress : MeTLServerAddress
    {
        private Uri BootstrapUrl(string bootstrapUrl)
        {
            try
            {
                //This is pre-dependency injection.  That means I can't inject a more specific webclient, and I hope that this cheap standard webclient doesn't have side-effects that will hurt us later.
                var serverString = XElement.Parse(new WebClient().DownloadString(bootstrapUrl)).Value;
                return new Uri("http://" + serverString, UriKind.Absolute);
            }
            catch (WebException e) {
                throw new TriedToStartMeTLWithNoInternetException(e);
            }
        }
        
        private Uri LoadServerAddress(bool useBootstrapUrl, string url)
        {
            if (useBootstrapUrl)
            {
                return BootstrapUrl(url);
            }
            else
            {
                return new Uri(url, UriKind.Absolute);               
            }
        }

        public ProductionServerAddress()
        {
            var conf = MeTLConfiguration.Config;

            productionUri = LoadServerAddress(conf.Production.IsBootstrapUrl, conf.Production.Host);
            stagingUri = LoadServerAddress(conf.Staging.IsBootstrapUrl, conf.Staging.Host);
            externalUri = LoadServerAddress(conf.External.IsBootstrapUrl, conf.External.Host);
        }
    }

    public class ProductionModule : NinjectModule
    {
        public override void Load()
        {
            Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
            Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            Bind<IUserInformationProvider>().To<ProductionUserInformationProvider>().InSingletonScope();
            Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            Bind<ClientConnection>().ToSelf().InSingletonScope();
            Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            Bind<WebClientWithTimeout>().ToSelf();
            Bind<JabberWireFactory>().ToSelf().InSingletonScope();
            Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            Bind<ITimerFactory>().To<ProductionTimerFactory>().InSingletonScope();
            Bind<IReceiveEvents>().To<ProductionReceiveEvents>().InSingletonScope();
            Bind<UserOptionsProvider>().To<UserOptionsProvider>().InSingletonScope();
        }
    }
}
