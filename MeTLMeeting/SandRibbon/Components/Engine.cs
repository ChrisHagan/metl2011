using SandRibbon.Pages;
using SandRibbon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SandRibbon.Components
{
    class GlobalPageEngine
    {
        public GlobalAwarePage page { get; protected set; }
        public GlobalPageEngine(GlobalAwarePage _page)
        {
            page = _page;
        }
    }
    class ServerPageEngine : GlobalPageEngine
    {
        public ServerAwarePage page { get; protected set; }
        public ServerPageEngine(ServerAwarePage _page) : base(_page)
        {
            page = _page;
        }
    }
    class ConversataionPageEngine :ServerPageEngine
    {
        public ConversationAwarePage page { get; protected set; }
        public ConversataionPageEngine(ConversationAwarePage _page) : base(_page)
        {
            page = _page;
        }
    }
    class SlidePageEngine : ConversataionPageEngine
    {
        public SlideAwarePage page { get; protected set; }
        public SlidePageEngine(SlideAwarePage _page) : base(_page)
        {
            page = _page;
        }
    }
}
