using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;
using System.Threading;
using System;

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
                var metlWindow = new UITestHelper(UITestHelper.RootElement, window);
                metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));
                metlWindow.Find();

                new ApplicationPopup(metlWindow.AutomationElement).Quit();

                UITestHelper.Wait(TimeSpan.FromSeconds(2));
            }

            Assert.AreEqual(0, MeTL.GetAllMainWindows().Count);
        }
        
        [TestMethod]
        public void CloseInstance()
        {
            if (metlWindows == null)
                metlWindows = MeTL.GetAllMainWindows();
            Assert.IsTrue(metlWindows.Count == 1, ErrorMessages.EXPECTED_ONE_INSTANCE);

            var metlWindow = new UITestHelper(UITestHelper.RootElement, metlWindows[0]);
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));
            metlWindow.Find();

            new ApplicationPopup(metlWindow.AutomationElement).Quit();

            UITestHelper.Wait(TimeSpan.FromSeconds(2));

            Assert.AreEqual(0, MeTL.GetAllMainWindows().Count);
        }

        [TestMethod]
        public void LogoutAndCloseInstance()
        {
            if (metlWindows == null)
                metlWindows = MeTL.GetAllMainWindows();
            Assert.IsTrue(metlWindows.Count == 1, ErrorMessages.EXPECTED_ONE_INSTANCE);

            var metlWindow = new UITestHelper(UITestHelper.RootElement, metlWindows[0]);
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));
            metlWindow.Find();

            var logoutAndExit = new UITestHelper(metlWindow);
            logoutAndExit.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_LOGOUT_AND_EXIT_BACKNAV_BUTTON));

            logoutAndExit.WaitForControlExist();
            logoutAndExit.AutomationElement.Invoke(); 

            UITestHelper.Wait(TimeSpan.FromSeconds(2));

            Assert.AreEqual(0, MeTL.GetAllMainWindows().Count);
        }
    }
}
