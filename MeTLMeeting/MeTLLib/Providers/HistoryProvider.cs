using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using agsXMPP.Xml;
using Ionic.Zip;
using agsXMPP.Xml.Dom;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using System.Diagnostics;
using Ninject;

namespace MeTLLib.Providers
{
    interface IHistoryProvider
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
    }
    public abstract class BaseHistoryProvider : IHistoryProvider {
        [Inject]public HttpHistoryProvider historyProvider{protected get;set;}
        [Inject]public HttpResourceProvider resourceProvider{protected get;set;}
        [Inject]public JabberWireFactory jabberWireFactory{protected get;set;}
        [Inject]public MeTLServerAddress serverAddress{protected get;set;}
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
        ) where T : PreParser {
            this.Retrieve(retrievalBeginning,retrievalProceeding,retrievalComplete,string.Format("{0}/{1}", author,room));
        }
    }
    public class CachedHistoryProvider : BaseHistoryProvider {
        private Dictionary<string, PreParser> cache = new Dictionary<string,PreParser>();
        private int measure<T>(int acc, T item){
            return acc + item.ToString().Length;
        }
        private long cacheTotalSize{
            get
            {
                return cache.Values.Aggregate(0, (acc, parser) => 
                    acc + 
                        parser.ink.Aggregate(0,measure<TargettedStroke>)+
                        parser.images.Values.Aggregate(0,measure<TargettedImage>)+
                        parser.text.Values.Aggregate(0,measure<TargettedTextBox>));
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
            //cache[currentSlide].ActOnUntypedMessage(message, timestamp);
            cache[currentSlide].ActOnUntypedMessage(element );
        }
        public void PopulateFromHistory(PreParser preParser)
        {
            var room = preParser.location.currentSlide.ToString();
            if (cache.ContainsKey(room))
                cache[room] =  cache[room].merge<PreParser>(preParser);
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
        public override void Retrieve<T>(Action retrievalBeginning, Action<int, int> retrievalProceeding, Action<T> retrievalComplete, string room)
        {
            var accumulatingParser = jabberWireFactory.create<T>(PreParser.ParentRoom(room));
            if (retrievalBeginning != null) retrievalBeginning();
            accumulatingParser.unOrderedMessages.Clear();
            var worker = new BackgroundWorker();
            worker.DoWork += (_sender, _args) =>
                                 {
                                     var zipUri = string.Format("https://{0}:1749/{1}/{2}/all.zip", serverAddress.host, INodeFix.Stem(room), room);
                                     try
                                     {
                                         var zipData = resourceProvider.secureGetData(new System.Uri(zipUri));
                                         if (zipData.Count() == 0)
                                         {
                                             return;
                                         }
                                         var zip = ZipFile.Read(zipData);
                                         var days = (from e in zip.Entries where e.FileName.EndsWith(".xml") orderby e.FileName select e).ToArray();
                                         for (int i = 0; i < days.Count(); i++)
                                         {
                                             using (var stream = new MemoryStream())
                                             {
                                                 days[i].Extract(stream);
                                                 parseHistoryItem(stream, accumulatingParser);
                                             }
                                             if (retrievalProceeding != null) retrievalProceeding(i, days.Count());
                                         }
                                     }
                                     catch (WebException e)
                                     {
                                         Trace.TraceWarning("HistoryProvider WebException in Retrieve: " + e.Message);
                                         //Nothing to do if it's a 404.  There is no history to obtain.
                                     }
                                 };
            if (retrievalComplete != null)
                worker.RunWorkerCompleted += (_sender, _args) =>
                {
                    try
                    {

                        accumulatingParser.ReceiveAndSortMessages();
                        /*sortedMessages = SortOnTimestamp(unSortedMessages);
                        foreach(Node message in sortedMessages)
                        {
                            accumulatingParser.ReceivedMessage(message, MessageOrigin.History);
                        }*/
                        retrievalComplete((T)accumulatingParser);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Exception on the retrievalComplete section: " + ex.Message.ToString());
                    }
                };
            worker.RunWorkerAsync(null);
        }
        protected readonly byte[] closeTag = Encoding.UTF8.GetBytes("</logCollection>");
        protected virtual void parseHistoryItem(MemoryStream stream, JabberWire wire)
        {//This takes all the time
            var parser = new StreamParser();

            parser.OnStreamElement += ((_sender, node) =>
                                           {
                                               wire.unOrderedMessages.Add(wire.ContructElement(node));
                                               //unSortedMessages.Add(node);
                                               //wire.ReceivedMessage(node, MessageOrigin.History);
                                           });

            parser.Push(stream.GetBuffer(), 0, (int)stream.Length);
            parser.Push(closeTag, 0, closeTag.Length);
        }
    }   
}
