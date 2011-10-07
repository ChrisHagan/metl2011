using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;

namespace Functional
{
    [TestClass]
    public class ShutdownTests
    {
        private AutomationElementCollection metlWindows;

        [TestMethod]
        public void CloseAllInstances()
        {
            if (metlWindows == null)
                metlWindows = MeTL.GetAllMainWindows();

            foreach (AutomationElement window in metlWindows)
            {
                new ApplicationPopup(window).Quit();
            }

            var control = new UITestHelper();
            var success = control.WaitForControlNotExist(Constants.ID_METL_MAIN_WINDOW);
            
            Assert.IsTrue(success, ErrorMessages.PROBLEM_SHUTTING_DOWN);
        }
        
        [TestMethod]
        public void CloseInstance()
        {
            // TODO: Test that there is only one instance
            if (metlWindows == null)
                metlWindows = MeTL.GetAllMainWindows();

            foreach (AutomationElement window in metlWindows)
            {
                new ApplicationPopup(window).Quit();
            }

            var control = new UITestHelper();
            var success = control.WaitForControlNotExist(Constants.ID_METL_MAIN_WINDOW);
            
            Assert.IsTrue(success, ErrorMessages.PROBLEM_SHUTTING_DOWN);
        }
    }
}
