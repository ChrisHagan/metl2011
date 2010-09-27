using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for ResourceCacheTest and is intended
    ///to contain all ResourceCacheTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ResourceCacheTest
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
        ///A test for ResourceCache Constructor
        ///</summary>
        [TestMethod()]
        public void ResourceCacheConstructorTest()
        {
            ResourceCache target = new ResourceCache();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void AddTest()
        {
            string remoteUri = string.Empty; // TODO: Initialize to an appropriate value
            Uri localUri = null; // TODO: Initialize to an appropriate value
            ResourceCache_Accessor.Add(remoteUri, localUri);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CleanUpCache
        ///</summary>
        [TestMethod()]
        public void CleanUpCacheTest()
        {
            ResourceCache target = new ResourceCache(); // TODO: Initialize to an appropriate value
            target.CleanUpCache();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for LocalSource
        ///</summary>
        [TestMethod()]
        public void LocalSourceTest()
        {
            Uri remoteUri = null; // TODO: Initialize to an appropriate value
            Uri expected = null; // TODO: Initialize to an appropriate value
            Uri actual;
            actual = ResourceCache.LocalSource(remoteUri);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LocalSource
        ///</summary>
        [TestMethod()]
        public void LocalSourceTest1()
        {
            string uri = string.Empty; // TODO: Initialize to an appropriate value
            Uri expected = null; // TODO: Initialize to an appropriate value
            Uri actual;
            actual = ResourceCache.LocalSource(uri);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadDictFromFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ReadDictFromFileTest()
        {
            Dictionary<string, Uri> expected = null; // TODO: Initialize to an appropriate value
            Dictionary<string, Uri> actual;
            actual = ResourceCache_Accessor.ReadDictFromFile();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for RemoteSource
        ///</summary>
        [TestMethod()]
        public void RemoteSourceTest()
        {
            Uri media = null; // TODO: Initialize to an appropriate value
            Uri expected = null; // TODO: Initialize to an appropriate value
            Uri actual;
            actual = ResourceCache.RemoteSource(media);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CacheDict
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void CacheDictTest()
        {
            Dictionary<string, Uri> expected = null; // TODO: Initialize to an appropriate value
            Dictionary<string, Uri> actual;
            ResourceCache_Accessor.CacheDict = expected;
            actual = ResourceCache_Accessor.CacheDict;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
