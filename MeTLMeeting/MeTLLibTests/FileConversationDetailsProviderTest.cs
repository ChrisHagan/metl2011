using MeTLLib.Providers.Structure;
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
    public class FileConversationDetailsProviderIntegrationTest
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
        public void UpdateTest()
        {
            ConversationDetails proposedDetails = new ConversationDetails { };
            ConversationDetails expectedDetails = new ConversationDetails { };
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.Update(proposedDetails);
            Assert.AreEqual(actual, expectedDetails);
        }
        [TestMethod()]
        public void appendSlideTest()
        {
            String proposedTitle = String.Empty;
            ConversationDetails expectedDetails = new ConversationDetails { };
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlide(proposedTitle);
            Assert.AreEqual(actual, expectedDetails);
        }
        [TestMethod()]
        public void appendSlideAfterTest()
        {
            String proposedTitle = String.Empty;
            ConversationDetails expectedDetails = new ConversationDetails { };
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlide(proposedTitle);
            Assert.AreEqual(actual, expectedDetails);
        }
        [TestMethod()]
        public void detailsOfTest()
        {
            String conversationJid = "100";
            ConversationDetails expectedDetails = new ConversationDetails
            {
                Author = "hagand",
                Created = new DateTime(2010, 05, 18, 15, 00, 53),
                Jid = "100",
                LastAccessed = new DateTime(0001, 01, 1, 0, 0, 0),
                Permissions = new Permissions
                {
                    conversationGroup = String.Empty,
                    Label = null,
                    studentCanOpenFriends = true,
                    studentCanPublish = true,
                    usersAreCompulsorilySynced = false
                },
                Slides = new List<Slide> 
                {
                    new Slide
                    {
                        author="hagand",
                        defaultHeight=540,
                        defaultWidth=720,
                        exposed=true,
                        id=101,
                        index=0,
                        type= Slide.TYPE.SLIDE
                    },
                    new Slide
                    {
                        author="hagand",
                        defaultHeight=540,
                        defaultWidth=720,
                        exposed=false,
                        id=106,
                        index=1,
                        type= Slide.TYPE.SLIDE
                    },
                    new Slide
                    {
                        author="hagand",
                        defaultHeight=540,
                        defaultWidth=720,
                        exposed=false,
                        id=105,
                        index=2,
                        type= Slide.TYPE.SLIDE
                    },
                    new Slide
                    {
                        author="hagand",
                        defaultHeight=540,
                        defaultWidth=720,
                        exposed=false,
                        id=104,
                        index=3,
                        type= Slide.TYPE.SLIDE
                    },
                    new Slide
                    {
                        author="hagand",
                        defaultHeight=540,
                        defaultWidth=720,
                        exposed=false,
                        id=102,
                        index=4,
                        type= Slide.TYPE.SLIDE
                    },
                    new Slide
                    {
                        author="hagand",
                        defaultHeight=540,
                        defaultWidth=720,
                        exposed=false,
                        id=103,
                        index=5,
                        type= Slide.TYPE.SLIDE
                    },
                },
                Subject = "Unrestricted",
                Tag = "",
                Title = "Welcome to Staging on Madam"
            };
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.DetailsOf(conversationJid);
            Assert.IsTrue(TestExtensions.deepComparedObject<ConversationDetails>(actual, expectedDetails));
        }
        [TestMethod()]
        public void CreateTest()
        {
            ConversationDetails proposedConversationDetails = new ConversationDetails { };
            ConversationDetails expectedDetails = new ConversationDetails { };
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.Create(proposedConversationDetails);
            Assert.AreEqual(actual, expectedDetails);
        }
        [TestMethod()]
        public void getApplicationInformationTest()
        {
            //Don't know what ApplicationLevelInformation to expect yet.
            //ApplicationLevelInformation expected = new ApplicationLevelInformation { };
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<MadamServerAddress>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ApplicationLevelInformation actual = provider.GetApplicationLevelInformation();
            Assert.IsInstanceOfType(actual, typeof(ApplicationLevelInformation));
            //Assert.AreEqual(actual, expected);
        }
    }

    [TestClass()]
    public class FileConversationDetailsProviderTest
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
        public void FileConversationDetailsProviderConstructorTest()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            Assert.IsInstanceOfType(provider, typeof(FileConversationDetailsProvider));
        }

        [TestMethod()]
        public void AppendSlideTest()
        {
            string title = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlide(title);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        public void AppendSlideAfterTest()
        {
            int currentSlide = 0; // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlideAfter(currentSlide, title);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        public void AppendSlideAfterTestSpecifyingType()
        {
            int currentSlide = 0; // TODO: Initialize to an appropriate value
            string title = string.Empty; // TODO: Initialize to an appropriate value
            Slide.TYPE type = new Slide.TYPE(); // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlideAfter(currentSlide, title, type);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        public void CreateTest()
        {
            ConversationDetails proposedDetails = new ConversationDetails { }; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.Create(proposedDetails);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void DetailsOfTest()
        {
            String conversationJid = String.Empty; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.DetailsOf(conversationJid);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        public void GetApplicationLevelInformationTest()
        {
            ApplicationLevelInformation expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ApplicationLevelInformation actual = provider.GetApplicationLevelInformation();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        public void ListConversationsTest()
        {
            IEnumerable<ConversationDetails> expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            IEnumerable<ConversationDetails> actual = provider.ListConversations();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ReceiveDirtyConversationDetailsTest()
        {
            string jid = string.Empty; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            provider.ReceiveDirtyConversationDetails(jid);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
        /*
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void RestrictToAccessibleTest()
        {
            IEnumerable<ConversationDetails> summary = null; // TODO: Initialize to an appropriate value
            IEnumerable<string> myGroups = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            List<ConversationDetails> expected = null; // TODO: Initialize to an appropriate value
            List<ConversationDetails> actual = provider.RestrictToAccessible(summary, myGroups);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        */
        [TestMethod()]
        public void UpdateTest()
        {
            ConversationDetails details = null; // TODO: Initialize to an appropriate value
            ConversationDetails expected = null; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<FileConversationDetailsProviderWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<FileConversationDetailsResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.Update(details);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        /*
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void getPositionTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            List<Slide> slides = null; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            int actual = provider.getPosition(slide, slides);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void NEXT_AVAILABLE_IDTest()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            string actual = provider.NEXT_AVAILABLE_ID;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ROOT_ADDRESSTest()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            string actual = provider.ROOT_ADDRESS;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
         */
    }
    public class FileConversationDetailsProviderWebClientFactory : IWebClientFactory
    {
        public IWebClient client()
        {
            return new FileConversationDetailsProviderWebClient();
        }
    }
    public class FileConversationDetailsProviderWebClient : IWebClient
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
    public class FileConversationDetailsResourceUploader : IResourceUploader
    {
        public string uploadResource(string path, string file)
        {
            throw new NotImplementedException();
        }

        public string uploadResource(string path, string file, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public string uploadResourceToPath(byte[] data, string file, string name)
        {
            throw new NotImplementedException();
        }

        public string uploadResourceToPath(byte[] data, string file, string name, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public string uploadResourceToPath(string localFile, string remotePath, string name)
        {
            throw new NotImplementedException();
        }

        public string uploadResourceToPath(string localFile, string remotePath, string name, bool overwrite)
        {
            throw new NotImplementedException();
        }
    }
}
