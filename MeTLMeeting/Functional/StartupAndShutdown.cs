using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;

namespace Functional
{
    [TestClass]
    public class StartupAndShutdown
    {
        private UITestHelper metlWindow;

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
        public void CloseProgram()
        {
            new ApplicationPopup(metlWindow.AutomationElement).Quit();

            var success = metlWindow.WaitForControlNotExist();
            Assert.IsTrue(success, ErrorMessages.PROBLEM_SHUTTING_DOWN);
        }
    }
}
