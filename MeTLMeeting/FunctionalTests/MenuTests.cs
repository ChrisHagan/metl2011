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

                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    foreach (var i in Enumerable.Range(0, home.PenColourCount))
                    {
                        PenPopupScreen colours = home.ModifyPen(i).PopupColours();
                        foreach (var j in Enumerable.Range(0, colours.ColourCount))
                        {
                            colours.SelectColour(j);
                            UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                            colours = home.ModifyPen(i).PopupColours();
                        }
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                        colours = home.ModifyPen(i).PopupColours();
                        foreach (var k in Enumerable.Range(0, colours.SizeCount))
                        {
                            colours.SelectSize(k);
                            UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                            colours = home.ModifyPen(i).PopupColours();
                        }
                    }
                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));

                    return true;
                });
        }

        [TestMethod]
        public void CanResetAllPensToDefault()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.ActivatePenMode();

                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    foreach (var i in Enumerable.Range(0, home.PenColourCount))
                    {
                        home.ModifyPen(i).PopupColours().ResetToDefault();
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    }

                    return true;
                });
        }

        [TestMethod]
        public void ActivatePrivateMode()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.OpenTab();

                    home.PrivateMode();

                    return true;
                });
        }

        [TestMethod]
        public void ActivatePublicMode()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.OpenTab();

                    home.PublicMode();

                    return true;
                });
        }
    }
}
