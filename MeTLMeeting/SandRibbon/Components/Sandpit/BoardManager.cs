using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandRibbon.Providers;
using SandRibbonObjects;
using SandRibbon.Providers.Structure;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components.Sandpit
{
    public class Board {
        public string name{get;set;}
        public int x { get; set; }
        public int y { get; set; }
        public bool online { get; set; }
    }
    public class BoardManager
    {
        public static double DISPLAY_WIDTH { get { return 130; } }
        public static double DISPLAY_HEIGHT { get { return 100; } }
        static BoardManager() {
            Commands.ReceivePong.RegisterCommand(new DelegateCommand<string>(ReceivePong));
        }
        public static void ReceivePong(string who){
            foreach(var room in boards.Values){
                foreach (var user in room.Where(b => b.name == who))
                    user.online = true;
            }
        }
        public static ConversationDetails DEFAULT_CONVERSATION
        {
            get { return ConversationDetailsProviderFactory.Provider.DetailsOf("20400"); }
        }
        public static Dictionary<string, IEnumerable<Board>> boards = new Dictionary<string,IEnumerable<Board>>
        { 
        //This is hardcoded for the demo - get the real data from "http://metl.adm.monash.edu.au:1234/S15.xml"
            {"S15",new List<Board>{
                new Board{name="S15-1",x=44-20,y=138-10},
                new Board{name="S15-2",x=51-20,y=225-10},
                new Board{name="S15-3",x=274-20,y=287-10},
                new Board{name="S15-4",x=338-20,y=109-10},
                new Board{name="S15-5",x=199-20,y=24-10}
                }
            }
        };
    }
}
