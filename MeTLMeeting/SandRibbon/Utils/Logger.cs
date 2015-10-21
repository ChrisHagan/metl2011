using System;
using SandRibbon.Providers;
using System.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Collections.ObjectModel;

namespace SandRibbon.Utils
{
    public class LogMessage
    {
        public string content { get; set; }
        public string user { get; set; }
        public int slide { get; set; }
        public long timestamp { get;set;}
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
        public static ObservableCollection<LogMessage> logs = new ObservableCollection<LogMessage>();

        private Logger()
        {
        }

        public static void Instantiate(string loggingServer)
        {
            Commands.MoveToCollaborationPage.RegisterCommand(new DelegateCommand<int>((where) => slide = where));
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>((who) => privacy = who));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>((_unused) => user = string.IsNullOrEmpty(Globals.me) ? unknownUser : Globals.me)); 
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
            logMessage(appendThis);          
        }
    
        private static void logMessage(string message)
        {
            if (String.IsNullOrEmpty(user)) return;
            if (String.IsNullOrEmpty(message)) return;            
            if (blacklist.Any(prefix => message.StartsWith(prefix))) return;

            var msg = new LogMessage
            {
                content = message,
                user = user,
                slide = slide,
                timestamp = DateTime.Now.Ticks                
            };

            logs.Add(msg);            
        }
    }
}
