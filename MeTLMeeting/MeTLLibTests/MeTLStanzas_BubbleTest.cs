using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_BubbleTest and is intended
    ///to contain all MeTLStanzas_BubbleTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_BubbleTest
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
        ///A test for Bubble Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_BubbleConstructorTest()
        {
            TargettedBubbleContext context = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.Bubble target = new MeTLStanzas.Bubble(context);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Bubble Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_BubbleConstructorTest1()
        {
            MeTLStanzas.Bubble target = new MeTLStanzas.Bubble();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for context
        ///</summary>
        [TestMethod()]
        public void contextTest()
        {
            MeTLStanzas.Bubble target = new MeTLStanzas.Bubble(); // TODO: Initialize to an appropriate value
            TargettedBubbleContext expected = null; // TODO: Initialize to an appropriate value
            TargettedBubbleContext actual;
            target.context = expected;
            actual = target.context;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
