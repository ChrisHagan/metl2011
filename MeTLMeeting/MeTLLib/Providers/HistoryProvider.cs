using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using agsXMPP.Xml;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using System.Diagnostics;
using System.Xml.Linq;

namespace MeTLLib.Providers
{
    public class HistorySummary{
        public int ActivityCount { get; set; }
        public string Id { get; set; }
        public List<String> Viewers { get; set; } = new List<String>();
        public List<String> Actors { get; set; } = new List<String>();
        public List<String> Spectators { get; set; } = new List<String>();
        public int ViewersCount { get { return Viewers.Count(); } }
        public int ActorsCount { get { return Actors.Count(); } }
        public int SpectatorsCount { get { return Spectators.Count(); } }

        public static HistorySummary parse(string xml) {
            var x = XElement.Parse(xml);
            var summary = new HistorySummary {
                Id = x.Descendants("jid").First().Value,
                ActivityCount = Int32.Parse(x.Descendants("stanzaCount").First().Value),
                Viewers = x.Descendants("occupant").SelectMany(o => o.Descendants("name").Select(d => d.Value)).ToList(),
                Actors = x.Descendants("publisher").SelectMany(o => o.Descendants("name").Select(d => d.Value)).ToList()
            };
            summary.Spectators = summary.Viewers.Except(summary.Actors).ToList();
            return summary;
        }        
    }
    public interface IHistoryProvider
    {
        void Retrieve<T>(
            Action retrievalBeginning,
            Action<int, int> retrievalProceeding,
            Action<T> retrievalComplete,
            string room
        ) where T : PreParser;
        void RetrievePrivateContent<T>(
            Action retrievalBeginning,
            Action<int, int> retrievalProceeding,
            Action<T> retrievalComplete,
            string author,
            string room
        ) where T : PreParser;
        HistorySummary Describe(int id);
    }
    public abstract class BaseHistoryProvider : IHistoryProvider
    {
        protected HttpResourceProvider resourceProvider;
        protected JabberWireFactory jabberWireFactory;
        protected MetlConfiguration serverAddress;
        protected IAuditor auditor;
        public BaseHistoryProvider(
                HttpResourceProvider _resourceProvider,
                JabberWireFactory _jabberWireFactory,
                MetlConfiguration _serverAddress,
                IAuditor _auditor
        )
        {
            resourceProvider = _resourceProvider;
            jabberWireFactory = _jabberWireFactory;
            serverAddress = _serverAddress;
            auditor = _auditor;
        }
        public abstract void Retrieve<T>(
                Action retrievalBeginning,
                Action<int, int> retrievalProceeding,
                Action<T> retrievalComplete,
                string room
            ) where T : PreParser;
        public void RetrievePrivateContent<T>(
            Action retrievalBeginning,
            Action<int, int> retrievalProceeding,
            Action<T> retrievalComplete,
            string author,
            string room
        ) where T : PreParser
        {
            this.Retrieve(retrievalBeginning, retrievalProceeding, retrievalComplete, string.Format("{0}/{1}", author, room));
        }

        public HistorySummary Describe(int id)
        {
            return HistorySummary.parse(resourceProvider.secureGetString(serverAddress.getSummary(id.ToString())));
        }
    }
    public class CachedHistoryProvider : BaseHistoryProvider
    {
        protected HttpHistoryProvider historyProvider;

        public CachedHistoryProvider(
                HttpHistoryProvider _historyProvider,
                HttpResourceProvider _resourceProvider,
                JabberWireFactory _jabberWireFactory,
                MetlConfiguration _serverAddress,
                IAuditor _auditor
        ) : base(_resourceProvider, _jabberWireFactory, _serverAddress, _auditor)
        {
            historyProvider = _historyProvider;
        }

        private Dictionary<string, PreParser> cache = new Dictionary<string, PreParser>();
        private int measure<T>(int acc, T item)
        {
            return acc + item.ToString().Length;
        }
        private long cacheTotalSize
        {
            get
            {
                return cache.Values.Aggregate(0, (acc, parser) =>
                    acc +
                        parser.ink.Aggregate(0, measure<TargettedStroke>) +
                        parser.images.Values.Aggregate(0, measure<TargettedImage>) +
                        parser.text.Values.Aggregate(0, measure<TargettedTextBox>));
            }
        }
        public override void Retrieve<T>(Action retrievalBeginning, Action<int, int> retrievalProceeding, Action<T> retrievalComplete, string room)
        {
            historyProvider.Retrieve<T>(
                null,
                null,
                history =>
                {
                    cache[room] = history;
                    retrievalComplete((T)cache[room]);
                },
                room);
        }

        //public void HandleMessage(int room, Element message, long timestamp) 
        public void HandleMessage(int room, MeTLStanzas.TimestampedMeTLElement element)
        {
            string currentSlide = room.ToString();
            if (!cache.ContainsKey(currentSlide))
            {
                cache[currentSlide] = jabberWireFactory.create<PreParser>(room);
            }
            cache[currentSlide].ActOnUntypedMessage(element);
        }
        public void PopulateFromHistory(PreParser preParser)
        {
            var room = preParser.location.currentSlide.ToString();
            if (cache.ContainsKey(room))
                cache[room] = cache[room].merge<PreParser>(preParser);
            else
                cache[room] = preParser;
        }

        public void ClearCache(string room)
        {
            if (cache.ContainsKey(room))
                cache.Remove(room);
        }

        public List<TargettedStroke> GetInks(string room)
        {
            if (!cache.ContainsKey(room))
                return new List<TargettedStroke>();

            return new List<TargettedStroke>(cache[room].ink);
        }

        public List<TargettedImage> GetImages(string room)
        {
            if (!cache.ContainsKey(room))
                return new List<TargettedImage>();

            return new List<TargettedImage>(cache[room].images.Values);
        }

        public List<TargettedTextBox> GetTexts(string room)
        {
            if (!cache.ContainsKey(room))
                return new List<TargettedTextBox>();

            return new List<TargettedTextBox>(cache[room].text.Values);
        }
    }
    public class HttpHistoryProvider : BaseHistoryProvider
    {
        public HttpHistoryProvider(
                HttpResourceProvider _resourceProvider,
                JabberWireFactory _jabberWireFactory,
                MetlConfiguration _serverAddress,
                IAuditor _auditor
        ) : base(_resourceProvider, _jabberWireFactory, _serverAddress, _auditor)
        {
        }
        public override void Retrieve<T>(Action retrievalBeginning, Action<int, int> retrievalProceeding, Action<T> retrievalComplete, string room)
        {
            var accumulatingParser = jabberWireFactory.create<T>(PreParser.ParentRoom(room));
            if (retrievalBeginning != null) retrievalBeginning();
            accumulatingParser.unOrderedMessages.Clear();
            var worker = new BackgroundWorker();
            var roomJid = room.Contains("/") ? room.Split('/').Reverse().Aggregate("", (acc, item) => acc + item) : room;
            worker.DoWork += (_sender, _args) =>
                {
                    auditor.wrapAction((g =>
                    {
                        var directoryUri = serverAddress.getRoomHistory(roomJid); 
                        var xmlString = resourceProvider.secureGetString(directoryUri);
                        using (var stream = GenerateStreamFromString(xmlString)) {
                            parseHistoryItem(stream, accumulatingParser);
                        }
                    }), "retrieveWorker: " + room.ToString(), "historyProvider");
                };
                if (retrievalComplete != null)
                    worker.RunWorkerCompleted += (_sender, _args) =>
                    {
                        try
                        {

                            accumulatingParser.ReceiveAndSortMessages();
                            retrievalComplete((T)accumulatingParser);
                        }
                        catch (Exception ex)
                        {
                            auditor.error("Retrieve: " + room, "PreParser", ex);
                        }
                    };
                worker.RunWorkerAsync(null);
            }
        protected readonly byte[] closeTag = Encoding.UTF8.GetBytes("</logCollection>");
        protected MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        protected virtual void parseHistoryItem(MemoryStream stream, JabberWire wire)
        {//This takes all the time
            var parser = new StreamParser();

            parser.OnStreamElement += ((_sender, node) =>
                                           {
                                               wire.unOrderedMessages.Add(wire.ContructElement(node));
                                           });

            parser.Push(stream.GetBuffer(), 0, (int)stream.Length);
            parser.Push(closeTag, 0, closeTag.Length);
        }
    }
}
