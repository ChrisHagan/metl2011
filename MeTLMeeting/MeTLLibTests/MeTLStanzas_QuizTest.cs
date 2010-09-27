using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_QuizTest and is intended
    ///to contain all MeTLStanzas_QuizTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_QuizTest
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
        ///A test for Quiz Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_QuizConstructorTest()
        {
            QuizQuestion parameters = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.Quiz target = new MeTLStanzas.Quiz(parameters);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Quiz Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_QuizConstructorTest1()
        {
            MeTLStanzas.Quiz target = new MeTLStanzas.Quiz();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for parameters
        ///</summary>
        [TestMethod()]
        public void parametersTest()
        {
            MeTLStanzas.Quiz target = new MeTLStanzas.Quiz(); // TODO: Initialize to an appropriate value
            QuizQuestion expected = null; // TODO: Initialize to an appropriate value
            QuizQuestion actual;
            target.parameters = expected;
            actual = target.parameters;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
