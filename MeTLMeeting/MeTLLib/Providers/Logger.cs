using System;
using System.IO;

namespace MeTLLib.Providers
{
    class Logger
    {
        private static string logDump = "crash.log";
        private static object logLock = new object();
        public static string log = "MeTL Log\r\n";
        public static void Log(string appendThis)
        {/*Interesting quirk about the formatting: \n is the windows line ending but ruby assumes
          *nix endings, which are \r.  Safest to use both, I guess.*/
            var now = DateTimeFactory.Now();
            string appendingText = now.ToString() +"."+ now.Millisecond.ToString() +": "+ appendThis;
            lock(Logger.logLock)
                log = string.Format("{0}{1}\r\n", log, appendingText);
            Commands.ReceiveLogMessage.Execute(appendingText);
        }
        public static void Log(string format, params object[] args)
        {
            var s = string.Format(format, args);
            Log(s);
        }
        public static void Dump()
        {
            try
            {
                using (var stream = new FileStream(logDump, FileMode.Append, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(log);
            }
            catch (Exception)
            {
                //Mm.
            }
        }
    }
}
