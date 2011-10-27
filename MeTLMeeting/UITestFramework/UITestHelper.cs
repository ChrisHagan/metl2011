using System.Windows.Automation;
using System.Threading;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UITestFramework
{
    public class UITestHelper
    {
        public delegate bool WaitCondition(AutomationElement element);
        
        private const int sleepIncrement = 100;
        private const int defaultTimeout = 30 * 1000;
        private AutomationElement parentElement;
        private AutomationElement matchingElement;
        private List<PropertyExpression> searchProperties = new List<PropertyExpression>();

        #region Constructors
        public UITestHelper()
        {
            parentElement = AutomationElement.RootElement;
        }
        public UITestHelper(AutomationElement parent)
        {
            if (parent == null)
                throw new ArgumentNullException();

            parentElement = parent;
        }
        public UITestHelper(UITestHelper parent)
        {
            if (parent == null)
                throw new ArgumentNullException();

            parentElement = parent.AutomationElement;
        }
        #endregion

        #region Helpers
        private TreeScope DetermineScopeFromParent()
        {
            return parentElement.Equals(AutomationElement.RootElement) ? TreeScope.Children : TreeScope.Element | TreeScope.Descendants;
        }

        private Condition GetPropertyConditions()
        {
            Condition properties = null;
            if (searchProperties.Count > 1)
            {
                var propertyList = new List<Condition>();
                foreach (var property in searchProperties)
                {
                    propertyList.Add(new PropertyCondition(property.PropertyName, property.PropertyValue));
                }
                properties = new AndCondition(propertyList.ToArray());
            }
            else
            {
                properties = new PropertyCondition(searchProperties[0].PropertyName, searchProperties[0].PropertyValue); 
            }

            return properties;
        }
        #endregion

        public void Find()
        {
            Assert.IsTrue(searchProperties.Count > 0, "SearchProperties must be set before calling WaitForControl functions");

            matchingElement = parentElement.FindFirst(DetermineScopeFromParent(), GetPropertyConditions());
        }

        #region WaitForControl functions
        public bool WaitForControl(WaitCondition loopCondition, WaitCondition returnCondition)
        {
            int totalTime = 0;
            AutomationElement uiControl = null;

            try
            {
                do
                {
                    Find();
                    uiControl = matchingElement;

                    totalTime += sleepIncrement;
                    Thread.Sleep(sleepIncrement);
                }
                while (loopCondition(uiControl) && totalTime < defaultTimeout);
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }

            return returnCondition(uiControl);
        }

        /// returns true if control is enabled before time-out; otherwise, false.
        public bool WaitForControlEnabled()
        {
            WaitCondition loopCondition = (uiControl) =>
            {
                return !(uiControl != null && (bool)uiControl.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty) == true);
            };

            WaitCondition returnCondition = (uiControl) =>
            {
                return uiControl != null;
            };

            return WaitForControl(loopCondition, returnCondition);
        }

        /// returns true if control is not found before time-out; otherwise, false.
        public bool WaitForControlNotExist()
        {
            WaitCondition loopCondition = (uiControl) =>
            {
                return uiControl != null;                
            };

            WaitCondition returnCondition = (uiControl) =>
            {
                return uiControl == null;
            };

            return WaitForControl(loopCondition, returnCondition); 
        }

        /// <summary>
        ///  returns true if control exists before time-out; otherwise, false.
        /// </summary>
        public bool WaitForControlExist()
        {
            WaitCondition loopCondition = (uiControl) =>
            {
                return uiControl == null;
            };

            WaitCondition returnCondition = (uiControl) =>
            {
                return uiControl != null;
            };

            return WaitForControl(loopCondition, returnCondition);
        }

        /// <summary>
        /// returns true if control meets specified condition before time-out; otherwise, false.
        /// </summary>
        public bool WaitForControlCondition(WaitCondition condition)
        {
            WaitCondition returnCondition = (uiControl) =>
            {
                return uiControl != null;
            };
            
            return WaitForControl(condition, returnCondition);
        }
        #endregion

        #region Properties
        public List<PropertyExpression> SearchProperties
        {
            get
            {
                return searchProperties;
            }
        }

        public AutomationElement AutomationElement
        {
            get
            {
                try
                {
                    return matchingElement;
                }
                catch (ElementNotAvailableException)
                {
                }

                return null;
            }

            set
            {
                matchingElement = value;
            }
        }

        public string Value
        {
            get
            {
                return AutomationElement.Value();
            }

            set
            {
                AutomationElement.Value(value);
            }
        }
        #endregion
    }
}
