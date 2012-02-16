using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using Functional;
using UITestFramework;
using FunctionalTests.DSL;
using Microsoft.Test.Input;

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
        }

        [TestMethod]
        public void JoinConversation()
        {
            var conversation = new FunctionalTests.Actions.SearchConversation();

            conversation.SearchForConversationAndJoin(new UITestHelper(UITestHelper.RootElement, ownerWindow), TestConstants.OWNER_CONVERSATION_TITLE);
            conversation.SearchForConversationAndJoin(new UITestHelper(UITestHelper.RootElement, participantWindow), TestConstants.OWNER_CONVERSATION_TITLE);
        }

        [TestMethod]
        public void AddTextToOwner()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode();

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    var textboxCount = canvas.ChildTextboxes.Count;

                    canvas.InsertTextbox(canvas.RandomPointWithinMargin(-40, -40), "owner");

                    canvas.ChildTextboxes.Count.ShouldEqual(textboxCount + 1);
                });
        }

        [TestMethod]
        public void AddTextToParticipant()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<HomeTabScreen>(home =>
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode();
                    return true;
                })
                .With<CollapsedCanvasStack>(canvas =>
                {
                    var textboxCount = canvas.ChildTextboxes.Count;

                    canvas.InsertTextbox(canvas.RandomPointWithinMargin(-40, -40), "participant");

                    canvas.ChildTextboxes.Count.ShouldEqual(textboxCount + 1);
                });
        }

        [TestMethod]
        public void AddInkToOwner()
        {

        }

        [TestMethod]
        public void AddInkToParticipant()
        {

        }
    }
}
