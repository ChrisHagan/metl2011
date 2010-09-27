using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Ink;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for StrokeExtensionsTest and is intended
    ///to contain all StrokeExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StrokeExtensionsTest
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
        ///A test for startingId
        ///</summary>
        [TestMethod()]
        public void startingIdTest()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            Guid expected = new Guid(); // TODO: Initialize to an appropriate value
            Guid actual;
            actual = StrokeExtensions.startingId(stroke);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for startingSum
        ///</summary>
        [TestMethod()]
        public void startingSumTest()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            actual = StrokeExtensions.startingSum(stroke);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for startingSum
        ///</summary>
        [TestMethod()]
        public void startingSumTest1()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            double startingSum = 0F; // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            actual = StrokeExtensions.startingSum(stroke, startingSum);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for sum
        ///</summary>
        [TestMethod()]
        public void sumTest()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            StrokeChecksum expected = new StrokeChecksum(); // TODO: Initialize to an appropriate value
            StrokeChecksum actual;
            actual = StrokeExtensions.sum(stroke);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for sumId
        ///</summary>
        [TestMethod()]
        public void sumIdTest()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            Guid expected = new Guid(); // TODO: Initialize to an appropriate value
            Guid actual;
            actual = StrokeExtensions.sumId(stroke);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            StrokeTag expected = new StrokeTag(); // TODO: Initialize to an appropriate value
            StrokeTag actual;
            actual = StrokeExtensions.tag(stroke);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest1()
        {
            Stroke stroke = null; // TODO: Initialize to an appropriate value
            StrokeTag tag = new StrokeTag(); // TODO: Initialize to an appropriate value
            StrokeTag expected = new StrokeTag(); // TODO: Initialize to an appropriate value
            StrokeTag actual;
            actual = StrokeExtensions.tag(stroke, tag);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
