using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for AutoShapeTest and is intended
    ///to contain all AutoShapeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AutoShapeTest
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
        ///A test for AutoShape Constructor
        ///</summary>
        [TestMethod()]
        public void AutoShapeConstructorTest()
        {
            AutoShape target = new AutoShape();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Background
        ///</summary>
        [TestMethod()]
        public void BackgroundTest()
        {
            AutoShape target = new AutoShape(); // TODO: Initialize to an appropriate value
            Brush expected = null; // TODO: Initialize to an appropriate value
            Brush actual;
            target.Background = expected;
            actual = target.Background;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Foreground
        ///</summary>
        [TestMethod()]
        public void ForegroundTest()
        {
            AutoShape target = new AutoShape(); // TODO: Initialize to an appropriate value
            Brush expected = null; // TODO: Initialize to an appropriate value
            Brush actual;
            target.Foreground = expected;
            actual = target.Foreground;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for PathData
        ///</summary>
        [TestMethod()]
        public void PathDataTest()
        {
            AutoShape target = new AutoShape(); // TODO: Initialize to an appropriate value
            PathGeometry expected = null; // TODO: Initialize to an appropriate value
            PathGeometry actual;
            target.PathData = expected;
            actual = target.PathData;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for StrokeThickness
        ///</summary>
        [TestMethod()]
        public void StrokeThicknessTest()
        {
            AutoShape target = new AutoShape(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.StrokeThickness = expected;
            actual = target.StrokeThickness;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
