using Microsoft.VisualStudio.TestTools.UnitTesting;
using FunctionalTests.DSL;
using System.Windows.Automation;
using Functional;
using UITestFramework;

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
                    navi.PagesCount.ShouldNotEqual(1);

                    var currentPage = navi.CurrentPage;
                    navi.MoveForwardViaShortcut();
                    navi.Refresh();
                    navi.WaitForPageChange(currentPage + 1).ShouldBeTrue();

                    return true;
                });
        }

        [TestMethod]
        public void MoveBackwardWithKeyboard()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<SlideNavigation>((navi) =>
                {
                    navi.PagesCount.ShouldNotEqual(1);

                    var currentPage = navi.CurrentPage;
                    navi.MoveBackViaShortcut();
                    navi.Refresh();
                    navi.WaitForPageChange(currentPage - 1).ShouldBeTrue();

                    return true;
                });
        }
    }
}
