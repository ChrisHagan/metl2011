using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for ConversationDetailsTest and is intended
    ///to contain all ConversationDetailsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConversationDetailsTest
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
        ///A test for ConversationDetails Constructor
        ///</summary>
        [TestMethod()]
        public void ConversationDetailsConstructorTest()
        {
            ConversationDetails target = new ConversationDetails();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            object obj = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetBytes
        ///</summary>
        [TestMethod()]
        public void GetBytesTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.GetBytes();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for NextAvailableSlideId
        ///</summary>
        [TestMethod()]
        public void NextAvailableSlideIdTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.NextAvailableSlideId();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadXml
        ///</summary>
        [TestMethod()]
        public void ReadXmlTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            XElement doc = null; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            ConversationDetails actual;
            actual = target.ReadXml(doc);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Refresh
        ///</summary>
        [TestMethod()]
        public void RefreshTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            target.Refresh();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteXml
        ///</summary>
        [TestMethod()]
        public void WriteXmlTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            XElement expected = null; // TODO: Initialize to an appropriate value
            XElement actual;
            actual = target.WriteXml();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsValid
        ///</summary>
        [TestMethod()]
        public void IsValidTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsValid;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Jid
        ///</summary>
        [TestMethod()]
        public void JidTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Jid = expected;
            actual = target.Jid;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Permissions
        ///</summary>
        [TestMethod()]
        public void PermissionsTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            Permissions expected = null; // TODO: Initialize to an appropriate value
            Permissions actual;
            target.Permissions = expected;
            actual = target.Permissions;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Subject
        ///</summary>
        [TestMethod()]
        public void SubjectTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Subject = expected;
            actual = target.Subject;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Tag
        ///</summary>
        [TestMethod()]
        public void TagTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Tag = expected;
            actual = target.Tag;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Title
        ///</summary>
        [TestMethod()]
        public void TitleTest()
        {
            ConversationDetails target = new ConversationDetails(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Title = expected;
            actual = target.Title;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
