using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;
using System.Threading;

namespace Functional
{
    [TestClass]
    public class LoginTests
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
        public void LoginWithValidCredentials()
        {
            // TODO: This needs to be data-driven, and not using personal details 
            var user = "jpjor1";
            var pass = "h3lp1nh4nd";

            new Login(metlWindow).username(user).password(pass);
            new Login(metlWindow).submit();

            Thread.Sleep(8000);
        }
    }
}
