using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;
using System.Threading;

namespace Functional
{
    [TestClass]
    public class LoginTests
    {
        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = new UITestHelper();
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metlWindow.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }
        
        [TestMethod]
        public void LoginWithValidCredentials()
        {
            // TODO: This needs to be data-driven, and not using personal details 
            var user = "jpjor1";
            var pass = "h3lp1nh4nd";

            var loginScreen = new Login(metlWindow.AutomationElement).username(user).password(pass);
            loginScreen.submit(); 

            var control = new UITestHelper(metlWindow);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_TEXTBOX));
            var success = control.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }

        [TestMethod]
        public void LoginAndSaveCredentials()
        {
            // TODO: This needs to be data-driven, and not using personal details 
            var user = "jpjor1";
            var pass = "h3lp1nh4nd";

            var loginScreen = new Login(metlWindow.AutomationElement).username(user).password(pass);
            loginScreen.remember().submit();

            var control = new UITestHelper(metlWindow);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_TEXTBOX));
            var success = control.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }
    }
}
