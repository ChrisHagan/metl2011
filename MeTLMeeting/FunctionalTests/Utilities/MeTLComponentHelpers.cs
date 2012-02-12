using System.Windows.Automation;
using Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SandRibbon.Components;
using UITestFramework;

namespace FunctionalTests.Utilities
{
    public abstract class MeTLScreenComponent
    {
    }

    public class ConversationAction : MeTLScreenComponent
    {
        public void WaitUntilConversationJoined(AutomationElement parent)
        {
            // wait until we've finished joining the conversation before returning
            var canvasStack = new UITestHelper(parent, parent.Descendant(typeof(SandRibbon.Components.CollapsedCanvasStack)));
            var success = canvasStack.WaitForControlEnabled();

            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
        }
    }
}
