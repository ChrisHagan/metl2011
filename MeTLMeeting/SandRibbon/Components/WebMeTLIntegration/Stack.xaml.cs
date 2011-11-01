using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Net;
using System.Xml.Linq;
using AwesomiumSharp;
using SandRibbon.Providers;
using AwesomiumSharp.Windows.Controls;
using System.Collections.Specialized;
using MeTLLib.DataTypes;

namespace SandRibbon.Components.WebMeTLIntegration
{
    public class AwesomiumDragObject{
        public String _html {private set; get;}
        public Uri _originatingPage { private set; get; }
        public AwesomiumDragObject(String html, System.Uri originatingPage)
        {
            _html = html;
            _originatingPage = originatingPage;
        }
    }
    public partial class Stack : UserControl
    {
        private static readonly string WEB_METL_URL = "http://webmetl3.adm.monash.edu:8080";
        private static readonly Uri CAS_URL = new Uri("https://my.monash.edu.au/login");
        private AwesomiumSharp.Windows.Controls.WebControl chromeBrowser;
        private CookieAwareWebClient client;
        private bool isHealthy = false;
        private static WebCoreConfig chromeConfig = new WebCoreConfig { EnableJavascript = true, EnablePlugins = true, LogLevel = LogLevel.Verbose, SaveCacheAndCookies = true };
        static Stack()
        {
            WebCore.Initialize(chromeConfig);
        }
        public Stack()
        {
            InitializeComponent();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>((cred) => establishWebMeTLConnection(cred)));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
        }
        private void establishWebMeTLConnection(Credentials cred)
        {
            try
            {
                //setupChrome();
                if (client != null)
                    client = null;
                client = setupCAS(cred);
                hideStack();
                if (!String.IsNullOrEmpty(Globals.conversationDetails.Jid))
                   JoinConversation(Globals.conversationDetails.Jid); 
            }
            catch (Exception ex)
            {
                App.Now("failed to setup WebMeTL connection with exception: " + ex.Message);
                showRetryWindow();
            }
        }
        private void showRetryWindow(){
            isHealthy = false;
            Dispatcher.adopt(delegate{
                ReconnectContainer.Visibility = Visibility.Visible;
                browserContainer.Visibility = Visibility.Collapsed;
                availableTeachingEvents.Visibility = Visibility.Collapsed;
            });
        }
        private void setupChrome(){
            Dispatcher.adopt(delegate {
              if (chromeBrowser != null)
              {
                      chromeBrowser.Unloaded -= unloadBrowserHandler;
                      chromeBrowser.Crashed -= crashBrowserHandler;
                      chromeBrowser.PluginCrashed -= pluginCrashedHandler; 
                      chromeBrowser.Close();
                      browserContainer.Children.Clear();
              }
              chromeBrowser = new WebControl();
              chromeBrowser.SelectionChanged += chromeBrowser_SelectionChanged;
              chromeBrowser.PreviewMouseMove += chromeBrowser_dragMoving;
              chromeBrowser.Crashed += crashBrowserHandler;
              chromeBrowser.Unloaded += unloadBrowserHandler;
              chromeBrowser.PluginCrashed += pluginCrashedHandler;
              browserContainer.Children.Add(chromeBrowser);
            });
        }
        void chromeBrowser_dragMoving(object sender, MouseEventArgs e){
            if (e.LeftButton == MouseButtonState.Released) return;
            if (String.IsNullOrEmpty(chromeBrowser.Selection.HTML)) return;
            var hostUri = chromeBrowser.Source;
            var dataObj = new AwesomiumDragObject(chromeBrowser.Selection.HTML,hostUri);
            Console.WriteLine("dragging " + chromeBrowser.Selection.HTML); 
            var dragObj = new DataObject("text/awesomiumHtml", dataObj);
            DragDrop.DoDragDrop((DependencyObject)sender, dataObj, DragDropEffects.All);
        }
        void chromeBrowser_SelectionChanged(object sender, WebSelectionEventArgs e)
        {
            //AbstractCanvas.ProcessHtmlDrop(e.Selection.HTML);
        }
        private void unloadBrowserHandler(object _sender, RoutedEventArgs _args)
        {
            showRetryWindow();
        }
        private void crashBrowserHandler(object _sender, EventArgs _args)
        {
            showRetryWindow();
        }
        private void pluginCrashedHandler(object _sender, EventArgs _args)
        {
            showRetryWindow();
        }
        private CookieAwareWebClient setupCAS(Credentials cred){
            var client = new CookieAwareWebClient();
            var data = new NameValueCollection();
            data["access"] = "authcate";
            data["username"] = cred.name;
            data["password"] = cred.password;
            data["request_uri"] = "/authentication/cas/login?service=" + WEB_METL_URL;
            data["submit"] = "login";
            var casResponse = client.UploadValues(CAS_URL,data);
            var wm3Response = client.DownloadString(WEB_METL_URL);
            var cookies = client.GetCookies(new Uri(WEB_METL_URL));
            foreach(var _cookie in cookies)
            {
                var cookie = (Cookie)_cookie;
                var cookieString = String.Format("{0}={1}; domain={2}", cookie.Name, cookie.Value, cookie.Domain);
                WebCore.SetCookie(WEB_METL_URL, cookieString);
                isHealthy = true;
            }
            return client;
        }
        private void showStack() {
            if (isHealthy)
            {
                Dispatcher.adopt(delegate{
                    browserContainer.Visibility = Visibility.Visible;
                    availableTeachingEvents.Visibility = Visibility.Collapsed;
                    ReconnectContainer.Visibility = Visibility.Collapsed;
                });
            }
        }
        private void hideStack() {
            if (isHealthy)
            {
                Dispatcher.adopt(delegate{
                    browserContainer.Visibility = Visibility.Collapsed;
                    availableTeachingEvents.Visibility = Visibility.Visible;
                    ReconnectContainer.Visibility = Visibility.Collapsed;
                });
            }
        }
        private void ShutdownChrome(object _unused){
            if (chromeBrowser != null)
            {
                Dispatcher.adopt(delegate
                {
                    chromeBrowser.Close();
                });
            }
            WebCore.Shutdown();
        }
        private void JoinConversation(string conversationJid) { 
            var teachingEvents = teachingEventsForConversation(conversationJid);
            ListAvailableTeachingEvents(teachingEvents);
            if (teachingEvents.Count() == 1) 
                LoadStackForTeachingEvent(teachingEvents.ElementAt(0).code); 
        }
        private class TeachingEvent {
            public string code{get;set;}
            public string label { get; set;}
            public string author{get;set;}
        }
        private IEnumerable<TeachingEvent> teachingEventsForConversation(string conversationJid) {
            try
            {
                var eventXml = client.DownloadString(String.Format("{0}/teachingEventsForConversation/{1}", WEB_METL_URL, conversationJid));
                var events = XDocument.Parse(eventXml);
                return events.Descendants("course").SelectMany(course =>
                    course.Descendants("event").Select(teachingEvent =>
                    {
                        return new TeachingEvent
                        {
                            code = teachingEvent.Element("id").Value,
                            label = course.Element("name").Value,
                            author = String.Join(",", course.Elements("teacher").Select(e => e.Value))
                        };
                    }));
            }
            catch (Exception ex)
            {
                App.Now(String.Format("failed to get teachingEvents for Conversation {0} with exception: {1}", conversationJid, ex.Message));
                showRetryWindow();
                return new TeachingEvent[] { };
            }
        }
        private void LoadStackForTeachingEvent(string teachingEvent) {
            showStack();
            try
            {
                if (chromeBrowser != null)
                {
                    var uri = String.Format("{0}/stackOverflow/{1}", WEB_METL_URL, teachingEvent);
                    chromeBrowser.LoadURL(uri);
                }
            }
            catch (Exception ex)
            {
                showRetryWindow();
                App.Now(String.Format("failed to LoadStackForTeachingEvent {0} with exception: {1}", teachingEvent, ex.Message));
            }
        }
        private void ListAvailableTeachingEvents(IEnumerable<TeachingEvent> events) {
            hideStack();
            availableTeachingEvents.ItemsSource = events;
            switch(events.Count()){
                case 0:teachingEventSummary.DataContext = "No WebMeTL teaching events currently reference this conversation.";
                break;
                default: teachingEventSummary.DataContext = "The following WebMeTL teaching events reference this conversation.  Please select one, to follow the stack discussion.";
                break;
            } 
        }
        private void refreshTeachingEvents(object sender, RoutedEventArgs e)
        {
            JoinConversation(Globals.conversationDetails.Jid);
        }

        private void availableTeachingEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
                LoadStackForTeachingEvent(((TeachingEvent)e.AddedItems[0]).code);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ShutdownChrome(null);
        }

        private void AttemptToReconnect(object sender, RoutedEventArgs e)
        {
            establishWebMeTLConnection(Globals.credentials);
        }
    }
}

public class CookieAwareWebClient : WebClient
{
    //private CookieContainer m_container = new CookieContainer();
    private CookieContainer m_container;
    public CookieAwareWebClient()
    {
       m_container = Globals.cookieContainer;
    }
    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest request = base.GetWebRequest(address);
        if (request is HttpWebRequest)
        {
            (request as HttpWebRequest).CookieContainer = m_container;
        }
        return request;
    }
    public CookieCollection GetCookies(Uri uri) {
        return m_container.GetCookies(uri);
    }
}