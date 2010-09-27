using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for ResourceUploaderTest and is intended
    ///to contain all ResourceUploaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ResourceUploaderTest
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
        ///A test for ResourceUploader Constructor
        ///</summary>
        [TestMethod()]
        public void ResourceUploaderConstructorTest()
        {
            ResourceUploader target = new ResourceUploader();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for uploadResource
        ///</summary>
        [TestMethod()]
        public void uploadResourceTest()
        {
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string file = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = ResourceUploader.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for uploadResource
        ///</summary>
        [TestMethod()]
        public void uploadResourceTest1()
        {
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string file = string.Empty; // TODO: Initialize to an appropriate value
            bool overwrite = false; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = ResourceUploader.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for uploadResourceToPath
        ///</summary>
        [TestMethod()]
        public void uploadResourceToPathTest()
        {
            string localFile = string.Empty; // TODO: Initialize to an appropriate value
            string remotePath = string.Empty; // TODO: Initialize to an appropriate value
            string name = string.Empty; // TODO: Initialize to an appropriate value
            bool overwrite = false; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = ResourceUploader.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for uploadResourceToPath
        ///</summary>
        [TestMethod()]
        public void uploadResourceToPathTest1()
        {
            byte[] resourceData = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string name = string.Empty; // TODO: Initialize to an appropriate value
            bool overwrite = false; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = ResourceUploader.uploadResourceToPath(resourceData, path, name, overwrite);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for uploadResourceToPath
        ///</summary>
        [TestMethod()]
        public void uploadResourceToPathTest2()
        {
            byte[] resourceData = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string name = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = ResourceUploader.uploadResourceToPath(resourceData, path, name);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for uploadResourceToPath
        ///</summary>
        [TestMethod()]
        public void uploadResourceToPathTest3()
        {
            string localFile = string.Empty; // TODO: Initialize to an appropriate value
            string remotePath = string.Empty; // TODO: Initialize to an appropriate value
            string name = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = ResourceUploader.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
