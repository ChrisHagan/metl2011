using System.Collections.ObjectModel;
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace SandRibbon.Pages.Conversations.Models
{
    public class OneNoteProcessingProgress {
        public int booksProgress { get; set; } = 0;
        public int maxBooks { get; set; } = 0;

        public int pagesProgress { get; set; } = 0;
        public int maxPages { get; set; } = 0;

        public int elementsProgress { get; set; } = 0;
        public int maxElements { get; set; } = 0;
    }
    public class OneNoteConfiguration : DependencyObject
    {
        public ObservableCollection<Notebook> Books { get; set; } = new ObservableCollection<Notebook>();
        public string apiKey { get; set; }
        public string apiSecret { get; set; }        

        static string notebooks = "https://www.onenote.com/api/v1.0/me/notes/notebooks?expand=sections";
        static string pages = "https://www.onenote.com/api/v1.0/me/notes/sections/{0}/pages";

        public int maxBooks
        {
            get { return (int)GetValue(maxBooksProperty); }
            set { SetValue(maxBooksProperty, value); }
        }       
        public static readonly DependencyProperty maxBooksProperty =
            DependencyProperty.Register("maxBooks", typeof(int), typeof(OneNoteConfiguration), new PropertyMetadata(0));

        public int maxPages
        {
            get { return (int)GetValue(maxPagesProperty); }
            set { SetValue(maxPagesProperty, value); }
        }
        public static readonly DependencyProperty maxPagesProperty =
            DependencyProperty.Register("maxPages", typeof(int), typeof(OneNoteConfiguration), new PropertyMetadata(0));

        public int maxElements
        {
            get { return (int)GetValue(maxElementsProperty); }
            set { SetValue(maxElementsProperty, value); }
        }
        public static readonly DependencyProperty maxElementsProperty =
            DependencyProperty.Register("maxElements", typeof(int), typeof(OneNoteConfiguration), new PropertyMetadata(0));

        public int currentBooks
        {
            get { return (int)GetValue(currentBooksProperty); }
            set { SetValue(currentBooksProperty, value); }
        }
        public static readonly DependencyProperty currentBooksProperty =
            DependencyProperty.Register("currentBooks", typeof(int), typeof(OneNoteConfiguration), new PropertyMetadata(0));

        public int currentPages
        {
            get { return (int)GetValue(currentPagesProperty); }
            set { SetValue(currentPagesProperty, value); }
        }
        public static readonly DependencyProperty currentPagesProperty =
            DependencyProperty.Register("currentPages", typeof(int), typeof(OneNoteConfiguration), new PropertyMetadata(0));

        public int currentElements
        {
            get { return (int)GetValue(currentElementsProperty); }
            set { SetValue(currentElementsProperty, value); }
        }
        public static readonly DependencyProperty currentElementsProperty =
            DependencyProperty.Register("currentElements", typeof(int), typeof(OneNoteConfiguration), new PropertyMetadata(0));


        public void LoadNotebooks(string token)
        {
            var worker = new BackgroundWorker {
                WorkerReportsProgress = true
            };
            worker.DoWork += (sender, e) =>
            {
                var progress = e.Argument as OneNoteProcessingProgress;
                var wc = new WebClient();
                wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
                var jsonS = wc.DownloadString(notebooks);
                var json = JObject.Parse(jsonS);
                var books = new List<Notebook>();
                var remoteBooks = json["value"].Children<JObject>();
                worker.ReportProgress(0, progress);
                progress.booksProgress = 0;
                progress.maxBooks = remoteBooks.Count();                
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
                        //LoadPages(token, section,worker,progress);
                        book.Sections.Add(section);
                    }
                    books.Add(book);
                    progress.booksProgress++;
                }
            };
            worker.ProgressChanged += (sender,p) => {
                var progress = p.UserState as OneNoteProcessingProgress;
                maxBooks = progress.maxBooks;
                currentBooks = progress.booksProgress;
                maxPages = progress.maxPages;
                currentPages = progress.pagesProgress;
                maxElements = progress.maxElements;
                currentElements = progress.elementsProgress;
            };
            worker.RunWorkerAsync(new OneNoteProcessingProgress());
        }        
        public void LoadPages(string token, NotebookSection section, BackgroundWorker worker, OneNoteProcessingProgress progress)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            wc.DownloadStringCompleted += (s, e) =>
            {
                var json = JObject.Parse(e.Result);
                var remotePages = json["value"].Children<JObject>();
                progress.maxPages = remotePages.Count();
                worker.ReportProgress(0, progress);
                foreach (var j in remotePages)
                {
                    var title = j["title"].Value<string>();
                    section.Pages.Add(new NotebookPage
                    {
                        Token = token,
                        Context = this,
                        Html = wc.DownloadString(j["contentUrl"].Value<string>()),
                        Title = title
                    });
                    progress.pagesProgress++;
                    worker.ReportProgress(0, progress);
                }
            };
            wc.DownloadStringAsync(new Uri(string.Format(pages, section.Id)));
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
                var elements = xDoc.Descendants().Where(d => d.Name.LocalName == "img");
                Context.maxElements = elements.Count();
                Context.currentElements = 0;
                foreach (var img in elements)
                {
                    Context.currentElements++;
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
                html = xDoc.ToString();
            }
        }
        public string Title { get; set; }
        public string Token { get; set; }
        public OneNoteConfiguration Context { get; set; }
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
