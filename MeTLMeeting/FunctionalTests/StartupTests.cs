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
        }
    }
}
