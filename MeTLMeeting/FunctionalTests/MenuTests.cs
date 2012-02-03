using System.Windows.Automation;
using Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;

namespace FunctionalTests
{
    [TestClass]
    public class MenuTests
    {
        [TestMethod]
        public void RecentConversationsPopulatedWithCurrentConversation()
        {
            var metlWindow = MeTL.GetMainWindow();
            var success = false;
            foreach (AutomationElement recent in new ApplicationPopup(metlWindow.AutomationElement).RecentConversations())
            {
                if (recent.WalkAllElements(TestConstants.OWNER_CONVERSATION_TITLE) != null)
                {
                    success = true;
                    break;
                }
            }

            Assert.IsTrue(success, ErrorMessages.CONVERSATION_MISSING_FROM_RECENT);
        }
    }
}
