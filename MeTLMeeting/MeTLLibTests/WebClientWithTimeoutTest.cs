using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using MeTLLib.Providers.Connection;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for WebClientWithTimeoutTest and is intended
    ///to contain all WebClientWithTimeoutTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WebClientWithTimeoutTest
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
        ///A test for WebClientWithTimeout Constructor
        ///</summary>
        [TestMethod()]
        public void WebClientWithTimeoutConstructorTest1()
        {
            WebClientWithTimeout target = new WebClientWithTimeout();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GetWebRequest
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void GetWebRequestTest1()
        {
            WebClientWithTimeout_Accessor target = new WebClientWithTimeout_Accessor(); // TODO: Initialize to an appropriate value
            Uri address = null; // TODO: Initialize to an appropriate value
            WebRequest expected = null; // TODO: Initialize to an appropriate value
            WebRequest actual;
            actual = target.GetWebRequest(address);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for WebClientWithTimeout Constructor
        ///</summary>
        [TestMethod()]
        public void WebClientWithTimeoutConstructorTest()
        {
            WebClientWithTimeout target = new WebClientWithTimeout();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GetWebRequest
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void GetWebRequestTest()
        {
            WebClientWithTimeout_Accessor target = new WebClientWithTimeout_Accessor(); // TODO: Initialize to an appropriate value
            Uri address = null; // TODO: Initialize to an appropriate value
            WebRequest expected = null; // TODO: Initialize to an appropriate value
            WebRequest actual;
            actual = target.GetWebRequest(address);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
