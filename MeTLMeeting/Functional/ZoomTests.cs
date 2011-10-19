using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using Microsoft.Test.VisualVerification;
using System;
using System.Drawing.Imaging;

namespace Functional
{
    [TestClass]
    public class ZoomTests
    {
        private UITestHelper metlWindow;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = new UITestHelper();
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metlWindow.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }

        [TestMethod]
        [DeploymentItem("Master.png")]
        [DeploymentItem("ToleranceMap.png")]
        public void ZoomInOnce()
        {
            new ZoomButtons(metlWindow.AutomationElement).ZoomIn();

            // verify zoom
            var tolerance = Snapshot.FromFile("ToleranceMap.png");
            var master = Snapshot.FromFile("Master.png");
            var actual = Snapshot.FromWindow((IntPtr)metlWindow.AutomationElement.Current.NativeWindowHandle, WindowSnapshotMode.ExcludeWindowBorder);
            var diff = actual.CompareTo(master);

            master.ToFile(@"Master-expected.png", ImageFormat.Png);
            actual.ToFile(@"Master-actual.png", ImageFormat.Png);
            diff.ToFile(@"Master-difference.png", ImageFormat.Png);

            var verifier = new SnapshotToleranceMapVerifier(tolerance);
            Assert.AreEqual(VerificationResult.Pass, verifier.Verify(diff));
        }

        [TestMethod]
        public void JoinZoomConversation()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);
            search.searchField("ZoomTest");
            search.Search();

            search.JoinFirstFound();
        }
    }
}
