using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Utils;
using System.Windows.Ink;

namespace SandRibbon.Components
{
    public partial class CommandBridge : UserControl
    {
        public CommandBridge()
        {
            InitializeComponent();
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CommandBridgeAutomationPeer(this);
        }
    }
    class CommandBridgeAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public CommandBridgeAutomationPeer(CommandBridge parent) : base(parent) { }
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;
            return base.GetPattern(patternInterface);
        }
        private CommandBridge Window1
        {
            get { return (CommandBridge)base.Owner; }
        }
        protected override string GetAutomationIdCore()
        {
            return "commandBridge";
        }
        public void SetValue(string value)
        {
            try
            {
                if (String.IsNullOrEmpty(value) || !(value.Contains(':'))) return;
                var parts = value.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                switch (parts[0])
                {
                    case "SetDrawingAttributes":
                        //This should come in the format of "SetDrawingAttributes:FF FF FF FF:15:false"
                        if (parts.Count() < 4 || String.IsNullOrEmpty(parts[2]) || String.IsNullOrEmpty(parts[3]) || !(parts[1].Contains(' '))) return;
                        var ARGBvalues = parts[1].Split(new[] { ' ' });
                        if (!(ARGBvalues.Count() == 4)) return;
                        AppCommands.SetLayer.Execute("sketch");
                        var color = Color.FromArgb(Byte.Parse(ARGBvalues[0]), Byte.Parse(ARGBvalues[1]), Byte.Parse(ARGBvalues[2]), Byte.Parse(ARGBvalues[3]));
                        var da = new DrawingAttributes { Color = color, Height = Double.Parse(parts[2]), Width = Double.Parse(parts[2]), IsHighlighter = Boolean.Parse(parts[3]) };
                        AppCommands.SetDrawingAttributes.Execute(da);
                        break;
                    case "SetInkCanvasMode":
                        //This should come in the format of "SetInkCanvas:Ink"
                        if (parts.Count() < 2 || String.IsNullOrEmpty(parts[1]) || !(new []{"Ink","EraseByStroke","Select","None"}.Contains(parts[1]))) return;
                        AppCommands.SetInkCanvasMode.Execute(parts[1]);
                        break;                    
                    case "SetLayer":
                        //This should come in the format of "SetLayer:Sketch"
                        if (parts.Count() < 2 || String.IsNullOrEmpty(parts[1]) || !(new []{"Sketch","Text","Insert","View"}.Contains(parts[1]))) return;
                        AppCommands.SetLayer.Execute(parts[1]);
                        break;
                }
            }
            catch (Exception e)
            {
                AppCommands.Mark.Execute(e.Message);
            }
        }
        bool IValueProvider.IsReadOnly
        {
            get { return false; }
        }
        string IValueProvider.Value
        {
            get
            {
                return "cannot return state of the commandBridge: commandBridge is one-way";
            }
        }
    }
}
