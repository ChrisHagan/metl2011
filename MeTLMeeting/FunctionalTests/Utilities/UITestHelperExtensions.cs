using UITestFramework;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests.Utilities
{
    public static class UITestHelperExtensions
    {
        public static void SetFocus(this UITestHelper element)
        {
            element.ShouldNotBeNull();
            element.AutomationElement.SetFocus();
        }

        public static AutomationElement Descendant(this UITestHelper element, string name)
        {
            element.ShouldNotBeNull();
            return element.AutomationElement.Descendant(name);
        }
    }
}
