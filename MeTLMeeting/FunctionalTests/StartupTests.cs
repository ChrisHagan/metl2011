using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Collections.Generic;
using System.Linq;

namespace Functional
{
    [TestClass]
    public class StartupTests
    {
        [TestMethod]
        public void StartOneInstance()
        {
            Assert.AreEqual(0, MeTL.GetAllMainWindows().Count);

            var metl = new UITestHelper(UITestHelper.RootElement, MeTL.StartProcess());
            metl.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metl.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }

        [TestMethod]
        public void StartTwoInstances()
        {
            var propertyExpression = new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW);
            foreach (var i in Enumerable.Range(0,2))
            {
                var metl = new UITestHelper(UITestHelper.RootElement, MeTL.StartProcess());
                metl.SearchProperties.Add(propertyExpression);

                var success = metl.WaitForControlEnabled();
                Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
            }

            var desktopBounds = AutomationElement.RootElement.Current.BoundingRectangle;
            var windows = MeTL.GetAllMainWindows(2, true);
            Assert.AreEqual(2, windows.Count);

            // move each window into position
            var windowCount = 0;
            foreach (AutomationElement window in windows)
            {
                var windowPattern = window.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
                windowPattern.SetWindowVisualState(WindowVisualState.Normal);

                var transformPattern = window.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                transformPattern.Resize(desktopBounds.Width / 2, desktopBounds.Height);
                transformPattern.Move((desktopBounds.Width / 2) * windowCount++, 0);
            }
        }
    }
}
