using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using System.Threading;

namespace Functional
{
    public class UITestControl
    {
        private const int sleepIncrement = 100;
        private const int defaultTimeout = 30 * 1000;
        private AutomationElement desktop;

        public UITestControl()
        {
            desktop = AutomationElement.RootElement;
        }

        /// returns true if control is enabled before time-out; otherwise, false.
        public bool WaitForControlEnabled(string controlAutomationId)
        {
            int totalTime = 0;
            AutomationElement uiControl = null;

            do
            {
                uiControl = desktop.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, controlAutomationId));
                totalTime += sleepIncrement;
                Thread.Sleep(sleepIncrement);
            }
            while (uiControl == null && totalTime < defaultTimeout);

            return uiControl != null;
        }
    }
}
