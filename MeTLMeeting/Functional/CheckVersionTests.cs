using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

namespace Functional
{
    [TestClass]
    public class CheckVersionTests
    {
        private UITestHelper metlWindow;
        private string expectedVersionString = "1.0.0.179";

        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
        }

        [TestMethod]
        public void CheckVersionAgainstHardcoded()
        {
            var version = new UITestHelper(metlWindow);
            version.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_VERSION_LABEL));

            version.Find();
            Assert.AreEqual(version.AutomationElement.Current.Name, expectedVersionString, ErrorMessages.VERSION_MISMATCH);
        }
    }
}
