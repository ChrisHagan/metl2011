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
using Majestic12;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using Ionic.Zip;
using SandRibbonInterop.MeTLStanzas;
using agsXMPP.Xml.Dom;

namespace SandRibbon.Providers
{
    public class HistoryProviderFactory
    {
        private static object instanceLock = new object();
        private static IHistoryProvider instance;
        public static IHistoryProvider provider
        {
            get
            {
                lock (instanceLock)
                    if (instance == null)
                        instance = new CachedHistoryProvider();
                return instance;
            }
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
    }
    public abstract class BaseHistoryProvider : IHistoryProvider {
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
        public override void Retrieve<T>(Action retrievalBeginning, Action<int, int> retrievalProceeding, Action<T> retrievalComplete, string room)
        {
            if (!cache.ContainsKey(room))
            {
                new HttpHistoryProvider().Retrieve<PreParser>(
                    delegate { },
                    (_i, _j) => { },
                    history =>
                    {
                        if (cache.ContainsKey(room))
                            cache[room].merge<PreParser>(history);
                        else
                            cache[room] = history;
                        Commands.PreParserAvailable.Execute(history);
                    },
                    room);
            }
            else {
                retrievalComplete((T)cache[room]);
            }
        }
        public void HandleMessage(string to, Element message) {
            int room = 0;
            try
            {
                room = Int32.Parse(to);
            }
            catch (FormatException)
            {
                return;
            }
            if (!cache.ContainsKey(room.ToString()))
                cache[room.ToString()] = new PreParser(room);
            cache[room.ToString()].ActOnUntypedMessage(message);
        }
    }
    public class HttpHistoryProvider : BaseHistoryProvider
    {
        public override void Retrieve<T>(Action retrievalBeginning, Action<int,int> retrievalProceeding, Action<T> retrievalComplete, string room)
        {
            Logger.Log(string.Format("HttpHistoryProvider.Retrieve: Beginning retrieve for {0}", room));
            var accumulatingParser = (T)Activator.CreateInstance(typeof(T), PreParser.ParentRoom(room));
            if(retrievalBeginning != null)
                Application.Current.Dispatcher.BeginInvoke(retrievalBeginning);
            var worker = new BackgroundWorker();
            worker.DoWork += (_sender, _args) =>
                                 {
                                     var zipUri = string.Format("https://{0}:1749/{1}/all.zip", Constants.JabberWire.SERVER, room);
                                     try
                                     {
                                         var zipData = HttpResourceProvider.secureGetData(zipUri);
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
                                         MessageBox.Show("WE: " + e.Message);
                                         //Nothing to do if it's a 404.  There is no history to obtain.
                                     }
                                 };
            if (retrievalComplete != null)
                worker.RunWorkerCompleted += (_sender, _args) =>
                {
                    Commands.AllContentRetrieved.Execute(room);
                    Logger.Log(string.Format("{0} retrieval complete at historyProvider", room));
                    Application.Current.Dispatcher.Invoke(retrievalComplete, (T)accumulatingParser);
                };
            worker.RunWorkerAsync(null);
        }
        protected virtual void parseHistoryItem(string item, JabberWire wire)
        {//This takes all the time
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
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
