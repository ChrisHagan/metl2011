using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for VideoMirrorTest and is intended
    ///to contain all VideoMirrorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VideoMirrorTest
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
        ///A test for VideoMirror Constructor
        ///</summary>
        [TestMethod()]
        public void VideoMirrorConstructorTest()
        {
            VideoMirror target = new VideoMirror();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for RequestNewRectangle
        ///</summary>
        [TestMethod()]
        public void RequestNewRectangleTest()
        {
            VideoMirror target = new VideoMirror(); // TODO: Initialize to an appropriate value
            target.RequestNewRectangle();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UpdateMirror
        ///</summary>
        [TestMethod()]
        public void UpdateMirrorTest()
        {
            VideoMirror target = new VideoMirror(); // TODO: Initialize to an appropriate value
            VideoMirror.VideoMirrorInformation info = null; // TODO: Initialize to an appropriate value
            target.UpdateMirror(info);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RectFill
        ///</summary>
        [TestMethod()]
        public void RectFillTest()
        {
            VideoMirror target = new VideoMirror(); // TODO: Initialize to an appropriate value
            Brush expected = null; // TODO: Initialize to an appropriate value
            Brush actual;
            target.RectFill = expected;
            actual = target.RectFill;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for RectHeight
        ///</summary>
        [TestMethod()]
        public void RectHeightTest()
        {
            VideoMirror target = new VideoMirror(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.RectHeight = expected;
            actual = target.RectHeight;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for RectWidth
        ///</summary>
        [TestMethod()]
        public void RectWidthTest()
        {
            VideoMirror target = new VideoMirror(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.RectWidth = expected;
            actual = target.RectWidth;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Rectangle
        ///</summary>
        [TestMethod()]
        public void RectangleTest()
        {
            VideoMirror target = new VideoMirror(); // TODO: Initialize to an appropriate value
            Rectangle expected = null; // TODO: Initialize to an appropriate value
            Rectangle actual;
            target.Rectangle = expected;
            actual = target.Rectangle;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
