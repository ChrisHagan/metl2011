using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for AuthorizedGroupTest and is intended
    ///to contain all AuthorizedGroupTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AuthorizedGroupTest
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
        ///A test for AuthorizedGroup Constructor
        ///</summary>
        [TestMethod()]
        public void AuthorizedGroupConstructorTest()
        {
            string newGroupKey = string.Empty; // TODO: Initialize to an appropriate value
            string newGroupType = string.Empty; // TODO: Initialize to an appropriate value
            AuthorizedGroup target = new AuthorizedGroup(newGroupKey, newGroupType);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for AuthorizedGroup Constructor
        ///</summary>
        [TestMethod()]
        public void AuthorizedGroupConstructorTest1()
        {
            string newGroupKey = string.Empty; // TODO: Initialize to an appropriate value
            AuthorizedGroup target = new AuthorizedGroup(newGroupKey);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for AuthorizedGroup Constructor
        ///</summary>
        [TestMethod()]
        public void AuthorizedGroupConstructorTest2()
        {
            AuthorizedGroup target = new AuthorizedGroup();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            AuthorizedGroup target = new AuthorizedGroup(); // TODO: Initialize to an appropriate value
            object obj = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            AuthorizedGroup target = new AuthorizedGroup(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for groupKey
        ///</summary>
        [TestMethod()]
        public void groupKeyTest()
        {
            AuthorizedGroup target = new AuthorizedGroup(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.groupKey = expected;
            actual = target.groupKey;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for groupType
        ///</summary>
        [TestMethod()]
        public void groupTypeTest()
        {
            AuthorizedGroup target = new AuthorizedGroup(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.groupType = expected;
            actual = target.groupType;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
