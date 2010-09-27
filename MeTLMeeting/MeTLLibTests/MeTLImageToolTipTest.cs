using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLImageToolTipTest and is intended
    ///to contain all MeTLImageToolTipTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLImageToolTipTest
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
        ///A test for MeTLImageToolTip Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLImageToolTipConstructorTest()
        {
            MeTLImageToolTip target = new MeTLImageToolTip();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ContentText
        ///</summary>
        [TestMethod()]
        public void ContentTextTest()
        {
            MeTLImageToolTip target = new MeTLImageToolTip(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.ContentText = expected;
            actual = target.ContentText;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ImageSource
        ///</summary>
        [TestMethod()]
        public void ImageSourceTest()
        {
            MeTLImageToolTip target = new MeTLImageToolTip(); // TODO: Initialize to an appropriate value
            ImageSource expected = null; // TODO: Initialize to an appropriate value
            ImageSource actual;
            target.ImageSource = expected;
            actual = target.ImageSource;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for TitleText
        ///</summary>
        [TestMethod()]
        public void TitleTextTest()
        {
            MeTLImageToolTip target = new MeTLImageToolTip(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.TitleText = expected;
            actual = target.TitleText;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
