using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;

namespace SandRibbon.Components.Sandpit
{
    public class Board {
        public string name{get;set;}
        public int x { get; set; }
        public int y { get; set; }
        public bool online { get; set; }
        public int slide { get; set; }
    }
    public class BoardManager
    {
        protected static MeTLLib.MetlConfiguration backend;
        public static double DISPLAY_WIDTH { get { return 80; } }
        public static double DISPLAY_HEIGHT { get { return 60; } }
        public static double AVATAR_HEIGHT { get { return 120; } }
        public static double AVATAR_WIDTH { get { return 60; } }
        static BoardManager() {
            App.getContextFor(backend).controller.commands.ReceivePong.RegisterCommand(new DelegateCommand<string>(ReceivePong));
        }
        public static void ReceivePong(string who){
            foreach(var room in boards.Values){
                foreach (var user in room.Where(b => b.name == who))
                    user.online = true;
            }
        }
        public static void ClearBoards(string room){
            foreach(var board in boards[room]){
                board.online = false;
            }
        }
        public static ConversationDetails DEFAULT_CONVERSATION
        {
            get { return ConversationDetails.Empty; }// App.getContextFor(backend).controller.client.DetailsOf("34000"); }
        }
        public static int slide(int index) {
            var slides = DEFAULT_CONVERSATION.Slides;
            return slides[index].id;
        }
        public static Dictionary<string, IEnumerable<Board>> boards = new Dictionary<string,IEnumerable<Board>>
        { 
        //This is hardcoded for the demo - get the real data from "http://metl.adm.monash.edu.au:1234/S15.xml"
            {"S15",new List<Board>{
                new Board{name="S15-1",x=40-20,y=100-10, slide=slide(0)},
                new Board{name="S15-2",x=40-20,y=200-10, slide=slide(1)},
                new Board{name="S15-3",x=260-20,y=200-10, slide=slide(2)},
                new Board{name="S15-4",x=260-20,y=100-10, slide=slide(3)},
                new Board{name="S15-5",x=150-20,y=280-10, slide=slide(4)}
                }
            }
        };
    }
}
