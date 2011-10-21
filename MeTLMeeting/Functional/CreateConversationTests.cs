using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using Functional.Utilities;
using System.Windows;

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

        [TestMethod]
        public void ConversationCanvasIsExpectedSize()
        {
            var canvasStack = metlWindow.AutomationElement.Descendant(typeof(UserCanvasStack));
            var canvasSize = ((Rect)canvasStack.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)).Size;
            var expectSize = new Size(759, 569); // Hardcoded values for the expected size of canvas 
            Assert.AreEqual(expectSize, canvasSize);
        }
    }
}
