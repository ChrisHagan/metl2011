using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;
//using Divan;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

namespace PowerpointJabber
{
    public class CouchTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Logger.Log(message);
        }
        public override void WriteLine(string message)
        {
            Logger.Log(message);
        }
    }
    public class LogMessage //: CouchDocument
    {
        public string version;
        public string content;
        public long timestamp;
        public string user;
        public string server;
        public int slide;
        /*public override void WriteJson(JsonWriter writer)
        {
            base.WriteJson(writer);
            writer.WritePropertyName("version");
            writer.WriteValue(ThisAddIn.version);
            writer.WritePropertyName("docType");
            writer.WriteValue("log");
            writer.WritePropertyName("content");
            writer.WriteValue(content);
            writer.WritePropertyName("timestamp");
            writer.WriteValue(timestamp);
            writer.WritePropertyName("user");
            writer.WriteValue(user);
            writer.WritePropertyName("server");
            writer.WriteValue(server);
            writer.WritePropertyName("slide");
            writer.WriteValue(slide);
        }
        public override void ReadJson(JObject obj)
        {
            base.ReadJson(obj);
            content = obj["message"].Value<string>();
            version = obj["version"].Value<string>();
            timestamp = obj["timestamp"].Value<long>();
            user = obj["user"].Value<string>();
            server = obj["server"].Value<string>();
            slide = obj["slide"].Value<int>();
        }*/
    }
    public class Logger
    {
        public static string log = "SimplePens Log\r\n";
        public static readonly string POST_LOG = "http://madam.adm.monash.edu.au:5984/simplepens_log";
    //    private static CouchServer server = new CouchServer("madam.adm.monash.edu.au", 5984);
        private static readonly string DB_NAME = "simplepens_log";
    //    private static ICouchDatabase db;
        private static string[] LogExceptions = new[] {
                "powerpnt.exe Information: 0 :"};
        public static void StartLogger()
        {
            try
            {
                checkDb();
            }
            catch (Exception) { }
            safetyQueue();
            Trace.Listeners.Add(new CouchTraceListener());
        }
        private static System.Threading.Timer backgroundUploadTimer;
        private static void renewTimer()
        {
            if (backgroundUploadTimer == null)
                backgroundUploadTimer = new System.Threading.Timer((state) =>
                    {
                        safetyQueue();
                    });
            backgroundUploadTimer.Change(60000, 60000);
        }
        private static void checkDb()
        {
//            if (db == null)
  //              db = server.GetDatabase(DB_NAME);
        }
        public static void Info(string message)
        {
            try
            {
                Trace.TraceInformation(message);
            }
            catch (Exception) { }
        }
        public static void Info(string message, params string[] args)
        {
            try
            {
                Trace.TraceInformation(message, args);
            }
            catch (Exception) { }
        }
        public static void Crash(Exception e)
        {
            var crashMessage = string.Format("CRASH: {0} @ {1} INNER: {2}",
                e.Message,
                e.StackTrace,
                e.InnerException == null ? "NONE" : e.InnerException.StackTrace);
            Log(crashMessage);
        }
        public static void Log(string appendThis)
        {
            var now = DateTime.Now;
            putCouch(appendThis, now);
        }
        private static void putCouch(LogMessage message)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    checkDb();
    //                db.SaveArbitraryDocument<LogMessage>(message);
                }
                catch (Exception e)
                {
                    saveToQueue(message);
                }
            });
        }
        private static void putCouch(string message, DateTime now)
        {
            var name = (ThisAddIn.instance == null
                || ThisAddIn.instance.Application == null
                || String.IsNullOrEmpty(ThisAddIn.instance.Application.Name)) ? "unknown" : ThisAddIn.instance.Application.Name;
            int currentPosition = (ThisAddIn.instance == null
                || ThisAddIn.instance.Application == null
                || String.IsNullOrEmpty(ThisAddIn.instance.Application.Name)
                || ThisAddIn.instance.Application.ActivePresentation == null
                || ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow == null
                || ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View == null) ? 0 : ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.CurrentShowPosition;
            if (String.IsNullOrEmpty(message)) return;
            if (message.Contains(POST_LOG)) return;
            if (LogExceptions.Any(prefix => message.StartsWith(prefix))) return;
            var msg = new LogMessage
            {
                content = message,
                timestamp = now.Ticks,
                user = name,
                slide = currentPosition,
                server = "noServer - SimplePens"
            };
            putCouch(msg);
        }
        private static List<LogMessage> MessageQueue = new List<LogMessage>();
        private static void saveToQueue(LogMessage message)
        {
            MessageQueue.Add(message);
            renewTimer();
        }
        private static void safetyQueue()
        {
            readMessageQueueFromDisk();
            tryToResendMessageQueue();
            saveRemainingMessageQueueToDisk();
            if (MessageQueue.Count == 0 && backgroundUploadTimer != null)
                backgroundUploadTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private static readonly string backupFile = "cachedLogMessages.xml";
        private static XmlSerializer serializer = new XmlSerializerFactory().CreateSerializer(typeof(List<LogMessage>));
        private static void readMessageQueueFromDisk()
        {
            try
            {
                using (var file = File.Open(backupFile, FileMode.Open))
                {
                    List<LogMessage> tempQueue = (List<LogMessage>)serializer.Deserialize(file);
                    foreach (LogMessage msg in tempQueue)
                        MessageQueue.Add(msg);
                }
            }
            catch (Exception) { }
        }
        private static void tryToResendMessageQueue()
        {
//            if (db != null)
//            {
                List<LogMessage> tempQueue = new List<LogMessage>();
                while (MessageQueue.Count > 0)
                {
                    tempQueue.Add((LogMessage)MessageQueue[0]);
                    MessageQueue.RemoveAt(0);
                }
                foreach (LogMessage msg in tempQueue)
                    putCouch(msg);
//            }
        }
        private static void saveRemainingMessageQueueToDisk()
        {
            try
            {
                File.Delete(backupFile);
                using (var file = File.Open(backupFile, FileMode.OpenOrCreate))
                {
                    List<LogMessage> tempQueue = new List<LogMessage>();
                    while (MessageQueue.Count > 0)
                    {
                        tempQueue.Add((LogMessage)MessageQueue[0]);
                        MessageQueue.RemoveAt(0);
                    }
                    serializer.Serialize(file, tempQueue);
                }
            }
            catch (Exception) { }
        }
    }
}