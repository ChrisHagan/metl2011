using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using Ninject;
using System.Text;
using MeTLLib;

namespace MeTLLibTests
{
    [TestClass()]
    public class HttpResourceProviderTest
    {
        //Screw this guy
        private TestContext testContextInstance;
        //This guy too 
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
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
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
        //Don't test constructors, this is retarded.
        [TestMethod()]
        public void HttpResourceProviderConstructorTest()
        {
        }
        //This _Accessor it's come up with is pretty cool too.  No idea how it works.  It looks static, so how can it shadow instance members?  Of which instance?
        //Oh.  Everything in ResourceProvider is static.  Did I do that?  There's an incorrect singleton in client() which is not threadsafe by the way :D
        //So, the _Accessor is exactly the proxy object we talked about, which turns the class inside out.
        //Ps these all aren't static anymore
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void NotifyStatusTest()
        {
            /*
            string status = string.Empty; // TODO: Initialize to an appropriate value
            string type = string.Empty; // TODO: Initialize to an appropriate value
            string uri = string.Empty; // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            HttpResourceProvider_Accessor.NotifyStatus(status, type, uri, filename);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
             */
        }
        /*
         * Ok, let's try for this one.  It pretty much involves all the hard bits of anything, so if we can get this we can do the rest.
         * it's got the remote file system, it's got asynchronous bullshit going on, there's plenty of action here.
         */
        [TestMethod()]
        public void securePutFileTest()
        {
            /*
             * What we're trying to do is to establish the following states and their correct behaviours:
             */
            /*
             * The upload could be successful, returning a string Uri which can be proven to represent the uploaded resource.
             */
            /*
             * The first problem that leaps out at us is that the method calls client(), which is a static method returning a singleton(ish) 
             * WebClient.
             * This looks like a prime candidate for dependency injection, especially considering that it features a medium deep dependency satisfaction
             * graph:
             * HttpResource <- (WebClient) WebClientWithTimeoutCredentials <- (ICredentials) Credentials <- (MetlUsername, MetlPassword) (Remember we talked about microtypes?)
             * Now, in test land we don't really need this guy to have any credentials so we could, if we felt like it, stop the resolution at WebClient.
             * But considering we're going to be relying on the DI framework to give the right password and stuff we should maybe give the system a little
             * test here.  Bad form to test your frameworks but sometimes you just want to be sure.  So along the way we'll make sure that an appropriate
             * test configured credentials is in our injected Client's credentials.
             */
            /*
             * We'll let all of our objects graph be determined by the test module, who is responsible for specifying our preferred resolutions.
             * This does mean that this entire library is now commited to similarly instantiating its members by using the ProductionModule we have yet
             * to write.  It can hold off on migration, but it MUST particularize resolution for the members we're ambiguating here.
             * 
             * The first thing you'll notice is that we must redesign away from statics.  That's okay, because we'll ask the DI framework to manage our
             * singleton status for us.  This merely means that once it's provided an instance of HttpResourceProvider to anyone, it must supply
             * that instance henceforth.  This is specified, as is everything else about DI, in TestModule.  The specific case of HttpResourceProvider,
             * being common to both test and prod, is specified in BaseModule, which TestModule inherits.
             * 
             * Another singleton which immediately surfaces is WebClientFactory, whose job it is to furnish the requested WebClient when client() is
             * called, appropriately configured with credentials (and, incidentally, constructing one time and elegantly taking care of that weird bug
             * you worked around).
             * 
             * Aaand five hours of hard refactoring later we've almost made everything in the entire graph injected and instance based.
             * I am a HUGE IDIOT for doing everything in statics and not considering that we might want to test later on.
             */
            IKernel kernel = new StandardKernel(new BaseModule(),new TestModule());
            HttpResourceProvider provider = kernel.Get<HttpResourceProvider>();
            Assert.AreEqual("http://nowhere.adm.monash.edu/resources/something.ext", provider.securePutFile("http://resourceServer.wherever", "something.ext"));
        }
    }
#region Stubs
    public class StubWebClientFactory : MeTLLib.Providers.Connection.IWebClientFactory {
        public IWebClient client()
        {
            return new StubWebClient();
        }
    }
    public class StubWebClient : IWebClient {
        //Normal rules about encapsulation don't apply to testing utilities.  We WANT to be able to look inside them all the time.
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
        public string uploadFile(Uri resource, string filename)
        {
            throw new NotImplementedException();
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
