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
            metlWindow = MeTL.GetMainWindow();
        }

        private void WaitForSearchScreen()
        {
            var control = new UITestHelper(metlWindow);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_TEXTBOX));
            
            var success = control.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }
        
        private void WaitForLoginError()
        {
            var control = new UITestHelper(metlWindow);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_LOGIN_ERROR_LABEL));

            var success = control.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }

        [TestMethod]
        public void LoginWithValidCredentials()
        {
            // TODO: This needs to be data-driven
            var user = "eecrole";
            var pass = "cleareight6";

            var loginScreen = new Login(metlWindow.AutomationElement).username(user).password(pass);
            loginScreen.submit();

            WaitForSearchScreen();
        }

        [TestMethod]
        public void LoginWithInvalidCredentials()
        {
            var user = "lsdflkjsdf";
            var pass = "lksadflkj";

            var loginScreen = new Login(metlWindow.AutomationElement).username(user).password(pass);

            WaitForLoginError();
        }

        [TestMethod]
        public void LoginAndSaveCredentials()
        {
            // TODO: This needs to be data-driven
            var user = "eecrole";
            var pass = "cleareight6";

            var loginScreen = new Login(metlWindow.AutomationElement).username(user).password(pass);
            loginScreen.remember().submit();

            WaitForSearchScreen();
        }

        [TestMethod]
        public void LoginFromSavedCredentials()
        {
            var loggingIn = new UITestHelper(metlWindow);
            loggingIn.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_LOGGING_IN_LABEL));

            var success = loggingIn.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            WaitForSearchScreen();
        }
    }
}
