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
            new ApplicationPopup(metlWindow.AutomationElement).open();

            var recent = metlWindow.AutomationElement.Descendant("PART_RecentDocumentsControl");
            recent.WalkAllElements("CITestsSearchTestOwner");
        }
    }
}
