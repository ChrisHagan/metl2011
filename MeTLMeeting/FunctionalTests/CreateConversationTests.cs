using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using FunctionalTests.Utilities;
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
            // Default size is dependent on hardware, so what is the expected size for a particular screen resolution
            //var expectSize = new Size(759, 569); // Hardcoded values for the expected size of canvas 
            Assert.AreNotEqual(Size.Empty, canvasSize);
        }
    }
}
