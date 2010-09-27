using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using agsXMPP.Xml.Dom;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for CachedHistoryProviderTest and is intended
    ///to contain all CachedHistoryProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CachedHistoryProviderTest
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
        ///A test for CachedHistoryProvider Constructor
        ///</summary>
        [TestMethod()]
        public void CachedHistoryProviderConstructorTest()
        {
            CachedHistoryProvider target = new CachedHistoryProvider();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for HandleMessage
        ///</summary>
        [TestMethod()]
        public void HandleMessageTest()
        {
            CachedHistoryProvider target = new CachedHistoryProvider(); // TODO: Initialize to an appropriate value
            string to = string.Empty; // TODO: Initialize to an appropriate value
            Element message = null; // TODO: Initialize to an appropriate value
            target.HandleMessage(to, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for isPrivateRoom
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void isPrivateRoomTest()
        {
            CachedHistoryProvider_Accessor target = new CachedHistoryProvider_Accessor(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.isPrivateRoom(room);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for measure
        ///</summary>
        public void measureTestHelper<T>()
        {
            CachedHistoryProvider_Accessor target = new CachedHistoryProvider_Accessor(); // TODO: Initialize to an appropriate value
            int acc = 0; // TODO: Initialize to an appropriate value
            T item = default(T); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.measure<T>(acc, item);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void measureTest()
        {
            measureTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for cacheSize
        ///</summary>
        [TestMethod()]
        public void cacheSizeTest()
        {
            long actual;
            actual = CachedHistoryProvider.cacheSize;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for cacheTotalSize
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void cacheTotalSizeTest()
        {
            CachedHistoryProvider_Accessor target = new CachedHistoryProvider_Accessor(); // TODO: Initialize to an appropriate value
            long actual;
            actual = target.cacheTotalSize;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
