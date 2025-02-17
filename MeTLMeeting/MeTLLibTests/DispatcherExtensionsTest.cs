﻿using MeTLLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Threading;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for DispatcherExtensionsTest and is intended
    ///to contain all DispatcherExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DispatcherExtensionsTest
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
        ///A test for adoptAsync
        ///</summary>
        [TestMethod()]
        public void adoptAsyncTest()
        {
            Dispatcher dispatcher = null; // TODO: Initialize to an appropriate value
            Action del = null; // TODO: Initialize to an appropriate value
            DispatcherExtensions.adoptAsync(dispatcher, del);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for adopt
        ///</summary>
        [TestMethod()]
        public void adoptTest()
        {
            Dispatcher dispatcher = null; // TODO: Initialize to an appropriate value
            Action del = null; // TODO: Initialize to an appropriate value
            DispatcherExtensions.adopt(dispatcher, del);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
