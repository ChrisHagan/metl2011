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

            metlWindow = MeTL.GetMainWindow();
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
