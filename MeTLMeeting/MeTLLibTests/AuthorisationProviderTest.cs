using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for AuthorisationProviderTest and is intended
    ///to contain all AuthorisationProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AuthorisationProviderTest
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
        ///A test for AuthorisationProvider Constructor
        ///</summary>
        [TestMethod()]
        public void AuthorisationProviderConstructorTest()
        {
            AuthorisationProvider target = new AuthorisationProvider();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for attemptAuthentication
        ///</summary>
        [TestMethod()]
        public void attemptAuthenticationTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            AuthorisationProvider.attemptAuthentication(username, password);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for authenticateAgainstFailoverSystem
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void authenticateAgainstFailoverSystemTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = AuthorisationProvider_Accessor.authenticateAgainstFailoverSystem(username, password);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getEligibleGroups
        ///</summary>
        [TestMethod()]
        public void getEligibleGroupsTest()
        {
            string AuthcateName = string.Empty; // TODO: Initialize to an appropriate value
            string AuthcatePassword = string.Empty; // TODO: Initialize to an appropriate value
            List<AuthorizedGroup> expected = null; // TODO: Initialize to an appropriate value
            List<AuthorizedGroup> actual;
            actual = AuthorisationProvider.getEligibleGroups(AuthcateName, AuthcatePassword);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for isAuthenticatedAgainstLDAP
        ///</summary>
        [TestMethod()]
        public void isAuthenticatedAgainstLDAPTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = AuthorisationProvider.isAuthenticatedAgainstLDAP(username, password);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for isAuthenticatedAgainstWebProxy
        ///</summary>
        [TestMethod()]
        public void isAuthenticatedAgainstWebProxyTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = AuthorisationProvider.isAuthenticatedAgainstWebProxy(username, password);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for isBackdoorUser
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void isBackdoorUserTest()
        {
            string user = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = AuthorisationProvider_Accessor.isBackdoorUser(user);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
