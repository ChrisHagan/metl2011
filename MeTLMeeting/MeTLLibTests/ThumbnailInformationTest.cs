using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for ThumbnailInformationTest and is intended
    ///to contain all ThumbnailInformationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ThumbnailInformationTest
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
        ///A test for ThumbnailInformation Constructor
        ///</summary>
        [TestMethod()]
        public void ThumbnailInformationConstructorTest()
        {
            ThumbnailInformation target = new ThumbnailInformation();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Canvas
        ///</summary>
        [TestMethod()]
        public void CanvasTest()
        {
            ThumbnailInformation target = new ThumbnailInformation(); // TODO: Initialize to an appropriate value
            Visual expected = null; // TODO: Initialize to an appropriate value
            Visual actual;
            target.Canvas = expected;
            actual = target.Canvas;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Exposed
        ///</summary>
        [TestMethod()]
        public void ExposedTest()
        {
            ThumbnailInformation target = new ThumbnailInformation(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.Exposed = expected;
            actual = target.Exposed;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ThumbnailBrush
        ///</summary>
        [TestMethod()]
        public void ThumbnailBrushTest()
        {
            ThumbnailInformation target = new ThumbnailInformation(); // TODO: Initialize to an appropriate value
            ImageBrush expected = null; // TODO: Initialize to an appropriate value
            ImageBrush actual;
            target.ThumbnailBrush = expected;
            actual = target.ThumbnailBrush;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for slideId
        ///</summary>
        [TestMethod()]
        public void slideIdTest()
        {
            ThumbnailInformation target = new ThumbnailInformation(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.slideId = expected;
            actual = target.slideId;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for slideNumber
        ///</summary>
        [TestMethod()]
        public void slideNumberTest()
        {
            ThumbnailInformation target = new ThumbnailInformation(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.slideNumber = expected;
            actual = target.slideNumber;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for type
        ///</summary>
        [TestMethod()]
        public void typeTest()
        {
            ThumbnailInformation target = new ThumbnailInformation(); // TODO: Initialize to an appropriate value
            Slide.TYPE expected = new Slide.TYPE(); // TODO: Initialize to an appropriate value
            Slide.TYPE actual;
            target.type = expected;
            actual = target.type;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
