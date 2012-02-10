using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;
using System.Data;
using System.Threading;
using System;

namespace Functional
{
    [TestClass]
    public class LoginTests
    {
        private TestContext testContext;
        private UITestHelper metlWindow;

        public TestContext TestContext 
        {
            get
            {
                return testContext;
            }
            set
            {
                testContext = value;
            }
        }

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

            var success = control.WaitForControlVisible();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\UserCredentials.csv", "UserCredentials#csv", DataAccessMethod.Sequential), DeploymentItem("FunctionalTests\\UserCredentials.csv"), TestMethod]
        public void LoginWithValidCredentials()
        {
            var user = testContext.DataRow["Username"].ToString();
            var pass = testContext.DataRow["Password"].ToString();

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
            loginScreen.submit();

            WaitForLoginError();
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\UserCredentials.csv", "UserCredentials#csv", DataAccessMethod.Sequential), DeploymentItem("FunctionalTests\\UserCredentials.csv"), TestMethod]
        public void LoginAndSaveCredentials()
        {
            var user = testContext.DataRow["Username"].ToString();
            var pass = testContext.DataRow["Password"].ToString();

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
