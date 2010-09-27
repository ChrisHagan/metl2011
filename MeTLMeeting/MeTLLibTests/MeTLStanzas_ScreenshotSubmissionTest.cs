using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_ScreenshotSubmissionTest and is intended
    ///to contain all MeTLStanzas_ScreenshotSubmissionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_ScreenshotSubmissionTest
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
        ///A test for ScreenshotSubmission Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_ScreenshotSubmissionConstructorTest()
        {
            TargettedSubmission submission = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.ScreenshotSubmission target = new MeTLStanzas.ScreenshotSubmission(submission);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScreenshotSubmission Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_ScreenshotSubmissionConstructorTest1()
        {
            MeTLStanzas.ScreenshotSubmission target = new MeTLStanzas.ScreenshotSubmission();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for parameters
        ///</summary>
        [TestMethod()]
        public void parametersTest()
        {
            MeTLStanzas.ScreenshotSubmission target = new MeTLStanzas.ScreenshotSubmission(); // TODO: Initialize to an appropriate value
            TargettedSubmission expected = null; // TODO: Initialize to an appropriate value
            TargettedSubmission actual;
            target.parameters = expected;
            actual = target.parameters;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
