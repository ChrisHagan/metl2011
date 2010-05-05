using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandRibbon.Providers;

namespace SandRibbon.Components.Sandpit
{
    public class Board {
        public string name{get;set;}
        public int x { get; set; }
        public int y { get; set; }
    }
    public class BoardManager
    {
        public static Dictionary<string, IEnumerable<Board>> boards = new Dictionary<string,IEnumerable<Board>>
        { 
        //This is hardcoded for the demo - get the real data from "http://metl.adm.monash.edu.au:1234/S15.xml"
            {"S15",new List<Board>{
                new Board{name="S151",x=44-20,y=138-10},
                new Board{name="S152",x=51-20,y=225-10},
                new Board{name="S153",x=274-20,y=287-10},
                new Board{name="S154",x=338-20,y=109-10},
                new Board{name="S155",x=199-20,y=24-10},
                new Board{name="S15Teacher",x=149-45,y=374-10}
                }
            }
        };
    }

}
