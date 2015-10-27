using System.Collections.ObjectModel;
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Xml.Linq;
using System.Windows;

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

        static string notebooks = "https://www.onenote.com/api/v1.0/me/notes/notebooks?expand=sections";
        static string pages = "https://www.onenote.com/api/v1.0/me/notes/sections/{0}/pages";
        
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
                    Name = j["name"].Value<string>()
                };
                Books.Add(book);
                foreach (var s in j["sections"].Children<JObject>())
                {
                    var section = new NotebookSection
                    {
                        Name = s["name"].Value<string>(),
                        Id = s["id"].Value<string>()
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
                    section.Pages.Add(new NotebookPage
                    {
                        Token = token,
                        OriginalHtml = wc.DownloadString(j["contentUrl"].Value<string>()),
                        Title = title
                    });
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(pages, section.Id)));
        }
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
                    var upload = MeTLLib.ClientFactory.Connection().UploadResourceToPath(oneNoteData, "onenote", alias, false);
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
    }
    public class NotebookSection : DependencyObject
    {
        public string Name { get; set; }
        public string Id { get; set; }

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

        public ObservableCollection<NotebookSection> Sections
        {
            get { return (ObservableCollection<NotebookSection>)GetValue(SectionsProperty); }
            set { SetValue(SectionsProperty, value); }
        }

        public static readonly DependencyProperty SectionsProperty =
            DependencyProperty.Register("Sections", typeof(ObservableCollection<NotebookSection>), typeof(Notebook), new PropertyMetadata(new ObservableCollection<NotebookSection>()));
    }
}
