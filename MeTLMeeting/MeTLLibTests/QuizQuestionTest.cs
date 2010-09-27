using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for QuizQuestionTest and is intended
    ///to contain all QuizQuestionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class QuizQuestionTest
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
        ///A test for QuizQuestion Constructor
        ///</summary>
        [TestMethod()]
        public void QuizQuestionConstructorTest()
        {
            QuizQuestion target = new QuizQuestion();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for author
        ///</summary>
        [TestMethod()]
        public void authorTest()
        {
            QuizQuestion target = new QuizQuestion(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.author = expected;
            actual = target.author;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for id
        ///</summary>
        [TestMethod()]
        public void idTest()
        {
            QuizQuestion target = new QuizQuestion(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            target.id = expected;
            actual = target.id;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for options
        ///</summary>
        [TestMethod()]
        public void optionsTest()
        {
            QuizQuestion target = new QuizQuestion(); // TODO: Initialize to an appropriate value
            List<Option> expected = null; // TODO: Initialize to an appropriate value
            List<Option> actual;
            target.options = expected;
            actual = target.options;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for question
        ///</summary>
        [TestMethod()]
        public void questionTest()
        {
            QuizQuestion target = new QuizQuestion(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.question = expected;
            actual = target.question;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for title
        ///</summary>
        [TestMethod()]
        public void titleTest()
        {
            QuizQuestion target = new QuizQuestion(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.title = expected;
            actual = target.title;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for url
        ///</summary>
        [TestMethod()]
        public void urlTest()
        {
            QuizQuestion target = new QuizQuestion(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.url = expected;
            actual = target.url;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
