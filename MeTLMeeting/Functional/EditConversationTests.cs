using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

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
    }
}
