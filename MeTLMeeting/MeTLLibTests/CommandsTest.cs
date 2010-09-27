using MeTLLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Collections.Generic;
using System.Windows.Input;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for CommandsTest and is intended
    ///to contain all CommandsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandsTest
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
        ///A test for all
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void allTest()
        {
            IEnumerable<CompositeCommand> actual;
            actual = Commands_Accessor.all;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for HandlerCount
        ///</summary>
        [TestMethod()]
        public void HandlerCountTest()
        {
            int actual;
            actual = Commands.HandlerCount;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for which
        ///</summary>
        [TestMethod()]
        public void whichTest()
        {
            ICommand command = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = Commands.which(command);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for called
        ///</summary>
        [TestMethod()]
        public void calledTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            CompositeCommand expected = null; // TODO: Initialize to an appropriate value
            CompositeCommand actual;
            actual = Commands.called(name);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for allHandlers
        ///</summary>
        [TestMethod()]
        public void allHandlersTest()
        {
            IEnumerable<ICommand> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<ICommand> actual;
            actual = Commands.allHandlers();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UnregisterAllCommands
        ///</summary>
        [TestMethod()]
        public void UnregisterAllCommandsTest()
        {
            Commands.UnregisterAllCommands();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RequerySuggested
        ///</summary>
        [TestMethod()]
        public void RequerySuggestedTest()
        {
            Commands.RequerySuggested();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RequerySuggested
        ///</summary>
        [TestMethod()]
        public void RequerySuggestedTest1()
        {
            CompositeCommand[] commands = null; // TODO: Initialize to an appropriate value
            Commands.RequerySuggested(commands);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Requery
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void RequeryTest()
        {
            CompositeCommand command = null; // TODO: Initialize to an appropriate value
            Commands_Accessor.Requery(command);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for AllStaticCommandsAreRegistered
        ///</summary>
        [TestMethod()]
        public void AllStaticCommandsAreRegisteredTest()
        {
            Commands.AllStaticCommandsAreRegistered();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Commands Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void CommandsConstructorTest()
        {
            Commands_Accessor target = new Commands_Accessor();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
