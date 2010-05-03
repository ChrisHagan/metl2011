using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers.Structure;
using SandRibbonInterop;
using SandRibbonObjects;

namespace SandRibbon.Quizzing
{
    public class PollMarker : Adorner
    {
        public readonly int ICON_RADIUS = 50;
        private UIElement adornee;
        private PollIcon control;
        private double x;
        private double y;
        public int target 
        { 
            get 
            { 
                return control.Quiz.targetSlide; 
            } 
        }
        
        public PollMarker(double x, double y, UIElement adornee, QuizDetails details) : base(adornee)
        {
            this.x = x;
            this.y = y;
            this.adornee = adornee;
            this.control = new PollIcon(details);
            AddVisualChild(control);
        }
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            control.Arrange(new Rect(new System.Windows.Point(x, y), control.DesiredSize));
            return finalSize;
        }
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return control;
        }
    }
    public class Quiz : Slide{
    }
    public class PollProvider
    {
        private string currentConversation;
        public PollProvider(){
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(title=>this.currentConversation = title));
        }
        public IEnumerable<Slide> ListPolls(string conversation){
            var slides = ConversationDetailsProviderFactory.Provider.DetailsOf(conversation).Slides;
            var polls = slides.Where(s=>s.type==Slide.TYPE.POLL);
            return polls.ToList();
        }
    }
    public class PollProviderFactory
    {
        private static object _instanceLock = new object();
        private static PollProvider _instance;
        public static PollProvider Provider
        {
            get
            {
                lock (_instanceLock)
                    if (_instance == null)
                        _instance = new PollProvider();
                return _instance;
            }
        }
    }
}