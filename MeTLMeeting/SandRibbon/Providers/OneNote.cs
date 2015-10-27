using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using SandRibbon.Pages.Conversations.Models;
using MeTLLib;

namespace SandRibbon.Providers
{
    public class OneNote
    {
        public MetlConfiguration backend { get; protected set; }
        public OneNoteConfiguration config { get; protected set; }
        public OneNote(OneNoteConfiguration _config)
        {
            backend = _config.backend;
            config = _config;
        }
        static string notebooks = "https://www.onenote.com/api/v1.0/me/notes/notebooks?expand=sections";
        static string pages = "https://www.onenote.com/api/v1.0/me/notes/sections/{0}/pages";
        public List<Notebook> Notebooks(string token)
        {
            var context = config;
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            var jsonS = wc.DownloadString(notebooks);
            var json = JObject.Parse(jsonS);
            var books = new List<Notebook>();
            var remoteBooks = json["value"].Children<JObject>();
            var bookCount = remoteBooks.Count();
            var booksProcessed = 0;
            context.ReportPagesProgress(booksProcessed, bookCount);
            foreach (var j in remoteBooks)
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
                    Pages(token, section, context);
                    book.Sections.Add(section);
                }
                books.Add(book);
                context.ReportPagesProgress(booksProcessed++, bookCount);
            }
            return books;
        }

        public void Pages(string token, NotebookSection section, OneNoteConfiguration context)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            wc.DownloadStringCompleted += (s, e) =>
            {
                var processed = 0;
                var json = JObject.Parse(e.Result);
                var remotePages = json["value"].Children<JObject>();
                var pageCount = remotePages.Count();
                context.ReportPageProgress(processed, pageCount, null);
                foreach (var j in remotePages)
                {
                    var title = j["title"].Value<string>();
                    section.Pages.Add(new NotebookPage
                    {
                        Token = token,
                        Html = wc.DownloadString(j["contentUrl"].Value<string>()),
                        Title = title,
                        Backend = backend,
                        context = context
                    });
                    context.ReportPageProgress(processed, pageCount, title);
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(OneNote.pages, section.Id)));
        }
    }
    public class NotebookPage
    {
        private string html;
        public string Html
        {
            get { return html; }
            set
            {
                var xDoc = XDocument.Parse(value);
                foreach (var img in xDoc.Descendants().Where(d => d.Name.LocalName == "img"))
                {
                    var source = img.Attribute("src").Value;
                    var wc = new WebClient();
                    wc.Headers.Add("Authorization", string.Format("Bearer {0}", Token));
                    var oneNoteData = wc.DownloadData(source);
                    var alias = string.Format("{0}.png", Guid.NewGuid().ToString());
                    var upload = App.getContextFor(Backend).controller.client.UploadResourceToPath(oneNoteData, "onenote", alias, false);
                    var securedUpload = new UriBuilder(upload)
                    {
                        Scheme = "https"
                    };
                    img.SetAttributeValue("src", securedUpload.Uri.AbsoluteUri);
                }
                html = xDoc.ToString();
            }
        }
        public string Title { get; set; }
        public string Token { get; set; }
        public OneNoteConfiguration context { get; set; }
        public MetlConfiguration Backend { get; set; }
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
