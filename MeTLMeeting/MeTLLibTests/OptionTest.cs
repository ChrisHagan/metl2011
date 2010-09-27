using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for OptionTest and is intended
    ///to contain all OptionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OptionTest
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
        ///A test for Option Constructor
        ///</summary>
        [TestMethod()]
        public void OptionConstructorTest()
        {
            Option target = new Option();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for color
        ///</summary>
        [TestMethod()]
        public void colorTest()
        {
            Option target = new Option(); // TODO: Initialize to an appropriate value
            Color expected = new Color(); // TODO: Initialize to an appropriate value
            Color actual;
            target.color = expected;
            actual = target.color;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for correct
        ///</summary>
        [TestMethod()]
        public void correctTest()
        {
            Option target = new Option(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.correct = expected;
            actual = target.correct;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for name
        ///</summary>
        [TestMethod()]
        public void nameTest()
        {
            Option target = new Option(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.name = expected;
            actual = target.name;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for optionText
        ///</summary>
        [TestMethod()]
        public void optionTextTest()
        {
            Option target = new Option(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.optionText = expected;
            actual = target.optionText;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
