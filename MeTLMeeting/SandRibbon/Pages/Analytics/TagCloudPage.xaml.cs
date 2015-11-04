using Awesomium.Windows.Controls;
using MeTLLib.DataTypes;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SandRibbon.Pages.Analytics
{
    public partial class TagCloudPage : Page
    {
        public TagCloudPage(Slide context)
        {
            InitializeComponent();            
            DataContext = context;
            loadWorkbench();            
        }
        public void loadWorkbench() {            
            wc.DocumentReady += Wc_DocumentReady;
            wc.Source = new System.Uri("http://localhost:8080/static/widget.html");
        }

        private void Wc_DocumentReady(object sender, Awesomium.Core.DocumentReadyEventArgs e)
        {
            var wc = sender as WebControl;
            var slide = DataContext as Slide;
            var page = string.Format("http://localhost:8080/themes?source={0}", slide.id);
            var t = new Task(async delegate {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(page))
                using (HttpContent content = response.Content)
                {
                    var s = await content.ReadAsStringAsync();
                    /*
                    var themes = String.Join(",", from theme in XDocument.Load(s).Descendants("theme")
                                                  select theme.Value);
                                                  */
                    var themes = "[['dog',2],,['cat',9],['mouse',3],['house',4],['vamoose',3],['moose',4],['chairs',3]]";
                    wc.ExecuteJavascript(string.Format("wordcloud({0});", themes));
                }
            });
            t.Start();      
        }
    }
}
