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

        public static UITestHelper RootElement = new UITestHelper();
        public static int sleepMultiplier = 1;
        
        private const int sleepIncrement = 100;
        private const int defaultTimeout = 30 * 1000;
        private int? customTimeout = null;
        private AutomationElement parentElement;
        private AutomationElement matchingElement;
        private List<PropertyExpression> searchProperties = new List<PropertyExpression>();

        #region Constructors
        public UITestHelper()
        {
            matchingElement = parentElement = AutomationElement.RootElement;
        }
        
        public UITestHelper(AutomationElement parent, AutomationElement child) : this(parent)
        {
            matchingElement = child; 
        }

        public UITestHelper(UITestHelper parent, AutomationElement child) : this(parent)
        {
            matchingElement = child;
        }

        public UITestHelper(AutomationElement parent)
        {
            if (parent == null)
                parent = AutomationElement.RootElement;

            parentElement = parent;
        }

        public UITestHelper(UITestHelper parent)
        {
            if (parent == null)
                parent = new UITestHelper(AutomationElement.RootElement);

            parentElement = parent.AutomationElement;
        }
        #endregion

        #region Helpers
        private static WaitCondition elementIsValid = (uiControl) =>
        {
            return ElementIsCurrent(uiControl);
        };

        private static WaitCondition elementIsInvalid = (uiControl) =>
        {
            return !ElementIsCurrent(uiControl);
        };

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

        private void DetermineMatchingElement()
        {
            if (matchingElement == null)
                Find(); 
            else
            {
                if (!ElementIsCurrent(matchingElement))
                    Find();
            }
        }

        private static bool ElementIsCurrent(AutomationElement element)
        {
            bool current = true;
            try
            {
                // find out whether the element is available by trying to retrieve a current property
                // hacky but works. it would be preferable if there was a specific property on the AutomationElement to check
                var unused = element != null ? element.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty, true) : null;
            }
            // used to be ElementNotAvailableException but depending on the lifecycle of the element it may throw a different exception
            catch (Exception) 
            {
                // handle ElementNotAvailableException, InvalidOperationException, COMException separately?
                current = false;
            }

            return element != null && current;
        }

        #endregion // Helpers

        public void Find()
        {
            Assert.IsTrue(searchProperties.Count > 0, "SearchProperties must be set before calling WaitForControl functions");

            matchingElement = parentElement.FindFirst(DetermineScopeFromParent(), GetPropertyConditions());
        }

        public static void Wait(TimeSpan time)
        {
            Thread.Sleep((int)time.TotalMilliseconds * sleepMultiplier);
        }

        private int TimeoutValue()
        {
            if (customTimeout.HasValue)
                return customTimeout.Value;

            return defaultTimeout;
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
                    DetermineMatchingElement();
                    uiControl = matchingElement;

                    totalTime += sleepIncrement;
                    Thread.Sleep(sleepIncrement);
                }
                while (loopCondition(uiControl) && totalTime < TimeoutValue());
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }

            return returnCondition(uiControl) && totalTime < TimeoutValue();
        }

        /// returns true if control is enabled before time-out; otherwise, false.
        public bool WaitForControlEnabled()
        {
            WaitCondition loopCondition = (uiControl) =>
            {
                return !(uiControl != null && (bool)uiControl.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty) == true);
            };

            return WaitForControl(loopCondition, elementIsValid);
        }

        /// returns true if control is completely visible ie. entirely scrolled into view; otherwise, false.
        public bool WaitForControlVisible()
        {
            WaitCondition loopCondition = (uiControl) =>
            {
                return !(uiControl != null && (bool)uiControl.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty) == false);
            };

            return WaitForControl(loopCondition, elementIsValid);
        }

        /// returns true if control is not found before time-out; otherwise, false.
        public bool WaitForControlNotExist()
        {
            WaitCondition loopCondition = (uiControl) =>
            {
                return ElementIsCurrent(uiControl);                
            };

            return WaitForControl(loopCondition, elementIsInvalid); 
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

            return WaitForControl(loopCondition, elementIsValid);
        }

        /// <summary>
        /// returns true if control meets specified condition before time-out; otherwise, false.
        /// </summary>
        public bool WaitForControlCondition(WaitCondition condition)
        {
            return WaitForControl(condition, elementIsValid);
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
            private set
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

        public int DefaultTimeout
        {
            get { return defaultTimeout; }
        }

        /// <summary>
        /// Set the override Timeout value to wait for the condition in milliseconds
        /// </summary>
        public int OverrideTimeout
        {
            get
            {
                return customTimeout ?? -1;
            }
            set
            {
                customTimeout = value;                
            }
        }

        public int SleepMultiplier
        {
            get
            {
                return sleepMultiplier;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();

                sleepMultiplier = value;
            }
        }
        #endregion
    }
}
