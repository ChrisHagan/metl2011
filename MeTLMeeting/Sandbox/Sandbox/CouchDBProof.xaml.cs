using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Divan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sandbox
{
    public class ConversationDetails : CouchDocument
    {
        public string author;
        public DateTime created;
        public IEnumerable<Slide> slides;
        public override void ReadJson(Newtonsoft.Json.Linq.JObject obj)
        {
            base.ReadJson(obj);
            author = obj["author"].Value<string>();
            created = obj["created"].Value<DateTime>();
            var slideArray = obj["slides"].Value<JArray>();
            slides = slideArray.Select<JToken, Slide>(o => {
                var slide = new Slide();
                slide.ReadJson((JObject)o);
                return slide;
            }).ToList();
        }
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer)
        {
            base.WriteJson(writer);
            writer.WritePropertyName("author");
            writer.WriteValue(author);
            writer.WritePropertyName("created");
            writer.WriteValue(created);
            writer.WritePropertyName("slides");
            writer.WriteStartArray();
            foreach (var slide in slides)
                slide.WriteJsonObject(writer);
            writer.WriteEndArray();
        }
    }
    public class Slide : CouchDocument
    {/*This is a CouchDocument for the convenience of object serialization, not because we need Ids or Revisions.  
      * The parent will take care of revisioning the entire tree.*/
        public string author;
        public int id;
        public int index;
        public override void ReadJson(JObject obj)
        {
            author = obj["author"].Value<string>();
            id = obj["id"].Value<int>();
            index = obj["index"].Value<int>();
        }
        public override void WriteJson(JsonWriter writer)
        {
            writer.WritePropertyName("author");
            writer.WriteValue(author);
            writer.WritePropertyName("id");
            writer.WriteValue(id);
            writer.WritePropertyName("index");
            writer.WriteValue(index);
        }
    }
    public partial class CouchDBProof : Window
    {
        CouchServer server = new CouchServer("drawkward.adm.monash.edu.au");
        public CouchDBProof()
        {
            InitializeComponent();
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var db = server.GetDatabase("conversation");
            var doc = db.SaveDocument(new ConversationDetails
            {
                author = "anAuthor",
                created = DateTime.Now,
                slides = Enumerable.Range(0, 10).Select(i =>
                new Slide
                {
                    author = "anAuthor",
                    id = i
                }).ToList()
            });
            var docs = db.GetAllDocuments();
            var aConversation = db.GetDocument<ConversationDetails>(doc.Id);
        }
    }
}
