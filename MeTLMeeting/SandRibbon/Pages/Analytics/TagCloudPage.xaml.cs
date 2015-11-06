using Awesomium.Windows.Controls;
using MeTLLib.DataTypes;
using Newtonsoft.Json.Linq;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace SandRibbon.Pages.Analytics
{
    public partial class TagCloudPage : Page
    {
        public TagCloudPage()
        {
            InitializeComponent();            
            loadWorkbench();            
        }
        public void loadWorkbench() {
            var wc = new WebControl();
            wc.WebSession = Globals.authenticatedWebSession;
            Content = wc;
            wc.DocumentReady += Wc_DocumentReady;
            wc.ProcessCreated += delegate
            {
                wc.Source = new Uri("http://localhost:8080/static/widget.html");
            };
        }

        static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

        public IEnumerable<string> Themes(Slide slide) {
            var page = string.Format("http://localhost:8080/themes?source={0}", slide.id);
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
                    var themes = new JArray(Globals.slides.SelectMany(Themes).GroupBy(t => t).Select(ts => new JArray(ts.Key, ts.Count())));
                    Console.WriteLine(themes.ToString());
                    Dispatcher.Invoke(() =>
                    {
                        var jsFormat = "wordcloud({0})";
                        wc.ExecuteJavascript(string.Format(jsFormat, themes.ToString()));
                    });                                           
                }
            }
        }        
    }
}
