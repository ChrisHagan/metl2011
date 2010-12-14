using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace scratchpad
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<string> { "this", "that" };
            var objectEnumerable = (IEnumerable<object>)list;
            var objectList = objectEnumerable.ToList();
            objectList.Add(3);
        }
    }
}
