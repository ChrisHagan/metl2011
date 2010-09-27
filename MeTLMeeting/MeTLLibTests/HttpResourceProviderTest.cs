using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for HttpResourceProviderTest and is intended
    ///to contain all HttpResourceProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HttpResourceProviderTest
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
        ///A test for HttpResourceProvider Constructor
        ///</summary>
        [TestMethod()]
        public void HttpResourceProviderConstructorTest()
        {
            HttpResourceProvider target = new HttpResourceProvider();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NotifyProgress
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void NotifyProgressTest()
        {
            int attempts = 0; // TODO: Initialize to an appropriate value
            string type = string.Empty; // TODO: Initialize to an appropriate value
            string resource = string.Empty; // TODO: Initialize to an appropriate value
            long recBytes = 0; // TODO: Initialize to an appropriate value
            long size = 0; // TODO: Initialize to an appropriate value
            int percentage = 0; // TODO: Initialize to an appropriate value
            bool isPercentage = false; // TODO: Initialize to an appropriate value
            HttpResourceProvider_Accessor.NotifyProgress(attempts, type, resource, recBytes, size, percentage, isPercentage);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for NotifyStatus
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void NotifyStatusTest()
        {
            string status = string.Empty; // TODO: Initialize to an appropriate value
            string type = string.Empty; // TODO: Initialize to an appropriate value
            string uri = string.Empty; // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            HttpResourceProvider_Accessor.NotifyStatus(status, type, uri, filename);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for NotifyStatus
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void NotifyStatusTest1()
        {
            string status = string.Empty; // TODO: Initialize to an appropriate value
            string type = string.Empty; // TODO: Initialize to an appropriate value
            string uri = string.Empty; // TODO: Initialize to an appropriate value
            HttpResourceProvider_Accessor.NotifyStatus(status, type, uri);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for bypassAllCertificateStuff
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void bypassAllCertificateStuffTest()
        {
            object sender = null; // TODO: Initialize to an appropriate value
            X509Certificate cert = null; // TODO: Initialize to an appropriate value
            X509Chain chain = null; // TODO: Initialize to an appropriate value
            SslPolicyErrors error = new SslPolicyErrors(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = HttpResourceProvider_Accessor.bypassAllCertificateStuff(sender, cert, chain, error);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for client
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void clientTest()
        {
            WebClient expected = null; // TODO: Initialize to an appropriate value
            WebClient actual;
            actual = HttpResourceProvider_Accessor.client();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for configureServicePointManager
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void configureServicePointManagerTest()
        {
            HttpResourceProvider_Accessor.configureServicePointManager();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for decode
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void decodeTest()
        {
            byte[] bytes = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = HttpResourceProvider_Accessor.decode(bytes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for exists
        ///</summary>
        [TestMethod()]
        public void existsTest()
        {
            string resource = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = HttpResourceProvider.exists(resource);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getSize
        ///</summary>
        [TestMethod()]
        public void getSizeTest()
        {
            string resource = string.Empty; // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            actual = HttpResourceProvider.getSize(resource);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for insecureGetString
        ///</summary>
        [TestMethod()]
        public void insecureGetStringTest()
        {
            string resource = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = HttpResourceProvider.insecureGetString(resource);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for secureGetData
        ///</summary>
        [TestMethod()]
        public void secureGetDataTest()
        {
            string resource = string.Empty; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = HttpResourceProvider.secureGetData(resource);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for secureGetString
        ///</summary>
        [TestMethod()]
        public void secureGetStringTest()
        {
            string resource = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = HttpResourceProvider.secureGetString(resource);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for securePutData
        ///</summary>
        [TestMethod()]
        public void securePutDataTest()
        {
            string uri = string.Empty; // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = HttpResourceProvider.securePutData(uri, data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for securePutFile
        ///</summary>
        [TestMethod()]
        public void securePutFileTest()
        {
            string uri = string.Empty; // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = HttpResourceProvider.securePutFile(uri, filename);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
