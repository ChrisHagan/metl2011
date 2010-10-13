using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using Ninject;
using MeTLLib;
using MeTLLib.Providers.Connection;
using System.Net;

namespace MeTLLibTests
{
    [TestClass()]
    public class AuthorisationProviderIntegrationTests
    {
        private TestContext testContextInstance;
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
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void authenticateAgainstFailoverSystemTest()
        {
            string username = "eecrole";
            string password = "m0nash2008";
            bool expected = true;
            bool actual;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            actual = target.authenticateAgainstFailoverSystem(username, password);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void getEligibleGroupsTest()
        {
            string AuthcateName = "eecrole";
            string AuthcatePassword = "m0nash2008";
            //List<AuthorizedGroup> expected = new List<AuthorizedGroup> { new AuthorizedGroup("Unrestricted",""), new AuthorizedGroup { groupKey = "Office of the Deputy Vice-Chancellor (Education)", groupType = "ou" }, new AuthorizedGroup { groupKey = "Administration", groupType = "ou" }, new AuthorizedGroup { groupKey = "Staff", groupType = "ou" }, new AuthorizedGroup { groupKey = "eecrole", groupType = "username" } };
            List<AuthorizedGroup> expected = null;
            List<AuthorizedGroup> actual;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            actual = target.getEligibleGroups(AuthcateName, AuthcatePassword);
            Assert.IsTrue(TestExtensions.comparedCollection<AuthorizedGroup>(expected, actual));
        }
        [TestMethod()]
        public void isAuthenticatedAgainstLDAPTest()
        {
            string username = "eecrole";
            string password = "m0nash2008";
            bool expected = true;
            bool actual;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            actual = target.isAuthenticatedAgainstLDAP(username, password);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void isAuthenticatedAgainstWebProxyTest()
        {
            string username = "eecrole";
            string password = "m0nash2008";
            bool expected = true;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            bool actual = target.isAuthenticatedAgainstWebProxy(username, password);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void attemptAuthenticationIntegrationTest()
        {
            string username = "eecrole";
            string password = "m0nash2008";
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            Credentials expected = null;
            //Credentials expected = new Credentials { name = "eecrole", password = "m0nash2008", authorizedGroups = new List<AuthorizedGroup> { new AuthorizedGroup { groupKey = "Unrestricted", groupType = "" }, new AuthorizedGroup { groupKey = "Office of the Deputy Vice-Chancellor (Education)", groupType = "ou" }, new AuthorizedGroup { groupKey = "Administration", groupType = "ou" }, new AuthorizedGroup { groupKey = "Staff", groupType = "ou" }, new AuthorizedGroup { groupKey = "eecrole", groupType = "username" }, } };
            Credentials actual = target.attemptAuthentication(username, password);
            Assert.IsTrue(TestExtensions.comparedCollection<AuthorizedGroup>(expected.authorizedGroups, actual.authorizedGroups));
            Assert.AreEqual(expected.name, actual.name);
            Assert.AreEqual(expected.password, actual.password);
        }
    }
    
    [TestClass()]
    public class AuthorisationProviderTest
    {
        private TestContext testContextInstance;
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
        [TestMethod()]
        public void AuthorisationProviderConstructorTest()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            Assert.IsInstanceOfType(target, typeof(AuthorisationProvider));
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void attemptAuthenticationTestFailsWhenPassedNullUsername()
        {
            string username = null;
            string password = "m0nash2008";
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            Credentials expected = new Credentials { name = "", password = "", authorizedGroups = new List<AuthorizedGroup> { } }; 
            Credentials actual;
            actual = target.attemptAuthentication(username, password);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void attemptAuthenticationTestFailsWhenPassedNullPassword()
        {
            string username = "eecrole";
            string password = null;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            Credentials expected = new Credentials { name = "", password = "", authorizedGroups = new List<AuthorizedGroup> { } };
            Credentials actual;
            actual = target.attemptAuthentication(username, password);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void attemptAuthenticationTestFailsWhenPassedNullUsernameAndNullPassword()
        {
            string username = null;
            string password = null;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            Credentials expected = new Credentials { name = "", password = "", authorizedGroups = new List<AuthorizedGroup> { } };
            Credentials actual;
            actual = target.attemptAuthentication(username, password);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void isBackdoorUserTestReturnsFalseWhenGivenEecrole()
        {
            string user = "eecrole"; 
            bool expected = false; 
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<AuthorizationProviderWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            bool actual = target.isBackdoorUser(user);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void isBackdoorUserTestReturnsTrueWhenGivenAdmirable()
        {
            string user = "AdmirableEecrole";
            bool expected = true;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<AuthorizationProviderWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            bool actual = target.isBackdoorUser(user);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void isBackdoorUserTestReturnsFalseWhenGivenEmptyString()
        {
            string user = string.Empty;
            bool expected = false;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<AuthorizationProviderWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            bool actual = target.isBackdoorUser(user);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        [ExpectedException(typeof(System.NullReferenceException))]
        public void isBackdoorUserTestFailsWhenGivenNull()
        {
            string user = null;
            bool expected = true;
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<AuthorizationProviderWebClientFactory>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            bool actual = target.isBackdoorUser(user);
            Assert.AreEqual(expected, actual);
        }
        class AuthorizationProviderWebClientFactory : IWebClientFactory
        {
            public IWebClient client()
            {
                return new AuthorizationProviderWebClient();
            }
        }
        class AuthorizationProviderWebClient : IWebClient
        {
            public long getSize(Uri resource)
            {
                throw new NotImplementedException();
            }

            public bool exists(Uri resource)
            {
                throw new NotImplementedException();
            }

            public void downloadStringAsync(Uri resource)
            {
                throw new NotImplementedException();
            }

            public string downloadString(Uri resource)
            {
                throw new NotImplementedException();
            }

            public byte[] downloadData(Uri resource)
            {
                throw new NotImplementedException();
            }

            public string uploadData(Uri resource, byte[] data)
            {
                throw new NotImplementedException();
            }

            public void uploadDataAsync(Uri resource, byte[] data)
            {
                throw new NotImplementedException();
            }

            public byte[] uploadFile(Uri resource, string filename)
            {
                throw new NotImplementedException();
            }

            public void uploadFileAsync(Uri resource, string filename)
            {
                throw new NotImplementedException();
            }
        }
    }
}
