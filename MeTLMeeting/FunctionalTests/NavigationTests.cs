using Microsoft.VisualStudio.TestTools.UnitTesting;
using FunctionalTests.DSL;
using System.Windows.Automation;
using Functional;
using UITestFramework;
using System;

namespace FunctionalTests
{
    [TestClass]
    public class NavigationTests
    {
        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
            metlWindow.AutomationElement.SetFocus();
        }

        [TestMethod]
        public void MoveForwardWithKeyboard()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<SlideNavigation>((navi) =>
                {
                    UITestHelper.Wait(TimeSpan.FromSeconds(2));
                    navi.PagesCount.ShouldNotEqual(1);
                    navi.WaitForPageChange(1, () => navi.MoveForwardViaShortcut() ).ShouldBeTrue();

                    return true;
                });
        }

        [TestMethod]
        public void MoveBackwardWithKeyboard()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<SlideNavigation>((navi) =>
                {
                    UITestHelper.Wait(TimeSpan.FromSeconds(2));
                    navi.PagesCount.ShouldNotEqual(1);
                    navi.WaitForPageChange(0, () => navi.MoveBackViaShortcut()).ShouldBeTrue();

                    return true;
                });
        }
    }
}
