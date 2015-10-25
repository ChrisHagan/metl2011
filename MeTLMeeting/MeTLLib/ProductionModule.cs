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
    public class TriedToStartMeTLWithNoInternetException : Exception
    {
        public TriedToStartMeTLWithNoInternetException(Exception inner) : base("Couldn't connect to MeTL servers", inner) { }
    }
    /*
    public class ProductionServerAddress : MeTLServerAddress
    {
        public ProductionServerAddress()
        {
            var conf = MeTLConfiguration.Config;

            uploadEndpoint = conf.Production.UploadEndpoint;
            webAuthenticationEndpoint = new Uri(conf.Production.WebAuthenticationEndpoint);
            thumbnail = conf.Production.Thumbnail;
            port = conf.Production.Port;
            Name = conf.Production.Name;
            protocol = conf.Production.Protocol;
            productionUri = LoadServerAddress(conf.Production.IsBootstrapUrl, conf.Production.Host);
            stagingUri = LoadServerAddress(conf.Staging.IsBootstrapUrl, conf.Staging.Host);
            externalUri = LoadServerAddress(conf.External.IsBootstrapUrl, conf.External.Host);
            xmppServiceName = conf.Production.xmppServiceName;

            Console.WriteLine("Setting Production server address: {0}, {1}", productionUri, xmppServiceName);
        }
    }

    public class StagingServerAddress : MeTLServerAddress
    {
        public StagingServerAddress()
        {
            var conf = MeTLConfiguration.Config;

            uploadEndpoint = conf.Staging.UploadEndpoint;
            webAuthenticationEndpoint = new Uri(conf.Staging.WebAuthenticationEndpoint);
            thumbnail = conf.Staging.Thumbnail;
            port = conf.Staging.Port;
            Name = conf.Staging.Name;
            protocol = conf.Staging.Protocol;
            productionUri = LoadServerAddress(conf.Staging.IsBootstrapUrl, conf.Staging.Host);
            stagingUri = LoadServerAddress(conf.Staging.IsBootstrapUrl, conf.Staging.Host);
            externalUri = LoadServerAddress(conf.Staging.IsBootstrapUrl, conf.Staging.Host);
            xmppServiceName = conf.Staging.xmppServiceName;

            Console.WriteLine("Setting Staging server address: {0}, {1}", stagingUri, xmppServiceName);
        }
    }

    public class ExternalServerAddress : MeTLServerAddress
    {
        public ExternalServerAddress()
        {
            var conf = MeTLConfiguration.Config;

            uploadEndpoint = conf.External.UploadEndpoint;
            webAuthenticationEndpoint = new Uri(conf.External.WebAuthenticationEndpoint);
            thumbnail = conf.External.Thumbnail;
            port = conf.External.Port;
            protocol = conf.External.Protocol;
            Name = conf.External.Name;
            productionUri = LoadServerAddress(conf.External.IsBootstrapUrl, conf.External.Host);
            stagingUri = LoadServerAddress(conf.External.IsBootstrapUrl, conf.External.Host);
            externalUri = LoadServerAddress(conf.External.IsBootstrapUrl, conf.External.Host);
            xmppServiceName = conf.External.xmppServiceName;

            Console.WriteLine("Setting External server address: {0}, {1}", externalUri, xmppServiceName);
        }
    }
    */
    public class ProductionModule : NinjectModule
    {
        public override void Load()
        {
            //      Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
            //Bind<MetlConfiguration>().To<MetlConfiguration>().InSingletonScope();
            Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
      //      Bind<IUserInformationProvider>().To<ProductionUserInformationProvider>().InSingletonScope();
            //Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            Bind<ClientConnection>().ToSelf().InSingletonScope();
            Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            Bind<WebClientWithTimeout>().ToSelf();
            Bind<JabberWireFactory>().ToSelf().InSingletonScope();
            Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            Bind<ITimerFactory>().To<ProductionTimerFactory>().InSingletonScope();
            Bind<UserOptionsProvider>().To<UserOptionsProvider>().InSingletonScope();
        }
    }
    /*
    public class StagingModule : ProductionModule
    {
        public override void Load()
        {
            Unbind<MeTLServerAddress>();
            Bind<MeTLServerAddress>().To<StagingServerAddress>().InSingletonScope();
        }
    }
    */
}
