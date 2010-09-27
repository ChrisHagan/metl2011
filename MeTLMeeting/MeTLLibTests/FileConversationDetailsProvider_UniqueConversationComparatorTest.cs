using MeTLLib.Providers.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for FileConversationDetailsProvider_UniqueConversationComparatorTest and is intended
    ///to contain all FileConversationDetailsProvider_UniqueConversationComparatorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileConversationDetailsProvider_UniqueConversationComparatorTest
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
        ///A test for UniqueConversationComparator Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void FileConversationDetailsProvider_UniqueConversationComparatorConstructorTest()
        {
            FileConversationDetailsProvider_Accessor.UniqueConversationComparator target = new FileConversationDetailsProvider_Accessor.UniqueConversationComparator();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void EqualsTest()
        {
            FileConversationDetailsProvider_Accessor.UniqueConversationComparator target = new FileConversationDetailsProvider_Accessor.UniqueConversationComparator(); // TODO: Initialize to an appropriate value
            ConversationDetails x = null; // TODO: Initialize to an appropriate value
            ConversationDetails y = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void GetHashCodeTest()
        {
            FileConversationDetailsProvider_Accessor.UniqueConversationComparator target = new FileConversationDetailsProvider_Accessor.UniqueConversationComparator(); // TODO: Initialize to an appropriate value
            ConversationDetails obj = null; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetHashCode(obj);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
