using System.Collections.ObjectModel;
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using System.Diagnostics;
using SandRibbon.Components;

namespace SandRibbon.Pages.Conversations.Models
{
    public class OneNoteConfiguration : DependencyObject
    {
        public ObservableCollection<Notebook> Books
        {
            get { return (ObservableCollection<Notebook>)GetValue(BooksProperty); }
            set { SetValue(BooksProperty, value); }
        }

        public static readonly DependencyProperty BooksProperty =
            DependencyProperty.Register("Books", typeof(ObservableCollection<Notebook>), typeof(OneNoteConfiguration), new PropertyMetadata(new ObservableCollection<Notebook>()));

        public string apiKey { get; set; }
        public string apiSecret { get; set; }
        public NetworkController networkController {get; set;}

        public static string notebooks = "https://www.onenote.com/api/v1.0/me/notes/notebooks?expand=sections";
        public static string sections = "https://www.onenote.com/api/v1.0/me/notes/notebooks/{0}/sections";
        public static string pages = "https://www.onenote.com/api/v1.0/me/notes/sections/{0}/pages";
        public static string pages_delete = "https://www.onenote.com/api/v1.0/me/notes/pages/{0}/";

        public void LoadNotebooks(string token)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            var jsonS = wc.DownloadString(notebooks);
            var json = JObject.Parse(jsonS);
            var remoteBooks = json["value"].Children<JObject>();
            foreach (var j in remoteBooks)
            {
                var book = new Notebook
                {
                    Name = j["name"].Value<string>(),
                    Id = j["id"].Value<string>(),
                    networkController = networkController
                };
                Books.Add(book);
                foreach (var s in j["sections"].Children<JObject>())
                {
                    var section = new NotebookSection
                    {
                        Name = s["name"].Value<string>(),
                        Id = s["id"].Value<string>(),
                        PagesUrl = s["pagesUrl"].Value<string>(),
                        networkController = networkController
                    };                    
                    book.Sections.Add(section);
                    LoadPages(token, section);
                }
            }
        }
        public void LoadPages(string token, NotebookSection section)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            wc.DownloadStringCompleted += (s, e) =>
            {
                var json = JObject.Parse(e.Result);
                var remotePages = json["value"].Children<JObject>();
                foreach (var j in remotePages)
                {
                    var title = j["title"].Value<string>();
                    try
                    {
                        section.Pages.Add(new NotebookPage
                        {
                            Token = token,
                            OriginalHtml = wc.DownloadString(j["contentUrl"].Value<string>()),
                            Title = title,
                            Id = j["id"].Value<string>(),
                            networkController = networkController
                        });
                    }
                    catch (WebException ex) {
                        Trace.TraceError("OneNoteConfiguration LoadPages {0}",ex.Message);                           
                    }
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(pages, section.Id)));
        }                
    }
    public class OneNoteSynchronizationSet {
        public string token { get; set; }
        public OneNoteConfiguration config { get; set; }
        public IEnumerable<OneNoteSynchronization> conversations { get; set; }
        public NetworkController networkController { get; set; }
    }
    public class OneNoteSynchronization : DependencyObject {
        public ConversationDetails Conversation { get; set; }

        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }        
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(int), typeof(OneNoteSynchronization), new PropertyMetadata(0));        
    }
    public class NotebookPage
    {        
        public string Html
        {
            get
            {
                var xDoc = XDocument.Parse(OriginalHtml);
                var elements = xDoc.Descendants().Where(d => d.Name.LocalName == "img");                
                foreach (var img in elements)
                {                    
                    var source = img.Attribute("src").Value;
                    var wc = new WebClient();
                    wc.Headers.Add("Authorization", string.Format("Bearer {0}", Token));
                    var oneNoteData = wc.DownloadData(source);
                    var alias = string.Format("{0}.png", Guid.NewGuid().ToString());
                    var upload = networkController.client.UploadResourceToPath(oneNoteData, "onenote", alias, false);
                    var securedUpload = new UriBuilder(upload)
                    {
                        Scheme = "https"
                    };
                    img.SetAttributeValue("src", securedUpload.Uri.AbsoluteUri);
                }
                return xDoc.ToString();
            }            
        }
        public string OriginalHtml { get; set; }
        public string Title { get; set; }
        public string Token { get; set; }   
        public string Id { get; set; }     
        public NetworkController networkController { get; set; }
    }
    public class NotebookSection : DependencyObject
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string PagesUrl { get; set; }
        public NetworkController networkController { get; set; }

        public ObservableCollection<NotebookPage> Pages
        {
            get { return (ObservableCollection<NotebookPage>)GetValue(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly DependencyProperty PagesProperty =
            DependencyProperty.Register("Pages", typeof(ObservableCollection<NotebookPage>), typeof(NotebookSection), new PropertyMetadata(new ObservableCollection<NotebookPage>()));
    }
    public class Notebook : DependencyObject
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public NetworkController networkController { get; set; }

        public ObservableCollection<NotebookSection> Sections
        {
            get { return (ObservableCollection<NotebookSection>)GetValue(SectionsProperty); }
            set { SetValue(SectionsProperty, value); }
        }

        public static readonly DependencyProperty SectionsProperty =
            DependencyProperty.Register("Sections", typeof(ObservableCollection<NotebookSection>), typeof(Notebook), new PropertyMetadata(new ObservableCollection<NotebookSection>()));
    }
}
