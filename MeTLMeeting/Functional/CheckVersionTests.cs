using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

namespace Functional
{
    [TestClass]
    public class CheckVersionTests
    {
        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = new UITestHelper();
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metlWindow.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }

        [TestMethod]
        public void CheckVersionAgainstHardcoded()
        {
        }
    }
}
