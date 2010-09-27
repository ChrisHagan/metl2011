using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_AutoShapeTest and is intended
    ///to contain all MeTLStanzas_AutoShapeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_AutoShapeTest
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
        public void MeTLStanzas_AutoShapeConstructorTest()
        {
            TargettedAutoShape autoshape = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(autoshape);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for AutoShape Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_AutoShapeConstructorTest1()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Background
        ///</summary>
        [TestMethod()]
        public void BackgroundTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            Brush expected = null; // TODO: Initialize to an appropriate value
            Brush actual;
            target.Foreground = expected;
            actual = target.Foreground;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for StrokeThickness
        ///</summary>
        [TestMethod()]
        public void StrokeThicknessTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.StrokeThickness = expected;
            actual = target.StrokeThickness;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for autoshape
        ///</summary>
        [TestMethod()]
        public void autoshapeTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            TargettedAutoShape expected = null; // TODO: Initialize to an appropriate value
            TargettedAutoShape actual;
            target.autoshape = expected;
            actual = target.autoshape;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for height
        ///</summary>
        [TestMethod()]
        public void heightTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.height = expected;
            actual = target.height;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for pathData
        ///</summary>
        [TestMethod()]
        public void pathDataTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            PathGeometry expected = null; // TODO: Initialize to an appropriate value
            PathGeometry actual;
            target.pathData = expected;
            actual = target.pathData;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.tag = expected;
            actual = target.tag;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for width
        ///</summary>
        [TestMethod()]
        public void widthTest()
        {
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.AutoShape target = new MeTLStanzas.AutoShape(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.y = expected;
            actual = target.y;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
