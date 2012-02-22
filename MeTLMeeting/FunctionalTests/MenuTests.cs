using System.Windows.Automation;
using Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using FunctionalTests.DSL;
using System.Linq;
using System;

namespace FunctionalTests
{
    [TestClass]
    public class MenuTests
    {
        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
        }

        [TestMethod]
        public void RecentConversationsPopulatedWithCurrentConversation()
        {
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

        [TestMethod]
        public void CanSelectAllInkColoursAndSizes()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.ActivatePenMode();

                    foreach (var i in Enumerable.Range(0, home.PenColourCount))
                    {
                        home.ModifyPen(i);
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    }

                    return true;
                });
        }
    }
}
