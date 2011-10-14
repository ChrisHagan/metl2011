using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

namespace Functional
{
    [TestClass]
    public class ZoomTests
    {        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            MeTL.StartProcess();

            metlWindow = new UITestHelper();
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metlWindow.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }
        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
