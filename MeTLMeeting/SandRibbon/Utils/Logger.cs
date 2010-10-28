using System;
using System.IO;
using System.Diagnostics;

namespace SandRibbon.Utils
{
    public class Logger
    {
        private static string logDump = "crash.log";
        private static object logLock = new object();
        public static string log = "MeTL Log\r\n";
        public static void Log(string appendThis)
        {/*Interesting quirk about the formatting: \n is the windows line ending but ruby assumes
          *nix endings, which are \r.  Safest to use both, I guess.*/
            var now = SandRibbonObjects.DateTimeFactory.Now();
            Trace.TraceInformation("{0}{1}.{2}: {3}\r\n", log, now, now.Millisecond, appendThis); 
        }
        public static void Log(string format, params object[] args)
        {
            var s = string.Format(format, args);
            Log(s);
        }
    }
}
