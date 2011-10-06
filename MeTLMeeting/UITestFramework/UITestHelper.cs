using System.Windows.Automation;
using System.Threading;

namespace UITestFramework
{
    public class UITestHelper
    {
        public delegate bool Condition(AutomationElement element);
        
        private const int sleepIncrement = 100;
        private const int defaultTimeout = 30 * 1000;
        private AutomationElement desktop;

        public UITestHelper()
        {
            desktop = AutomationElement.RootElement;
        }

        private AutomationElement FindFirstChildUsingAutomationId(string controlAutomationId)
        {
            return desktop.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, controlAutomationId));
        }

        public bool WaitForControl(string controlAutomationId, Condition loopCondition, Condition returnCondition)
        {
            int totalTime = 0;
            AutomationElement uiControl = null;

            do
            {
                uiControl = FindFirstChildUsingAutomationId(controlAutomationId);
                totalTime += sleepIncrement;
                Thread.Sleep(sleepIncrement);
            }
            while (loopCondition(uiControl) && totalTime < defaultTimeout);

            return returnCondition(uiControl);
        }

        /// returns true if control is enabled before time-out; otherwise, false.
        public bool WaitForControlEnabled(string controlAutomationId)
        {
            Condition loopCondition = (uiControl) =>
            {
                return uiControl == null || (bool)uiControl.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty) == false;
            };

            Condition returnCondition = (uiControl) =>
            {
                return uiControl != null;
            };

            return WaitForControl(controlAutomationId, loopCondition, returnCondition);
        }

        /// returns true if control is not found before time-out; otherwise, false.
        public bool WaitForControlNotExist(string controlAutomationId)
        {
            Condition loopCondition = (uiControl) =>
            {
                return uiControl != null;                
            };

            Condition returnCondition = (uiControl) =>
            {
                return uiControl == null;
            };

            return WaitForControl(controlAutomationId, loopCondition, returnCondition); 
        }
    }
}
