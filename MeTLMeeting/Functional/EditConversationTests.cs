using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;

namespace Functional
{
    [TestClass]
    public class EditConversationTests
    {
        private UITestHelper metlWindow;
        
        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
        }

        [TestMethod]
        public void ChangePrivacyToOnlyOwner()
        {
            var editConversation = new ConversationEditScreen(metlWindow.AutomationElement);

            editConversation.ChangeGroup("jpjor1");
            editConversation.Save();
        }

        [TestMethod]
        public void DeleteCurrentConversation()
        {
            var editConversation = new ConversationEditScreen(metlWindow.AutomationElement);

            editConversation.Delete();
        }
        [TestMethod]
        public void AddPageToConversation()
        {
            new SlideNavigation(metlWindow.AutomationElement).Add();
            // TODO: Check that there are now two pages in the conversation
        }
    }
}
