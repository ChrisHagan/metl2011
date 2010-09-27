using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for VideoTest and is intended
    ///to contain all VideoTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VideoTest
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
        ///A test for Video Constructor
        ///</summary>
        [TestMethod()]
        public void VideoConstructorTest()
        {
            Video target = new Video();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for UpdateMirror
        ///</summary>
        [TestMethod()]
        public void UpdateMirrorTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            string id = string.Empty; // TODO: Initialize to an appropriate value
            target.UpdateMirror(id);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Duration
        ///</summary>
        [TestMethod()]
        public void DurationTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            Duration expected = new Duration(); // TODO: Initialize to an appropriate value
            Duration actual;
            target.Duration = expected;
            actual = target.Duration;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Height
        ///</summary>
        [TestMethod()]
        public void HeightTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.Height = expected;
            actual = target.Height;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for MediaElement
        ///</summary>
        [TestMethod()]
        public void MediaElementTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            MediaElement expected = null; // TODO: Initialize to an appropriate value
            MediaElement actual;
            target.MediaElement = expected;
            actual = target.MediaElement;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Position
        ///</summary>
        [TestMethod()]
        public void PositionTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.Position = expected;
            actual = target.Position;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VideoDuration
        ///</summary>
        [TestMethod()]
        public void VideoDurationTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
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
            Video target = new Video(); // TODO: Initialize to an appropriate value
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
            Video target = new Video(); // TODO: Initialize to an appropriate value
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
            Video target = new Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.VideoWidth = expected;
            actual = target.VideoWidth;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Width
        ///</summary>
        [TestMethod()]
        public void WidthTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.Width = expected;
            actual = target.Width;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for X
        ///</summary>
        [TestMethod()]
        public void XTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.X = expected;
            actual = target.X;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Y
        ///</summary>
        [TestMethod()]
        public void YTest()
        {
            Video target = new Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.Y = expected;
            actual = target.Y;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
