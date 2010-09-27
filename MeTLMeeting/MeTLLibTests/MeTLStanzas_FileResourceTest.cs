using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_FileResourceTest and is intended
    ///to contain all MeTLStanzas_FileResourceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_FileResourceTest
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
        ///A test for FileResource Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_FileResourceConstructorTest()
        {
            TargettedFile file = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.FileResource target = new MeTLStanzas.FileResource(file);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for FileResource Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_FileResourceConstructorTest1()
        {
            MeTLStanzas.FileResource target = new MeTLStanzas.FileResource();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for fileResource
        ///</summary>
        [TestMethod()]
        public void fileResourceTest()
        {
            MeTLStanzas.FileResource target = new MeTLStanzas.FileResource(); // TODO: Initialize to an appropriate value
            TargettedFile expected = null; // TODO: Initialize to an appropriate value
            TargettedFile actual;
            target.fileResource = expected;
            actual = target.fileResource;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
