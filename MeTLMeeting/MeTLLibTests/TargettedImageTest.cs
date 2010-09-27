using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for TargettedImageTest and is intended
    ///to contain all TargettedImageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TargettedImageTest
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
        ///A test for TargettedImage Constructor
        ///</summary>
        [TestMethod()]
        public void TargettedImageConstructorTest()
        {
            TargettedImage target = new TargettedImage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for image
        ///</summary>
        [TestMethod()]
        public void imageTest()
        {
            TargettedImage target = new TargettedImage(); // TODO: Initialize to an appropriate value
            Image expected = null; // TODO: Initialize to an appropriate value
            Image actual;
            target.image = expected;
            actual = target.image;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
