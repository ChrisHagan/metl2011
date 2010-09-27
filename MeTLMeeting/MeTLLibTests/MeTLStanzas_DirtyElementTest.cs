using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_DirtyElementTest and is intended
    ///to contain all MeTLStanzas_DirtyElementTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_DirtyElementTest
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
        ///A test for DirtyElement Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_DirtyElementConstructorTest()
        {
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyElement target = new MeTLStanzas.DirtyElement(element);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for DirtyElement Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_DirtyElementConstructorTest1()
        {
            MeTLStanzas.DirtyElement target = new MeTLStanzas.DirtyElement();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for element
        ///</summary>
        [TestMethod()]
        public void elementTest()
        {
            MeTLStanzas.DirtyElement target = new MeTLStanzas.DirtyElement(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement expected = null; // TODO: Initialize to an appropriate value
            TargettedDirtyElement actual;
            target.element = expected;
            actual = target.element;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
