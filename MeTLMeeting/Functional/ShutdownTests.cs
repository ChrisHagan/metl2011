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

                var control = new UITestHelper(window);
                control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

                var success = control.WaitForControlNotExist();
                Assert.IsTrue(success, ErrorMessages.PROBLEM_SHUTTING_DOWN);
            }
        }
        
        [TestMethod]
        public void CloseInstance()
        {
            if (metlWindows == null)
                metlWindows = MeTL.GetAllMainWindows();
            Assert.IsTrue(metlWindows.Count == 1, ErrorMessages.EXPECTED_ONE_INSTANCE);

            var metlWindow = new UITestHelper(metlWindows[0]);
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));
            metlWindow.Find();

            new ApplicationPopup(metlWindow.AutomationElement).Quit();

            var success = metlWindow.WaitForControlNotExist();
            Assert.IsTrue(success, ErrorMessages.PROBLEM_SHUTTING_DOWN);
        }
    }
}
