using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;

namespace Functional
{
    [TestClass]
    public class TextModeTests
    {
        private UITestHelper metlWindow;
        private HomeTabScreen homeTab;
        
        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();

            homeTab = new HomeTabScreen(metlWindow.AutomationElement).OpenTab();
        }

        [TestMethod]
        public void ActivateTextMode()
        {
            homeTab.ActivateTextMode();
        }

        [TestMethod]
        public void InsertText()
        {

        }
    }
}
