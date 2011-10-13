using System.Windows.Automation;
using System.Threading;
using System.Collections.Generic;
using System;

namespace UITestFramework
{
    public class UITestHelper
    {
        public delegate bool Condition(AutomationElement element);
        
        private const int sleepIncrement = 100;
        private const int defaultTimeout = 30 * 1000;
        private AutomationElement parentElement;
        private List<AutomationElement> matchingElements = new List<AutomationElement>();
        private PropertyExpression searchProperties = new PropertyExpression();

        public UITestHelper()
        {
            parentElement = AutomationElement.RootElement;
        }
        public UITestHelper(AutomationElement parent)
        {
            if (parent != null)
            {
                parentElement = parent;
            }
            else
                throw new ArgumentNullException();
        }
    
        private AutomationElement FindFirstChildUsingAutomationId(string controlAutomationId)
        {
            return parentElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, controlAutomationId));
        }

        private TreeScope DetermineScopeFromParent()
        {
            return parentElement.Equals(AutomationElement.RootElement) ? TreeScope.Children : TreeScope.Descendants;
        }

        public void Find()
        {
            var element = parentElement.FindFirst(DetermineScopeFromParent(), new PropertyCondition(searchProperties.PropertyName, searchProperties.PropertyValue));
            matchingElements.Add(element);
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

        public PropertyExpression SearchProperties
        {
            get
            {
                return searchProperties;
            }
        }
    }
}
