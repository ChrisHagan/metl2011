using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for DateTimeFactoryTest and is intended
    ///to contain all DateTimeFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DateTimeFactoryTest
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
        ///A test for DateTimeFactory Constructor
        ///</summary>
        [TestMethod()]
        public void DateTimeFactoryConstructorTest()
        {
            DateTimeFactory target = new DateTimeFactory();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for DateTime
        ///</summary>
        [TestMethod()]
        public void DateTimeTest()
        {
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = DateTimeFactory.DateTime();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Now
        ///</summary>
        [TestMethod()]
        public void NowTest()
        {
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = DateTimeFactory.Now();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest()
        {
            string s = string.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = DateTimeFactory.Parse(s);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            DateTimeFactory target = new DateTimeFactory(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
