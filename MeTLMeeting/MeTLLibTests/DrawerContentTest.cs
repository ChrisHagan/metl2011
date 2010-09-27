using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;
using System.Windows;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for DrawerContentTest and is intended
    ///to contain all DrawerContentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DrawerContentTest
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
        ///A test for DrawerContent Constructor
        ///</summary>
        [TestMethod()]
        public void DrawerContentConstructorTest()
        {
            DrawerContent target = new DrawerContent();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for column
        ///</summary>
        [TestMethod()]
        public void columnTest()
        {
            DrawerContent target = new DrawerContent(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.column = expected;
            actual = target.column;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for content
        ///</summary>
        [TestMethod()]
        public void contentTest()
        {
            DrawerContent target = new DrawerContent(); // TODO: Initialize to an appropriate value
            UserControl expected = null; // TODO: Initialize to an appropriate value
            UserControl actual;
            target.content = expected;
            actual = target.content;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for header
        ///</summary>
        [TestMethod()]
        public void headerTest()
        {
            DrawerContent target = new DrawerContent(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.header = expected;
            actual = target.header;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for visibility
        ///</summary>
        [TestMethod()]
        public void visibilityTest()
        {
            DrawerContent target = new DrawerContent(); // TODO: Initialize to an appropriate value
            Visibility expected = new Visibility(); // TODO: Initialize to an appropriate value
            Visibility actual;
            target.visibility = expected;
            actual = target.visibility;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
