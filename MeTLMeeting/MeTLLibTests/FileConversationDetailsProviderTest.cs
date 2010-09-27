using MeTLLib.Providers.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for FileConversationDetailsProviderTest and is intended
    ///to contain all FileConversationDetailsProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileConversationDetailsProviderTest
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
        ///A test for FileConversationDetailsProvider Constructor
        ///</summary>
        [TestMethod()]
        public void FileConversationDetailsProviderConstructorTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for AppendSlide
        ///</summary>
        [TestMethod()]
        public void AppendSlideTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.AppendSlide(title);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AppendSlideAfter
        ///</summary>
        [TestMethod()]
        public void AppendSlideAfterTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            int currentSlide = 0; // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            Slide.TYPE type = new Slide.TYPE(); // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.AppendSlideAfter(currentSlide, title, type);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AppendSlideAfter
        ///</summary>
        [TestMethod()]
        public void AppendSlideAfterTest1()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            int currentSlide = 0; // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.AppendSlideAfter(currentSlide, title);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CreateTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            ConversationDetails details = null; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.Create(details);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DetailsOf
        ///</summary>
        [TestMethod()]
        public void DetailsOfTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            string conversationJid = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.DetailsOf(conversationJid);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetApplicationLevelInformation
        ///</summary>
        [TestMethod()]
        public void GetApplicationLevelInformationTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            ApplicationLevelInformation expected = null; // TODO: Initialize to an appropriate value
            ApplicationLevelInformation actual;
            actual = target.GetApplicationLevelInformation();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ListConversations
        ///</summary>
        [TestMethod()]
        public void ListConversationsTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            IEnumerable<ConversationDetails> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<ConversationDetails> actual;
            actual = target.ListConversations();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReceiveDirtyConversationDetails
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ReceiveDirtyConversationDetailsTest()
        {
            FileConversationDetailsProvider_Accessor target = new FileConversationDetailsProvider_Accessor(); // TODO: Initialize to an appropriate value
            string jid = string.Empty; // TODO: Initialize to an appropriate value
            target.ReceiveDirtyConversationDetails(jid);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RestrictToAccessible
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void RestrictToAccessibleTest()
        {
            FileConversationDetailsProvider_Accessor target = new FileConversationDetailsProvider_Accessor(); // TODO: Initialize to an appropriate value
            IEnumerable<ConversationDetails> summary = null; // TODO: Initialize to an appropriate value
            IEnumerable<string> myGroups = null; // TODO: Initialize to an appropriate value
            List<ConversationDetails> expected = null; // TODO: Initialize to an appropriate value
            List<ConversationDetails> actual;
            actual = target.RestrictToAccessible(summary, myGroups);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            FileConversationDetailsProvider target = new FileConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            ConversationDetails details = null; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.Update(details);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getPosition
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void getPositionTest()
        {
            FileConversationDetailsProvider_Accessor target = new FileConversationDetailsProvider_Accessor(); // TODO: Initialize to an appropriate value
            int slide = 0; // TODO: Initialize to an appropriate value
            List<Slide> slides = null; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.getPosition(slide, slides);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for NEXT_AVAILABLE_ID
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void NEXT_AVAILABLE_IDTest()
        {
            string actual;
            actual = FileConversationDetailsProvider_Accessor.NEXT_AVAILABLE_ID;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ROOT_ADDRESS
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ROOT_ADDRESSTest()
        {
            string actual;
            actual = FileConversationDetailsProvider_Accessor.ROOT_ADDRESS;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
