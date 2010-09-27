using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLContentToolTipTest and is intended
    ///to contain all MeTLContentToolTipTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLContentToolTipTest
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
        ///A test for MeTLContentToolTip Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLContentToolTipConstructorTest()
        {
            MeTLContentToolTip target = new MeTLContentToolTip();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ContentElement
        ///</summary>
        [TestMethod()]
        public void ContentElementTest()
        {
            MeTLContentToolTip target = new MeTLContentToolTip(); // TODO: Initialize to an appropriate value
            UIElement expected = null; // TODO: Initialize to an appropriate value
            UIElement actual;
            target.ContentElement = expected;
            actual = target.ContentElement;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ContentText
        ///</summary>
        [TestMethod()]
        public void ContentTextTest()
        {
            MeTLContentToolTip target = new MeTLContentToolTip(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.ContentText = expected;
            actual = target.ContentText;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for TitleText
        ///</summary>
        [TestMethod()]
        public void TitleTextTest()
        {
            MeTLContentToolTip target = new MeTLContentToolTip(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.TitleText = expected;
            actual = target.TitleText;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
