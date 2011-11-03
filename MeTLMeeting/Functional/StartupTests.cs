using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

namespace Functional
{
    [TestClass]
    public class StartupTests
    {
        private UITestHelper metlWindow1;
        private UITestHelper metlWindow2;
        //private AutomationElementCollection metlWindows;
        
        [TestMethod]
        public void StartOneInstance()
        {
            MeTL.StartProcess();

            metlWindow1 = MeTL.GetMainWindow();
            var success1 = metlWindow1.WaitForControlEnabled();
            Assert.IsTrue(success1, ErrorMessages.EXPECTED_MAIN_WINDOW);

            /*//Assert.AreEqual(metlWindows.Count, 1);

            metlWindow1 = new UITestHelper(metlWindows[0]);
            metlWindow1.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metlWindow1.WaitForControlEnabled();
            //Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);

            foreach (var window in metlWindows)
            {
                var metlWindow = new UITestHelper();
                metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

                var success = metlWindow.WaitForControlExist();
                Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
            }
            */
        }

        [TestMethod]
        public void StartTwoInstances()
        {
            metlWindow1 = new UITestHelper(MeTL.StartProcess());
            metlWindow2 = new UITestHelper(MeTL.StartProcess());

            var propertyExpression = new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW);
            metlWindow1.SearchProperties.Add(propertyExpression);
            metlWindow2.SearchProperties.Add(propertyExpression);

            var success1 = metlWindow1.WaitForControlEnabled();
            Assert.IsTrue(success1, ErrorMessages.EXPECTED_MAIN_WINDOW);

            var success2 = metlWindow2.WaitForControlEnabled();
            Assert.IsTrue(success2, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }
    }
}
