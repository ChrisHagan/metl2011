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
        public void attemptAuthenticationIntegrationTest()
        {
            string username = "eecrole";
            string password = "m0nash2008";
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            AuthorisationProvider target = kernel.Get<AuthorisationProvider>();
            Credentials expected = new Credentials(
                "eecrole",
                "m0nash2008",
                new List<AuthorizedGroup> { 
                    new AuthorizedGroup("Unrestricted",""),
                    new AuthorizedGroup("Office of the Deputy Vice-Chancellor (Education)","ou"), 
                    new AuthorizedGroup("Administration","ou"), 
                    new AuthorizedGroup("Staff", "ou"), 
                    new AuthorizedGroup("eecrole","username"), });
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
            Credentials expected = new Credentials("","", new List<AuthorizedGroup>()); 
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
            Credentials expected = new Credentials("","",new List<AuthorizedGroup>());
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
            Credentials expected = new Credentials("","",new List<AuthorizedGroup>());
            Credentials actual;
            actual = target.attemptAuthentication(username, password);
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
