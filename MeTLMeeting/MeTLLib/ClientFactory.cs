using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Ninject;
using System.Net;
using MeTLLib.Providers;
using MeTLLib.Providers.Connection;
using MeTLLib.Providers.Structure;
using MeTLLib.DataTypes;
namespace MeTLLib
{
    public class ClientFactory
    {
        public static ClientConnection Connection(MetlConfiguration config,Credentials creds)
        {

            var webCreds = new NetworkCredential(config.resourceUsername, config.resourcePassword);
            //var jabberCreds = new Credentials(config.xmppUsername, config.xmppPassword,new List<AuthorizedGroup>(),"");
            var wcf = new WebClientFactory(webCreds);
            var receiveEvents = new ProductionReceiveEvents();
            var authProvider = new AuthorisationProvider(wcf, config);
            var httpProvider = new HttpResourceProvider(wcf);
            var resourceUploaderFactory = new ProductionResourceUploaderFactory(config,httpProvider);
            var resourceUploader = resourceUploaderFactory.get();
            var resourceCache = new ResourceCache();
            var configurationProvider = new ConfigurationProvider(wcf);
            var conversationDetailsProvider = new FileConversationDetailsProvider(config, wcf, resourceUploader,creds);
            var jabberWireFactory = new JabberWireFactory(config,creds, configurationProvider, conversationDetailsProvider, resourceCache, receiveEvents, wcf, httpProvider);
            var userOptionsProvider = new UserOptionsProvider(config,httpProvider,resourceUploader);
            var cc = new ClientConnection(config, receiveEvents, authProvider, resourceUploader,conversationDetailsProvider,resourceCache,jabberWireFactory,wcf,userOptionsProvider, httpProvider);
            return cc;
        }
    }
}
