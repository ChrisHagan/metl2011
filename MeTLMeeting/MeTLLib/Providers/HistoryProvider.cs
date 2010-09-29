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
            this.Retrieve(retrievalBeginning,retrievalProceeding,retrievalComplete,string.Format("{1}{0}", author,room));
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
        private bool isPrivateRoom(string room)
        {
            var validChar = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
            if (!room.All(s=>(validChar.Contains(s)))) return true;
            return false;
            
        }
        public void HandleMessage(string to, Element message) {
            if(isPrivateRoom(to)) return;
            var room = Int32.Parse(to);
            if (!cache.ContainsKey(room.ToString()))
                cache[room.ToString()] = jabberWireFactory.preParser(room);
            cache[room.ToString()].ActOnUntypedMessage(message);
        }
    }
    public class HttpHistoryProvider : BaseHistoryProvider
    {
        public override void Retrieve<T>(Action retrievalBeginning, Action<int,int> retrievalProceeding, Action<T> retrievalComplete, string room)
        {
            Trace.TraceInformation(string.Format("HttpHistoryProvider.Retrieve: Beginning retrieve for {0}", room));
            var accumulatingParser = jabberWireFactory.create<T>(PreParser.ParentRoom(room));
            if(retrievalBeginning != null)
                Application.Current.Dispatcher.adoptAsync(retrievalBeginning);
            var worker = new BackgroundWorker();
            worker.DoWork += (_sender, _args) =>
                                 {
                                     var zipUri = string.Format("https://{0}:1749/{1}/all.zip", serverAddress.uri.Host, room);
                                     try
                                     {
                                         var zipData = resourceProvider.secureGetData(zipUri);
                                         if (zipData.Count() == 0) return;
                                         var zip = ZipFile.Read(zipData);
                                         var days = (from e in zip.Entries where e.FileName.EndsWith(".xml") orderby e.FileName select e).ToArray();
                                         for (int i = 0; i < days.Count(); i++)
                                         {
                                             using(var stream = new MemoryStream())
                                             {
                                                 days[i].Extract(stream);
                                                 var historicalDay = Encoding.UTF8.GetString(stream.ToArray());
                                                 parseHistoryItem(historicalDay, accumulatingParser);
                                             }
                                             if (retrievalProceeding != null)
                                                 Application.Current.Dispatcher.BeginInvoke(retrievalProceeding, i, days.Count());
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
                    Trace.TraceInformation(string.Format("{0} retrieval complete at historyProvider", room));
                    try
                    {
                        Application.Current.Dispatcher.Invoke(retrievalComplete, (T)accumulatingParser);
                    }
                    catch (Exception ex) {
                        Trace.TraceError("Exception on the retrievalComplete section: "+ex.Message.ToString()); 
                    }
                    };
            worker.RunWorkerAsync(null);
        }
        protected virtual void parseHistoryItem(string item, JabberWire wire)
        {//This takes all the time
            Application.Current.Dispatcher.adoptAsync((Action)delegate
            {//Creating and event setting on the dispatcher thread?  Might be expensive, might not.  Needs bench.
                var history = item + "</logCollection>";
                var parser = new StreamParser();
                parser.OnStreamElement += ((_sender, node) =>
                                               {
                                                   wire.ReceivedMessage(node);
                                               });
                parser.Push(Encoding.UTF8.GetBytes(history), 0, history.Length);
            });
        }
    }
}
