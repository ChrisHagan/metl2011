using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

namespace Functional
{
    [TestClass]
    public class StartupTests
    {
        private AutomationElement metlWindow;
        
        [TestMethod]
        public void StartOneInstance()
        {
            metlWindow = MeTL.StartProcess();

            var control = new UITestHelper();
            var success = control.WaitForControlEnabled(Constants.ID_METL_MAIN_WINDOW);

            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);

            if (metlWindow == null)
                metlWindow = MeTL.GetMainWindow();

            Assert.IsNotNull(metlWindow, ErrorMessages.EXPECTED_MAIN_WINDOW); 
        }

        [TestMethod]
        public void StartTwoInstances()
        {
            MeTL.StartProcess();
            MeTL.StartProcess();

            var control = new UITestHelper();
            var success = control.WaitForControlEnabled(Constants.ID_METL_MAIN_WINDOW);

            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);

            if (metlWindow == null)
                metlWindow = MeTL.GetMainWindow();

            Assert.IsNotNull(metlWindow, ErrorMessages.EXPECTED_MAIN_WINDOW); 
        }
    }
}
