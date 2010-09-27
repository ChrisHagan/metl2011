using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for ConfigurationProviderTest and is intended
    ///to contain all ConfigurationProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConfigurationProviderTest
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
        ///A test for ConfigurationProvider Constructor
        ///</summary>
        [TestMethod()]
        public void ConfigurationProviderConstructorTest()
        {
            ConfigurationProvider target = new ConfigurationProvider();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for getMeTLPedagogyLevel
        ///</summary>
        [TestMethod()]
        public void getMeTLPedagogyLevelTest()
        {
            ConfigurationProvider target = new ConfigurationProvider(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.getMeTLPedagogyLevel();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getMeTLType
        ///</summary>
        [TestMethod()]
        public void getMeTLTypeTest()
        {
            ConfigurationProvider target = new ConfigurationProvider(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getMeTLType();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getMetlVersion
        ///</summary>
        [TestMethod()]
        public void getMetlVersionTest()
        {
            ConfigurationProvider target = new ConfigurationProvider(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getMetlVersion();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SERVER
        ///</summary>
        [TestMethod()]
        public void SERVERTest()
        {
            ConfigurationProvider target = new ConfigurationProvider(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.SERVER;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for instance
        ///</summary>
        [TestMethod()]
        public void instanceTest()
        {
            ConfigurationProvider actual;
            actual = ConfigurationProvider.instance;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
