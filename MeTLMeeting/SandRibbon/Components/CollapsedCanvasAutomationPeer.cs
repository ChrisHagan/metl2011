using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using System.Windows.Automation.Provider;

namespace SandRibbon.Components
{
    public class CollapsedCanvasStackAutomationPeer : FrameworkElementAutomationPeer, IRawElementProviderSimple
    {
        public CollapsedCanvasStackAutomationPeer(FrameworkElement owner) : base(owner)
        {
            if (!(owner is CollapsedCanvasStack))
                throw new ArgumentOutOfRangeException();
        }

        protected override string GetClassNameCore()
        {
            return "CollapsedCanvasStack";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        object IRawElementProviderSimple.GetPatternProvider(int patternId)
        {
            return null;
        }
    }
}
