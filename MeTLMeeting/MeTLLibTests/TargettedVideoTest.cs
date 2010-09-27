using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for TargettedVideoTest and is intended
    ///to contain all TargettedVideoTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TargettedVideoTest
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
        ///A test for TargettedVideo Constructor
        ///</summary>
        [TestMethod()]
        public void TargettedVideoConstructorTest()
        {
            TargettedVideo target = new TargettedVideo();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Height
        ///</summary>
        [TestMethod()]
        public void HeightTest()
        {
            TargettedVideo target = new TargettedVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.Height = expected;
            actual = target.Height;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Width
        ///</summary>
        [TestMethod()]
        public void WidthTest()
        {
            TargettedVideo target = new TargettedVideo(); // TODO: Initialize to an appropriate value
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
            TargettedVideo target = new TargettedVideo(); // TODO: Initialize to an appropriate value
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
            TargettedVideo target = new TargettedVideo(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.Y = expected;
            actual = target.Y;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for video
        ///</summary>
        [TestMethod()]
        public void videoTest()
        {
            TargettedVideo target = new TargettedVideo(); // TODO: Initialize to an appropriate value
            Video expected = null; // TODO: Initialize to an appropriate value
            Video actual;
            target.video = expected;
            actual = target.video;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
