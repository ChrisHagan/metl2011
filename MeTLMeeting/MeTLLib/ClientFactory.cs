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
        public static ClientConnection Connection(MetlConfiguration config,Credentials creds,IAuditor auditor)
        {
            return auditor.wrapFunction(((auditAction) =>
            {
                var webCreds = new NetworkCredential(creds.name, creds.password);
                auditAction(GaugeStatus.InProgress,7);
                //var jabberCreds = new Credentials(config.xmppUsername, config.xmppPassword,new List<AuthorizedGroup>(),"");
                var wcf = new WebClientFactory(webCreds,auditor,creds);
                auditAction(GaugeStatus.InProgress,14);
                var receiveEvents = new ProductionReceiveEvents();
                auditAction(GaugeStatus.InProgress,21);
                var authProvider = new AuthorisationProvider(wcf, config,auditor);
                auditAction(GaugeStatus.InProgress,28);
                var httpProvider = new HttpResourceProvider(wcf,auditor);
                auditAction(GaugeStatus.InProgress,35);
                var resourceUploaderFactory = new ProductionResourceUploaderFactory(config, httpProvider);
                auditAction(GaugeStatus.InProgress,42);
                var resourceUploader = resourceUploaderFactory.get();
                auditAction(GaugeStatus.InProgress,49);
                var resourceCache = new ResourceCache();
                auditAction(GaugeStatus.InProgress,56);
                var configurationProvider = new ConfigurationProvider(wcf,auditor);
                auditAction(GaugeStatus.InProgress,63);
                auditAction(GaugeStatus.InProgress,70);
                var jabberWireFactory = new JabberWireFactory(config, creds, configurationProvider,resourceUploader,resourceCache, receiveEvents, wcf, httpProvider,auditor);
                auditAction(GaugeStatus.InProgress,77);
                var userOptionsProvider = new UserOptionsProvider(config, httpProvider, resourceUploader);
                auditAction(GaugeStatus.InProgress,84);
                var cc = new ClientConnection(config, receiveEvents, authProvider, resourceUploader, jabberWireFactory.conversationDetailsProvider, resourceCache, jabberWireFactory, wcf, userOptionsProvider, httpProvider,auditor);
                auditAction(GaugeStatus.InProgress, 91);
                return cc;
            }), "create client connection", "backend");
        }
    }
}
