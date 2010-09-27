using MeTLLib.Providers.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for IConversationDetailsProviderTest and is intended
    ///to contain all IConversationDetailsProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IConversationDetailsProviderTest
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


        internal virtual IConversationDetailsProvider CreateIConversationDetailsProvider()
        {
            // TODO: Instantiate an appropriate concrete class.
            IConversationDetailsProvider target = null;
            return target;
        }

        /// <summary>
        ///A test for AppendSlide
        ///</summary>
        [TestMethod()]
        public void AppendSlideTest()
        {
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
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
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            int slideId = 0; // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.AppendSlideAfter(slideId, title);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AppendSlideAfter
        ///</summary>
        [TestMethod()]
        public void AppendSlideAfterTest1()
        {
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            int slideId = 0; // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            Slide.TYPE type = new Slide.TYPE(); // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.AppendSlideAfter(slideId, title, type);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CreateTest()
        {
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
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
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            string jid = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.DetailsOf(jid);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetApplicationLevelInformation
        ///</summary>
        [TestMethod()]
        public void GetApplicationLevelInformationTest()
        {
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
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
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            IEnumerable<ConversationDetails> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<ConversationDetails> actual;
            actual = target.ListConversations();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            IConversationDetailsProvider target = CreateIConversationDetailsProvider(); // TODO: Initialize to an appropriate value
            ConversationDetails details = null; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.Update(details);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
