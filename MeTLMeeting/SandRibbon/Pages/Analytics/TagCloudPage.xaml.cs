using Awesomium.Windows.Controls;
using MeTLLib.DataTypes;
using Newtonsoft.Json.Linq;
using SandRibbon.Components;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Navigation;

namespace SandRibbon.Pages.Analytics
{
    public partial class TagCloudPage : ConversationAwarePage
    {
        public NetworkController controller { get; protected set; }
        public ConversationDetails details { get; protected set; }
        public UserGlobalState userGlobal { get; protected set; }
        public UserServerState userServer { get; protected set; }
        public TagCloudPage(NetworkController networkController, ConversationDetails _details, UserGlobalState _userGlobal, UserServerState _userServer) : base()
        {
            InitializeComponent();
            details = _details;
            controller = networkController;
            userGlobal = _userGlobal;
            userServer = _userServer;
            loadWorkbench();            
        }

        private void loadWorkbench()
        {            
            var wc = new WebControl();
            wc.WebSession = userServer.authenticatedWebSession;
            Content = wc;
            wc.DocumentReady += Wc_DocumentReady;
            wc.ProcessCreated += delegate
            {
                wc.Source = controller.client.server.widgetUri;
            };
        }
        
        public IEnumerable<string> Themes(Slide slide) {
            var page = controller.client.server.themes(slide.id);
            var wc = new WebClient();
            var ts = XDocument.Parse(wc.DownloadString(page)).Descendants("theme").Select(t => t.Value);
            Console.WriteLine("slide {0}", slide.id);
            foreach (var t in ts) Console.WriteLine(t);
            return ts;
        }

        public IEnumerable<string> Words(Slide slide)
        {
            var page = string.Format("http://localhost:8080/words/{0}", slide.id);
            var wc = new WebClient();
            var ts = XDocument.Parse(wc.DownloadString(page)).Descendants("theme").Select(t => t.Value);
            Console.WriteLine("slide {0}", slide.id);
            foreach (var t in ts) Console.WriteLine(t);
            return ts;
        }

        bool tagsRendered = false;
        private void Wc_DocumentReady(object sender, Awesomium.Core.DocumentReadyEventArgs e)
        {
            var wc = sender as WebControl;
            if (wc.HTML.Contains("function wordcloud"))
            {
                if (!tagsRendered)
                {
                    tagsRendered = true;
                    var themes = new List<String>();
                    var count = 0;
                    var max = details.Slides.Count;
                    foreach (var slide in details.Slides) {
                        ThreadPool.QueueUserWorkItem(delegate
                       {
                           themes.AddRange(Themes(slide));
                           Dispatcher.Invoke(new Action(delegate
                           {
                               var themeCopy = new List<string>(themes);
                               var jsFormat = "wordcloud({0},{1},{2})";
                               wc.ExecuteJavascriptWithResult(string.Format(jsFormat,
                                   new JArray(themeCopy.GroupBy(t => t).Select(ts => new JArray(ts.Key, ts.Count()))),
                                   ++count,
                                   max));                               
                           }));
                           Console.WriteLine("Rendered {0}", slide.index);
                       });
                    }                                          
                }
            }
        }

        public ConversationDetails getDetails()
        {
            throw new NotImplementedException();
        }

        public UserConversationState getUserConversationState()
        {
            throw new NotImplementedException();
        }

        public NetworkController getNetworkController()
        {
            throw new NotImplementedException();
        }

        public UserServerState getUserServerState()
        {
            return userServer;
        }

        public UserGlobalState getUserGlobalState()
        {
            return userGlobal;
        }

        public NavigationService getNavigationService()
        {
            return NavigationService;
        }
    }
}
