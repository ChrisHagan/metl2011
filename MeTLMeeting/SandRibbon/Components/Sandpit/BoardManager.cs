using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SandRibbon.Components.Sandpit
{
    public class BoardManager
    {
        public static Dictionary<string, IEnumerable<string>> boards = new Dictionary<string,IEnumerable<string>>
        { 
            {"S15",Enumerable.Range(1,7).Select(i=>string.Format("S15{0}",i)).ToList()}
        };
    }
}
