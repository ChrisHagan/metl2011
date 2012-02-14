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
        private static AutomationElement ownerWindow;
        private static AutomationElement participantWindow;
        private static AutomationElementCollection metlWindows;


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

        [ClassInitialize]
        public static void ClassSetup(TestContext testContext)
        {
            // need to do this once at the start because the window order returned back is nondeterministic
            if (metlWindows == null)
            {
                metlWindows = MeTL.GetAllMainWindows(2, true);
            }
            ownerWindow = null;
            participantWindow = null;
        }

        private void WaitForSearchScreen(AutomationElement window)
        {
            var control = new UITestHelper(window);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_TEXTBOX));
            
            var success = control.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }

        private AutomationElement DetermineCurrentWindow()
        {
            if (ownerWindow == null)
            {
                ownerWindow = metlWindows[0];
                return ownerWindow;
            }

            if (participantWindow == null)
            {
                participantWindow = metlWindows[1];
                return participantWindow;
            }

            return null;
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\MultipleUserCredentials.csv", "MultipleUserCredentials#csv", DataAccessMethod.Sequential), DeploymentItem("FunctionalTests\\UserCredentials.csv"), TestMethod]
        public void LoginOwnerAndParticipant()
        {
            AutomationElement currentWindow = DetermineCurrentWindow();
            
            var user = testContext.DataRow["Username"].ToString();
            var pass = testContext.DataRow["Password"].ToString();

            var loginScreen = new Login(currentWindow).username(user).password(pass);
            loginScreen.submit();

            WaitForSearchScreen(currentWindow);
        }

        [TestMethod]
        public void AddTextToOwner()
        {

        }

        [TestMethod]
        public void AddInkToOwner()
        {

        }

        [TestMethod]
        public void AddTextToParticipant()
        {

        }

        [TestMethod]
        public void AddInkToParticipant()
        {

        }

        [TestMethod]
        public void JoinConversation()
        {
            
        }
    }
}
