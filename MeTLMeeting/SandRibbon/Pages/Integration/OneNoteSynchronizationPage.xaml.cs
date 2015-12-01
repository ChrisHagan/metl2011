using MeTLLib;
using MeTLLib.DataTypes;
using Newtonsoft.Json.Linq;
using SandRibbon.Components;
using SandRibbon.Pages.Conversations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Navigation;

namespace SandRibbon.Pages.Integration
{
    public partial class OneNoteSynchronizationPage : Page, ServerAwarePage
    {
        public static string METL_NOTEBOOK_TITLE = "MeTL";
        public NetworkController networkController {get; protected set;}
        public UserGlobalState userGlobal { get; protected set; }
        public UserServerState userServer { get; protected set; }

        public OneNoteSynchronizationPage(UserGlobalState _userGlobal, UserServerState _userServer, NetworkController _controller, OneNoteSynchronizationSet sync)
        {
            networkController = _controller;
            userGlobal = _userGlobal;
            userServer = _userServer;
            InitializeComponent();
            DataContext = sync;
            SynchronizeConversations();
        }
        public async void SynchronizeConversations()
        {
            var sync = DataContext as OneNoteSynchronizationSet;
            var configuration = sync.config;
            configuration.LoadNotebooks(sync.token);
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", sync.token));
            var metlNotebook = configuration.Books.Where(b => b.Name == METL_NOTEBOOK_TITLE);
            if (metlNotebook.Count() == 0)
            {
                var data = JObject.FromObject(new
                {
                    name = METL_NOTEBOOK_TITLE
                }).ToString();
                await httpClient.PostAsync(OneNoteConfiguration.notebooks, new StringContent(data, Encoding.UTF8, "application/json"));
                configuration.LoadNotebooks(sync.token);
                metlNotebook = configuration.Books.Where(b => b.Name == METL_NOTEBOOK_TITLE);
            }
            var metlNotebookId = metlNotebook.First().Id;
            var timestamp = DateTime.Now.Ticks;
            foreach (var c in sync.conversations)
            {
                var escapedTitle = string.Join("", c.Conversation.Title.Split('?', '*', '\\', '/', ':', '<', '>', '|', '&', '#', '"', '%', '~'));
                var section = metlNotebook.First().Sections.Where(s => s.Name == escapedTitle);
                string pagesUrl;
                if (section.Count() == 0)
                {
                    var content = JObject.FromObject(new
                    {
                        name = escapedTitle
                    }).ToString();
                    var req = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = new StringContent(content, Encoding.UTF8, "application/json"),
                        RequestUri = new Uri(string.Format(OneNoteConfiguration.sections, metlNotebookId))
                    };
                    var response = await httpClient.SendAsync(req);
                    var jTask = response.Content.ReadAsStringAsync();
                    jTask.Wait();
                    var jObject = JObject.Parse(jTask.Result);
                    pagesUrl = jObject["pagesUrl"].Value<string>();
                }
                else
                {
                    pagesUrl = section.First().PagesUrl;
                }
                c.Progress = 0;
                var bucketSize = 30;
                foreach (var bucket in c
                    .Conversation
                    .Slides
                    .OrderBy(s => s.index)
                    .Select((s, i) => new { s, i })
                    .GroupBy(g => g.i / bucketSize, g => g.s))
                {
                    var content = new MultipartFormDataContent();
                    var images = new List<XElement>();
                    foreach (var slide in bucket)
                    {
                        var blockName = string.Format("IMAGE{0}", slide.id);
                        var url = networkController.config.renderUri(slide.id.ToString(),1024,768);
                        var t = await httpClient.GetAsync(url);
                        var data = await t.Content.ReadAsByteArrayAsync();
                        var dataContent = new ByteArrayContent(data);
                        dataContent.Headers.Add("Content-Type", "image/jpeg");
                        content.Add(dataContent, blockName);
                        c.Progress++;
                        Console.WriteLine(c.Progress);
                        images.Add(new XElement("img", new XAttribute("src", string.Format("name:{0}", blockName))));
                    }
                    content.Add(
                        new StringContent(
                            new XElement("html",
                                new XElement("head",
                                    new XElement("title",
                                        string.Format("MeTL: {0} @ {1} {2}",
                                        c.Conversation.Title,
                                        DateTime.Now.ToShortDateString(),
                                        DateTime.Now.ToShortTimeString()))),
                                new XElement("body",
                                images)).ToString(),
                            Encoding.UTF8, "text/html"), "Presentation");
                    var req = new HttpRequestMessage
                    {
                        Content = content,
                        RequestUri = new Uri(pagesUrl),
                        Method = HttpMethod.Post
                    };
                    var response = await httpClient.SendAsync(req);
                    Console.WriteLine(response);
                }
            }
            NavigationService.GoBack();
        }

        public NetworkController getNetworkController()
        {
            return networkController;
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

/*To delete all pages
                    foreach (var page in section.First().Pages)
                    {
                        var deleteRequest = new HttpRequestMessage {
                            Method = HttpMethod.Delete,
                            RequestUri = new Uri(string.Format(OneNoteConfiguration.pages_delete, page.Id))
                        };
                        var result = await httpClient.SendAsync(deleteRequest);
                        Console.WriteLine(result);
                    }
                    */
