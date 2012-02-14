using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using System.Windows.Automation.Provider;

namespace SandRibbon.Components
{
    public class CollapsedCanvasStackAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public CollapsedCanvasStackAutomationPeer(CollapsedCanvasStack owner) : base(owner)
        {
            if (!(owner is CollapsedCanvasStack))
                throw new ArgumentOutOfRangeException();
        }

        private CollapsedCanvasStack CanvasStack
        {
            get { return base.Owner as CollapsedCanvasStack; }
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return "CollapsedCanvasStack";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
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
                        break;
                    case "SetInkCanvasMode":
                        //This should come in the format of "SetInkCanvas:Ink"
                        if (parts.Count() < 2 || String.IsNullOrEmpty(parts[1]) || !(new []{"Ink","EraseByStroke","Select","None"}.Contains(parts[1]))) return;
                        Commands.SetInkCanvasMode.Execute(parts[1]);
                        break;                    
                    case "SetLayer":
                        //This should come in the format of "SetLayer:Sketch"
                        if (parts.Count() < 2 || String.IsNullOrEmpty(parts[1]) || !(new []{"Sketch","Text","Insert","View"}.Contains(parts[1]))) return;
                        Commands.SetLayer.Execute(parts[1]);
                        break;
                }
            }
            catch (Exception)
            {
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
                return CanvasStack.Work.Strokes.Count.ToString();
            }
        }
    }
}
