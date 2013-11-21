using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using Ninject;
using System.Text;
using MeTLLib;
using Ninject.Modules;

namespace MeTLLibTests
{
    [TestClass()]
    public class HttpResourceProviderTest
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
        //These guys are useful for setting up fixtures.  Kind of akin to that globals situation we talked about though so avoid it unless you're starting to rock data driven testing.
        #region Additional test attributes
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            MeTLConfiguration.Load();
        }
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //I have no idea why they just couldn't call these Setup and Teardown like EVERYONE ELSE IN THE WORLD
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        #endregion
        [TestMethod()]
        public void HttpResourceProviderConstructorTest()
        {
        }
/*
        [TestMethod()]
        public void clientIsConfiguredAgainstWorkingEndpoint()
        {
            var client = new WebClientFactory(new MeTLCredentials()).client();
            var config = MeTLConfiguration.Config.Staging;
            var uri = new Uri(String.Format("{0}:{1}/{2}", config.Host, config.Port, config.AuthenticationEndpoint));
            var result = client.downloadString(uri);
            Assert.IsTrue(result.Contains("metl.monash") && result.Contains("Query string malformed"));
        }
 */
        [TestMethod()]
        public void providerCallsClientUploadFileWithCorrectlyFormattedUrl()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            Assert.AreEqual(StubWebClient.xml, provider.securePutFile(new System.Uri("http://resourceServer.wherever"), "something.ext"));
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void nullUriPassedToSecurePutFileFails()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            provider.securePutFile(null, "something.ext");
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void nullFilePassedToSecurePutFileFails()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            provider.securePutFile(new System.Uri("http://resourceServer.wherever"), null);
        }
        [TestMethod()]
        public void providerCallsClientUploadDataWithCorrectlyFormattedUrl()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            Assert.AreEqual(StubWebClient.xml, provider.securePutFile(new System.Uri("http://resourceServer.wherever"), "something.ext"));
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void nullUriPassedToSecurePutDataFails()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            provider.securePutData(null, new byte[] { });
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void nullDataPassedToSecurePutDataFails()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            provider.securePutData(new System.Uri("http://nowhere.adm.monash.edu/resources/something.exe"), null);
        }

        [TestMethod()]
        public void providerCallsClientDownloadStringWithCorrectlyFormattedUrl()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            Assert.AreEqual("<type>data</type>", provider.secureGetString(new System.Uri("http://resourceServer.wherever")));
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void nullUriPassedToSecureGetStringFails()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            provider.secureGetString(null);
        }

    }
    #region Stubs
    public class StubWebClientFactory : MeTLLib.Providers.Connection.IWebClientFactory
    {
        public StubWebClientFactory()
        {
            Console.WriteLine("StubWebClientFactory");
        }
        public IWebClient client()
        {
            return new StubWebClient();
        }
    }
    public class StubWebClient : IWebClient
    {
        //Normal rules about encapsulation don't apply to testing utilities.  We WANT to be able to look inside them all the time.
        public long getSize(Uri resource)
        {
            if (resource == null) throw new ArgumentNullException("address", "Value cannot be null.");
            return (long)1000;
        }
        public bool exists(Uri resource)
        {
            if (resource == null) throw new ArgumentNullException("address", "Value cannot be null.");
            return true;
        }
        public void downloadStringAsync(Uri resource)
        {
            throw new NotImplementedException();
        }
        public string downloadString(Uri resource)
        {
            if (resource == null) throw new ArgumentNullException("address", "Value cannot be null.");
            return "<type>data</type>";
        }
        public byte[] downloadData(Uri resource)
        {
            if (resource == null) throw new ArgumentNullException("address", "Value cannot be null.");
            return new byte[] { 60, 116, 121, 112, 101, 62, 100, 97, 116, 97, 60, 47, 116, 121, 112, 101, 62 };
        }
        public readonly static string xml = "<file url='http://nowhere.adm.monash.edu/resources/something.ext' />";
        public string uploadData(Uri resource, byte[] data)
        {
            if (resource == null) throw new ArgumentNullException("address", "Value cannot be null.");
            if (data == null) throw new ArgumentNullException("data", "Value cannot be null.");
            return xml;
        }
        public void uploadDataAsync(Uri resource, byte[] data)
        {
            throw new NotImplementedException();
        }
        public byte[] uploadFile(Uri resource, string filename)
        {
            if (resource == null) throw new ArgumentNullException("address", "Value cannot be null.");
            if (filename == null) throw new ArgumentNullException("filename", "Value cannot be null.");
            return Encoding.UTF8.GetBytes(xml);
        }
        public void uploadFileAsync(Uri resource, string filename)
        {
            throw new NotImplementedException();
        }
    }
    public class StubCredentials : ICredentials
    {
        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return new NetworkCredential();
        }
    }
    #endregion
}
