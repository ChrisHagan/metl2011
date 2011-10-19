using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using Microsoft.Test.VisualVerification;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows;

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
        [DeploymentItem("Master1280x800.png")]
        [DeploymentItem("ToleranceMap1280x800.png")]
        [DeploymentItem("Master1280x768.png")]
        [DeploymentItem("ToleranceMap1280x768.png")]
        public void ZoomInOnce()
        {
            string toleranceFile = @"ToleranceMap1280x768.png";
            string masterFile = @"Master1280x768.png";
 
            if (SystemParameters.PrimaryScreenWidth == 1280 && SystemParameters.PrimaryScreenHeight == 800)
            {
                toleranceFile = @"ToleranceMap1280x800.png";
                masterFile = @"Master1280x800.png";
            }

            new ZoomButtons(metlWindow.AutomationElement).ZoomIn();

            // verify zoom
            var tolerance = Snapshot.FromFile(toleranceFile);
            var master = Snapshot.FromFile(masterFile);
            var actual = Snapshot.FromWindow((IntPtr)metlWindow.AutomationElement.Current.NativeWindowHandle, WindowSnapshotMode.ExcludeWindowBorder);
            actual.ToFile(@"Master-actual.png", ImageFormat.Png);
            var diff = actual.CompareTo(master);

            master.ToFile(@"Master-expected.png", ImageFormat.Png);
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
