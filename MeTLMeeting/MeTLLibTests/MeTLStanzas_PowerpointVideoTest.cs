using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_PowerpointVideoTest and is intended
    ///to contain all MeTLStanzas_PowerpointVideoTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_PowerpointVideoTest
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
        public void MeTLStanzas_PowerpointVideoConstructorTest()
        {
            TargettedPowerpointBackgroundVideo video = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(video);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PowerpointVideo Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_PowerpointVideoConstructorTest1()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for forceEvaluation
        ///</summary>
        [TestMethod()]
        public void forceEvaluationTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            MeTLStanzas.PowerpointVideo expected = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.PowerpointVideo actual;
            actual = target.forceEvaluation();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AnimationTimeLine
        ///</summary>
        [TestMethod()]
        public void AnimationTimeLineTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            MediaState expected = new MediaState(); // TODO: Initialize to an appropriate value
            MediaState actual;
            target.PlayMode = expected;
            actual = target.PlayMode;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for height
        ///</summary>
        [TestMethod()]
        public void heightTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.height = expected;
            actual = target.height;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.tag = expected;
            actual = target.tag;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for targettedVideo
        ///</summary>
        [TestMethod()]
        public void targettedVideoTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            TargettedPowerpointBackgroundVideo expected = null; // TODO: Initialize to an appropriate value
            TargettedPowerpointBackgroundVideo actual;
            target.targettedVideo = expected;
            actual = target.targettedVideo;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for video
        ///</summary>
        [TestMethod()]
        public void videoTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            MediaElement expected = null; // TODO: Initialize to an appropriate value
            MediaElement actual;
            target.video = expected;
            actual = target.video;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for width
        ///</summary>
        [TestMethod()]
        public void widthTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.width = expected;
            actual = target.width;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for x
        ///</summary>
        [TestMethod()]
        public void xTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.x = expected;
            actual = target.x;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for y
        ///</summary>
        [TestMethod()]
        public void yTest()
        {
            MeTLStanzas.PowerpointVideo target = new MeTLStanzas.PowerpointVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.y = expected;
            actual = target.y;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
