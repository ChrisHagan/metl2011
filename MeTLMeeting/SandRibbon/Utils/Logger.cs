using System;
using SandRibbon.Providers;
using MeTLLib;
using System.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace SandRibbon.Utils
{
    class LogQueue
    {
        public static Uri LoggingServer { get; private set; }
        public static string LoggingServerString { get; private set; }

        readonly object lockObj = new object();
        Thread[] workers;
        WebClient server;
        Queue<LogMessage> logMessages = new Queue<LogMessage>();

        public LogQueue(string loggingServer, int workerCount)
        {
            LoggingServer = new Uri(loggingServer);
            LoggingServerString = loggingServer;

            server = new WebClient();
            workers = new Thread[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                (workers[i] = new Thread(PostMessage)).Start();
            }
        }

        public void Shutdown(bool waitForWorkers)
        {
            // Enqueue a null item to make each exit
            foreach (Thread worker in workers)
            {
                EnqueueLog(null);
            }

            // Wait for all the workers to finish
            if (waitForWorkers)
            {
                foreach (Thread worker in workers)
                {
                    worker.Join();
                }
            }
            server.Dispose();
        }

        public void EnqueueLog(LogMessage log)
        {
            lock (lockObj)
            {
                logMessages.Enqueue(log);
                Monitor.Pulse(lockObj);     // pulsing because we're changing a blocking condition
            }
        }

        void PostMessage()
        {
            while (true)
            {
                LogMessage log;
                lock (lockObj)
                {
                    while (logMessages.Count == 0)
                        Monitor.Wait(lockObj);

                    log = logMessages.Dequeue();
                }

                if (log == null)
                    return;

                // send off the message to the server
                try
                {
                    server.QueryString = log.BuildQueryString();
                    server.DownloadString(LoggingServer);
                }
                catch (WebException)
                {
                    // something horrible happened with the logging server
                }
            }
        }
    }

    class LogMessage
    {
        public string content;
        public string user;
        public string server;
        public int slide;

        public NameValueCollection BuildQueryString()
        {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (App.isStaging)
                queryString["program"] = "metlStaging";
            else
                queryString["program"] = "metl2011";
            queryString["version"] = ConfigurationProvider.instance.getMetlVersion();
            queryString["content"] = content;
            queryString["user"] = user;
            queryString["collaborationLevel"] = Globals.conversationDetails != null ? Globals.conversationDetails.Permissions.studentCanPublish ? "Enabled" : "Disabled" : "None";
            queryString["server"] = server;
            queryString["slide"] = Convert.ToString(slide);

            return queryString;
        }
    }

    public class Logger
    {
        public static string log = "MeTL Log\r\n";
        private static string[] blacklist = new[] {
                "CouchServer(madam.adm.monash.edu.au:5984)",
                "MeTL Presenter.exe ", 
                "MeTL Presenter.vshost.exe ", 
                "Failed to add item to relogin-queue.", 
                "MeTL Presenter.exe Warning: 0 :", 
                "MeTL Presenter.exe Info: 0 :", 
                "MeTL Presenter.exe Information: 0 :", 
                "MeTL Staging.vshost.exe Information: 0 : ",
                "MeTL Staging.exe Information: 0 : ",
                "Error loading thumbnail:"};
        private static int slide = -1;
        private static string privacy = "Not set";
        private static readonly string unknownUser = "UNKNOWN";
        private static string user = unknownUser;
        private static LogQueue logQueue;

        private Logger()
        {
        }

        public static void Instantiate(string loggingServer)
        {
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((where) => slide = where));
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>((who) => privacy = who));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>((_unused) => user = string.IsNullOrEmpty(Globals.me) ? unknownUser : Globals.me));
            Commands.LeaveAllRooms.RegisterCommand(new DelegateCommand<object>((_unused) => CleanupLogQueue()));
            Commands.CloseApplication.RegisterCommand(new DelegateCommand<object>((_unused) => CleanupLogQueue()));

            logQueue = new LogQueue(loggingServer, workerCount: 1);
        }

        public static void CleanupLogQueue()
        {
            // just to make sure the threads are cleaned up
            if (logQueue != null)
            {
                logQueue.Shutdown(true);
            }
        }

        public static void Crash(Exception e)
        {
            var crashMessage = string.Format("CRASH: {0} @ {1} INNER: {2}",
                e.Message,
                e.StackTrace,
                e.InnerException == null ? "NONE" : e.InnerException.StackTrace);
            Log(crashMessage);
        }

        public static void Fixed(string message)
        {
            try
            {
                Log(string.Format("CRASH: (fixed): {0} {1}", user, message));
            }
            catch (NotSetException)
            {
                Log(string.Format("CRASH: (fixed): {0} {1}", "USERNAME_NOT_SET", message));
            }
        }

        public static void Log(string appendThis)
        {
            // JJ: Disabled logging for the MeTL Resiliency Project
            /*if (!App.isStaging)
            {
                logMessage(appendThis);
            }*/
        }

        private static void logMessage(string message)
        {
            if (String.IsNullOrEmpty(user)) return;
            if (String.IsNullOrEmpty(message)) return;
            if (message.Contains(LogQueue.LoggingServerString)) return;
            if (blacklist.Any(prefix => message.StartsWith(prefix))) return;

            var msg = new LogMessage
            {
                content = message,
                user = user,
                slide = slide,
                server = App.controller.config.name
            };

            logQueue.EnqueueLog(msg);
        }
    }
}
