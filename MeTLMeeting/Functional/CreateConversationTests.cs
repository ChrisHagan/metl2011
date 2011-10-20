using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using Functional.Utilities;

namespace Functional
{
    [TestClass]
    public class CreateConversationTests
    {
        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
        }

        [TestMethod]
        public void CreateNewConversation()
        {
            new ApplicationPopup(metlWindow.AutomationElement).CreateConversation();
        }
    }
}
