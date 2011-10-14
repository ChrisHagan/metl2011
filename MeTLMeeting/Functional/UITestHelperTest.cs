using UITestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Automation;

namespace Functional
{
    
    
    /// <summary>
    ///This is a test class for UITestHelperTest and is intended
    ///to contain all UITestHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UITestHelperTest
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
        ///A test for UITestHelper Constructor
        ///</summary>
        [TestMethod()]
        public void UITestHelperConstructorTest()
        {
            UITestHelper target = new UITestHelper();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for WaitForControl
        ///</summary>
        [TestMethod()]
        public void WaitForControlTest()
        {
            UITestHelper target = new UITestHelper(); // TODO: Initialize to an appropriate value
            string controlAutomationId = string.Empty; // TODO: Initialize to an appropriate value
            UITestHelper.Condition loopCondition = null; // TODO: Initialize to an appropriate value
            UITestHelper.Condition returnCondition = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.WaitForControl(loopCondition, returnCondition);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for WaitForControlEnabled
        ///</summary>
        [TestMethod()]
        public void WaitForControlEnabledTest()
        {
            UITestHelper target = new UITestHelper(); // TODO: Initialize to an appropriate value
            string controlAutomationId = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.WaitForControlEnabled();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for WaitForControlNotExist
        ///</summary>
        [TestMethod()]
        public void WaitForControlNotExistTest()
        {
            UITestHelper target = new UITestHelper(); // TODO: Initialize to an appropriate value
            string controlAutomationId = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.WaitForControlNotExist();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
