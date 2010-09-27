using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for PowerpointVideoTest and is intended
    ///to contain all PowerpointVideoTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PowerpointVideoTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for PowerpointVideo Constructor
        ///</summary>
        [TestMethod()]
        public void PowerpointVideoConstructorTest()
        {
            PowerpointVideo target = new PowerpointVideo();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            TextTag expected = new TextTag(); // TODO: Initialize to an appropriate value
            TextTag actual;
            actual = target.tag();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AnimationTimeLine
        ///</summary>
        [TestMethod()]
        public void AnimationTimeLineTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.AnimationTimeLine = expected;
            actual = target.AnimationTimeLine;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for PlayMode
        ///</summary>
        [TestMethod()]
        public void PlayModeTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            MediaState expected = new MediaState(); // TODO: Initialize to an appropriate value
            MediaState actual;
            target.PlayMode = expected;
            actual = target.PlayMode;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VideoDuration
        ///</summary>
        [TestMethod()]
        public void VideoDurationTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.VideoDuration = expected;
            actual = target.VideoDuration;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VideoHeight
        ///</summary>
        [TestMethod()]
        public void VideoHeightTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.VideoHeight = expected;
            actual = target.VideoHeight;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VideoSource
        ///</summary>
        [TestMethod()]
        public void VideoSourceTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            Uri expected = null; // TODO: Initialize to an appropriate value
            Uri actual;
            target.VideoSource = expected;
            actual = target.VideoSource;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VideoWidth
        ///</summary>
        [TestMethod()]
        public void VideoWidthTest()
        {
            PowerpointVideo target = new PowerpointVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.VideoWidth = expected;
            actual = target.VideoWidth;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
