using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SandRibbon.Utils
{
    public class Stopwatch
    {
        private DateTime start = DateTime.Now;
        public Stopwatch() {
            Console.WriteLine("Timer initialized");
        }
        public void mark(string content) { 
            Console.WriteLine(string.Format("{0}:{1}",content,DateTime.Now - start));
        }
    }
}
