using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;

namespace Functional
{
    [TestClass]
    public class StartupAndShutdown
    {
        private AutomationElement metlWindow;

        [ClassInitialize]
        public static void StartProcess(TestContext context)
        {
            MeTL.StartProcess();
        }

        [TestInitialize]
        public void Setup()
        {
            var control = new UITestHelper();
            var success = control.WaitForControlEnabled(Constants.ID_METL_MAIN_WINDOW);
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);

            if (metlWindow == null)
                metlWindow = MeTL.GetMainWindow();

            Assert.IsNotNull(metlWindow, ErrorMessages.EXPECTED_MAIN_WINDOW); 
        }

        [TestMethod]
        public void Quit()
        {
            new ApplicationPopup(metlWindow).Quit();

            var control = new UITestHelper();
            var success = control.WaitForControlNotExist(Constants.ID_METL_MAIN_WINDOW);
            Assert.IsTrue(success, ErrorMessages.PROBLEM_SHUTTING_DOWN);
        }
    }
}
