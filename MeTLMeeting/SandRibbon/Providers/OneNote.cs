using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace SandRibbon.Providers
{
    public class OneNote
    {
        static string notebooks = "https://www.onenote.com/api/v1.0/me/notes/notebooks?expand=sections";
        static string pages = "https://www.onenote.com/api/v1.0/me/notes/sections/{0}/pages";
        public static List<Notebook> Notebooks(string token)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            var jsonS = wc.DownloadString(notebooks);
            var json = JObject.Parse(jsonS);
            var books = new List<Notebook>();
            foreach (var j in json["value"].Children<JObject>())
            {
                var book = new Notebook
                {
                    Name = j["name"].Value<string>()
                };
                foreach (var s in j["sections"].Children<JObject>())
                {
                    var section = new NotebookSection
                    {
                        Name = s["name"].Value<string>(),
                        Id = s["id"].Value<string>()
                    };
                    Pages(token, section);
                    book.Sections.Add(section);
                }
                books.Add(book);
            }
            return books;
        }

        public static void Pages(string token, NotebookSection section)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            wc.DownloadStringCompleted += (s, e) =>
            {           
                var json = JObject.Parse(e.Result);
                foreach (var j in json["value"].Children<JObject>())
                {
                    section.Pages.Add(new NotebookPage
                    {
                        Token = token,
                        Html = wc.DownloadString(j["contentUrl"].Value<string>()),
                        Title = j["title"].Value<string>()                        
                    });
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(OneNote.pages, section.Id)));
        }
    }
    public class NotebookPage
    {
        private string html;
        public string Html { get { return html; }
            set {
                var xDoc = XDocument.Parse(value);
                foreach (var img in xDoc.Descendants().Where(d => d.Name.LocalName == "img")) {
                    var source = img.Attribute("src").Value;
                    var wc = new WebClient();
                    wc.Headers.Add("Authorization", string.Format("Bearer {0}", Token));          
                    var oneNoteData = wc.DownloadData(source);
                    var alias = string.Format("{0}.png",Guid.NewGuid().ToString());                                        
                    var upload = MeTLLib.ClientFactory.Connection().UploadResourceToPath(oneNoteData,"onenote",alias,false);                    
                    img.SetAttributeValue("src", upload.AbsoluteUri);
                }
                html = xDoc.ToString();               
            }
        }
        public string Title { get; set; }        
        public string Token { get; set; }
    }
    public class NotebookSection
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public ObservableCollection<NotebookPage> Pages { get; set; } = new ObservableCollection<NotebookPage>();
    }
    public class Notebook
    {
        public string Name { get; set; }
        public ObservableCollection<NotebookSection> Sections { get; set; } = new ObservableCollection<NotebookSection>();
    }
}
