using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using Functional;
using UITestFramework;

namespace FunctionalTests
{
    [TestClass]
    public class CollaborationTests
    {
        private TestContext testContext;
        private AutomationElement ownerWindow;
        private AutomationElement participantWindow;

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
            var windows = MeTL.GetAllMainWindows(2, true);

            ownerWindow = windows[0];
            participantWindow = windows[1];
        }

        private void WaitForSearchScreen(AutomationElement window)
        {
            var control = new UITestHelper(window);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_TEXTBOX));
            
            var success = control.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\UserCredentials.csv", "UserCredentials#csv", DataAccessMethod.Sequential), DeploymentItem("FunctionalTests\\UserCredentials.csv"), TestMethod]
        public void LoginOwnerAndParticipant()
        {
            // owner
            var ownerUser = testContext.DataRow["Username"].ToString();
            var ownerPass = testContext.DataRow["Password"].ToString();

            var loginScreen = new Login(ownerWindow).username(ownerUser).password(ownerPass);
            loginScreen.submit();

            WaitForSearchScreen(ownerWindow);
        }
    }
}
