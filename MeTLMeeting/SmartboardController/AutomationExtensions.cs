using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace SmartboardController
{
    public static class AutomationExtensions
    {
        public static AutomationElement Descendant(this AutomationElement element, string name)
        {
            var result = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, name));
            return result;
        }
        public static AutomationElement Descendant(this AutomationElement element, Type type)
        {
            var result = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static AutomationElementCollection Descendants(this AutomationElement element, Type type)
        {
            var result = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static IEnumerable<AutomationElement> Descendants(this AutomationElement element)
        {
            var result = new AutomationElement[1024];
            element.FindAll(TreeScope.Descendants, System.Windows.Automation.Condition.TrueCondition).CopyTo(result, 0);
            return result.TakeWhile(e => e != null).ToArray();
        }
        public static AutomationElementCollection Children(this AutomationElement element, Type type)
        {
            var result = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static AutomationElement Child(this AutomationElement element, Type type)
        {
            var result = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static string Value(this AutomationElement element)
        {
            return ((ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
        }
        public static AutomationElement Value(this AutomationElement element, string value)
        {
            ((ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern)).SetValue(value);
            return element;
        }
        public static AutomationElement Invoke(this AutomationElement element)
        {
            ((InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
            return element;
        }
        public static string AutomationId(this AutomationElement element)
        {
            return element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty).ToString();
        }
    }
}
