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
            return auditor.wrapTask(((auditAction) =>
            {
                var webCreds = new NetworkCredential(config.resourceUsername, config.resourcePassword);
                auditAction(GaugeStatus.InProgress);
                //var jabberCreds = new Credentials(config.xmppUsername, config.xmppPassword,new List<AuthorizedGroup>(),"");
                var wcf = new WebClientFactory(webCreds,auditor);
                auditAction(GaugeStatus.InProgress);
                var receiveEvents = new ProductionReceiveEvents();
                auditAction(GaugeStatus.InProgress);
                var authProvider = new AuthorisationProvider(wcf, config,auditor);
                auditAction(GaugeStatus.InProgress);
                var httpProvider = new HttpResourceProvider(wcf,auditor);
                auditAction(GaugeStatus.InProgress);
                var resourceUploaderFactory = new ProductionResourceUploaderFactory(config, httpProvider);
                auditAction(GaugeStatus.InProgress);
                var resourceUploader = resourceUploaderFactory.get();
                auditAction(GaugeStatus.InProgress);
                var resourceCache = new ResourceCache();
                auditAction(GaugeStatus.InProgress);
                var configurationProvider = new ConfigurationProvider(wcf,auditor);
                auditAction(GaugeStatus.InProgress);
                var conversationDetailsProvider = new FileConversationDetailsProvider(config, wcf, resourceUploader, creds,auditor);
                auditAction(GaugeStatus.InProgress);
                var jabberWireFactory = new JabberWireFactory(config, creds, configurationProvider, conversationDetailsProvider, resourceCache, receiveEvents, wcf, httpProvider,auditor);
                auditAction(GaugeStatus.InProgress);
                var userOptionsProvider = new UserOptionsProvider(config, httpProvider, resourceUploader);
                var cc = new ClientConnection(config, receiveEvents, authProvider, resourceUploader, conversationDetailsProvider, resourceCache, jabberWireFactory, wcf, userOptionsProvider, httpProvider);
                return cc;
            }), "create client connection", "backend");
        }
    }
}
