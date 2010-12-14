using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.IO;
using SharpGIS;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Xml;

namespace NavigationMeTL.Views
{
    public partial class Conversations : Page
    {
        private ObservableCollection<string> conversations = new ObservableCollection<string>();
        public Conversations()
        {
            InitializeComponent();
        }
        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var useClientTrustLevel = WebRequest.RegisterPrefix("https://", System.Net.Browser.WebRequestCreator.ClientHttp);
            var client = new WebClient
            {
                Credentials = new NetworkCredential
                {
                    UserName = "exampleUsername",
                    Password = "examplePassword"
                }                
            };
            client.UseDefaultCredentials = false;
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
            client.OpenReadAsync(new Uri("https://madam.adm.monash.edu.au:1188/Structure/all.zip"));
        }
        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            var info = new StreamResourceInfo(e.Result, null);
            var zipper = new UnZipper(info.Stream);
            foreach(var dir in zipper.DirectoriesInZip)
                conversations.Add(XElement.Load(zipper.GetFileStream(string.Format("{0}/{1}",dir, "details.xml"))).ToString());
        }
    }
}
