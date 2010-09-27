using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for GlobalsTest and is intended
    ///to contain all GlobalsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GlobalsTest
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
        ///A test for Globals Constructor
        ///</summary>
        [TestMethod()]
        public void GlobalsConstructorTest()
        {
            Globals target = new Globals();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for authorizedGroups
        ///</summary>
        [TestMethod()]
        public void authorizedGroupsTest()
        {
            List<AuthorizedGroup> actual;
            actual = Globals.authorizedGroups;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for conversationDetails
        ///</summary>
        [TestMethod()]
        public void conversationDetailsTest()
        {
            ConversationDetails actual;
            actual = Globals.conversationDetails;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for credentials
        ///</summary>
        [TestMethod()]
        public void credentialsTest()
        {
            Credentials actual;
            actual = Globals.credentials;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for isAuthor
        ///</summary>
        [TestMethod()]
        public void isAuthorTest()
        {
            bool actual;
            actual = Globals.isAuthor;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for location
        ///</summary>
        [TestMethod()]
        public void locationTest()
        {
            Location actual;
            actual = Globals.location;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for me
        ///</summary>
        [TestMethod()]
        public void meTest()
        {
            string actual;
            actual = Globals.me;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for slide
        ///</summary>
        [TestMethod()]
        public void slideTest()
        {
            int actual;
            actual = Globals.slide;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for slides
        ///</summary>
        [TestMethod()]
        public void slidesTest()
        {
            List<Slide> actual;
            actual = Globals.slides;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for synched
        ///</summary>
        [TestMethod()]
        public void synchedTest()
        {
            bool actual;
            actual = Globals.synched;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for teacherSlide
        ///</summary>
        [TestMethod()]
        public void teacherSlideTest()
        {
            int actual;
            actual = Globals.teacherSlide;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
