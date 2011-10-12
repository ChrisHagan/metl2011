using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;

namespace Functional
{
    [TestClass]
    public class ContentTests
    {
        private AutomationElement metlWindow;

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
        public void CreateNewConversation()
        {
            new ApplicationPopup(metlWindow).CreateConversation();
        }
    }
}
