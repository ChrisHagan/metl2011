using MeTLLib.Providers.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using Ninject;
using MeTLLib;
using MeTLLib.Providers.Connection;
using System.Net;
using System.Linq;

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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            MeTLConfiguration.Load();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        [TestInitialize()]
        public void MyTestInitialize()
        {
            kernel = new StandardKernel(new BaseModule());
            kernel.Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
            kernel.Bind<MeTLGenericAddress>().To<DummyGenericAddress>().InSingletonScope();
            kernel.Bind<IReceiveEvents>().To<DummyReceiveEvents>().InSingletonScope();
            kernel.Bind<JabberWireFactory>().To<JabberWireFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            //This is doubled here to override an internal call.  Only DetailsOf is overridden, the rest of the functionality is under test.
            kernel.Bind<FileConversationDetailsProvider>().To<StubFileConversationDetailsProvider>().InSingletonScope();
            kernel.Get<JabberWireFactory>().credentials = new Credentials("", "", new List<AuthorizedGroup>(), "");
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        IKernel kernel;
        [TestMethod()]
        public void UpdateTest()
        {
            ConversationDetails proposedDetails = new ConversationDetails
            (
                "Welcome to Staging on Madam",
                "100",
                "hagand",
                "",
                new List<Slide> 
                {
                    new Slide(101,"hagand",Slide.TYPE.SLIDE,0,720,540),
                    new Slide(106,"hagand",Slide.TYPE.SLIDE,1,720,540),
                    new Slide(105,"hagand",Slide.TYPE.SLIDE,2,720,540),
                    new Slide(104,"hagand",Slide.TYPE.SLIDE,3,720,540),
                    new Slide(102,"hagand",Slide.TYPE.SLIDE,4,720,540),
                    new Slide(103,"hagand",Slide.TYPE.SLIDE,5,720,540)
                },
                new Permissions(null, true, true, false),
                "Unrestricted",
                new DateTime(2010, 05, 18, 15, 00, 53),
                new DateTime(0001, 01, 1, 0, 0, 0)
            );
            ConversationDetails expectedDetails = new ConversationDetails
            (
                "Welcome to Staging on Madam",
                "100",
                "hagand",
                "",
                new List<Slide> 
                {
                    new Slide(101,"hagand",Slide.TYPE.SLIDE,0,720,540),
                    new Slide(106,"hagand",Slide.TYPE.SLIDE,1,720,540),
                    new Slide(105,"hagand",Slide.TYPE.SLIDE,2,720,540),
                    new Slide(104,"hagand",Slide.TYPE.SLIDE,3,720,540),
                    new Slide(102,"hagand",Slide.TYPE.SLIDE,4,720,540),
                    new Slide(103,"hagand",Slide.TYPE.SLIDE,5,720,540)
                },
                new Permissions(null, true, true, false),
                "Unrestricted",
                new DateTime(2010, 05, 18, 15, 00, 53),
                new DateTime(0001, 01, 1, 0, 0, 0)
            );
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.Update(proposedDetails);
            Assert.AreEqual(actual, expectedDetails);
        }
        [TestMethod()]
        public void appendSlideTest()
        {
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlide("title");
            Assert.AreEqual("1,2,3", String.Join(",",actual.Slides.Select(s=>s.id)));
        }
        [TestMethod()]
        public void appendSlideAfterTest()
        {
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlideAfter(1,"title");
            Assert.AreEqual("1,3,2", String.Join(",",actual.Slides.Select(s=>s.id)));
        }
        [TestMethod()]
        public void detailsOfTest()
        {
            String conversationJid = "100";
            ConversationDetails expectedDetails = new ConversationDetails
            (
                "Welcome to Staging on Madam",
                "100",
                "hagand",
                "",
                new List<Slide> 
                {
                    new Slide(101,"hagand",Slide.TYPE.SLIDE,0,720,540),
                    new Slide(106,"hagand",Slide.TYPE.SLIDE,1,720,540),
                    new Slide(105,"hagand",Slide.TYPE.SLIDE,2,720,540),
                    new Slide(104,"hagand",Slide.TYPE.SLIDE,3,720,540),
                    new Slide(102,"hagand",Slide.TYPE.SLIDE,4,720,540),
                    new Slide(103,"hagand",Slide.TYPE.SLIDE,5,720,540)
                },
                new Permissions(null, true, true, false),
                "Unrestricted",
                new DateTime(2010, 05, 18, 15, 00, 53),
                new DateTime(0001, 01, 1, 0, 0, 0)
            );
            kernel.Unbind<FileConversationDetailsProvider>();
            kernel.Bind<FileConversationDetailsProvider>().To<FileConversationDetailsProvider>().InSingletonScope();
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.DetailsOf(conversationJid);
            Assert.AreEqual(expectedDetails.Title, actual.Title);
            Assert.AreEqual(expectedDetails.Jid, actual.Jid);
            Assert.AreEqual(expectedDetails.Slides.Count, actual.Slides.Count);
        }
        [TestMethod()]
        public void CreateTest()
        {
            ConversationDetails proposedConversationDetails = new ConversationDetails();
            ConversationDetails expectedDetails = null;
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.Create(proposedConversationDetails);
            Assert.AreEqual(1, actual.Slides.Count);
        }
        [TestMethod()]
        public void getApplicationInformationTest()
        {
            //Don't know what ApplicationLevelInformation to expect yet.
            //ApplicationLevelInformation expected = new ApplicationLevelInformation { };
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ApplicationLevelInformation actual = provider.GetApplicationLevelInformation();
            Assert.IsInstanceOfType(actual, typeof(ApplicationLevelInformation));
            //Assert.AreEqual(actual, expected);
        }
    
        [TestMethod()]
        public void FileConversationDetailsProviderConstructorTest()
        {
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            Assert.IsInstanceOfType(provider, typeof(FileConversationDetailsProvider));
        }

        [TestMethod()]
        public void AppendSlideTest()
        {
            var title = "title";
            FileConversationDetailsProvider provider = kernel.Get<FileConversationDetailsProvider>();
            ConversationDetails actual = provider.AppendSlide(title);
            Assert.AreEqual(title, actual.Title);
        }
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
    public class DummyReceiveEvents : IReceiveEvents
    {
        public void receivePresence(MeTLPresence presence) { }
        public void receiveSubmission(TargettedSubmission ts) { }
        public void receivesubmissions(PreParser parser) { }
        public void receiveQuiz(QuizQuestion qq) { }
        public void receiveUpdatedSlideCollection(int conversationJid) { }
        public void receiveQuizAnswer(QuizAnswer qa) { }
        public void receiveFileResource(TargettedFile tf) { }
        public void receiveStroke(TargettedStroke ts) { }
        public void receiveStrokes(TargettedStroke[] tsa) { }
        public void receiveImages(TargettedImage[] tia) { }
        public void receiveImage(TargettedImage ti) { }
        public void receiveTextBox(TargettedTextBox ttb) { }
        public void receiveDirtyStroke(TargettedDirtyElement tde) { }
        public void receiveDirtyTextBox(TargettedDirtyElement tde) { }
        public void receiveDirtyVideo(TargettedDirtyElement tde) { }
        public void receiveDirtyImage(TargettedDirtyElement tde) { }
        public void receiveMoveDelta(TargettedMoveDelta moveDelta) { }
        public void receiveChat(TargettedTextBox ttb) { }
        public void receivePreParser(PreParser pp) { }
        public void receiveLiveWindow(LiveWindowSetup lws) { }
        public void receiveDirtyLiveWindow(TargettedDirtyElement tde) { }
        public void receiveDirtyAutoShape(TargettedDirtyElement tde) { }
        public void receiveConversationDetails(ConversationDetails cd) { }
        public void statusChanged(bool isConnected, Credentials credentials) { }
        public void syncMoveRequested(int where) { }
        public void teacherStatusRequest(string where, string who) { }
        public void teacherStatusRecieved(TeacherStatus status) { }
        public void receieveQuizzes(PreParser finishedParser) { }
        public void receieveAttachments(PreParser finishedParser) { }
        public void receieveQuiz(PreParser finishedParser, long id) { }
        public event MeTLLibEventHandlers.AttachmentsAvailableRequestEventHandler AttachmentsAvailable;
        public event MeTLLibEventHandlers.QuizzesAvailableRequestEventHandler QuizzesAvailable;
        public event MeTLLibEventHandlers.QuizAvailableRequestEventHandler QuizAvailable;
        public event MeTLLibEventHandlers.TeacherStatusReceivedEventHandler TeacherStatusReceived;
        public event MeTLLibEventHandlers.TeacherStatusRequestEventHandler TeacherStatusRequest;
        public event MeTLLibEventHandlers.PresenceAvailableEventHandler PresenceAvailable;
        public event MeTLLibEventHandlers.SubmissionAvailableEventHandler SubmissionAvailable;
        public event MeTLLibEventHandlers.SubmissionsAvailableEventHandler SubmissionsAvailable;
        public event MeTLLibEventHandlers.SlideCollectionUpdatedEventHandler SlideCollectionUpdated;
        public event MeTLLibEventHandlers.FileAvailableEventHandler FileAvailable;
        public event MeTLLibEventHandlers.StatusChangedEventHandler StatusChanged;
        public event MeTLLibEventHandlers.PreParserAvailableEventHandler PreParserAvailable;
        public event MeTLLibEventHandlers.StrokeAvailableEventHandler StrokeAvailable;
        public event MeTLLibEventHandlers.ImageAvailableEventHandler ImageAvailable;
        public event MeTLLibEventHandlers.TextBoxAvailableEventHandler TextBoxAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyTextBoxAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyImageAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyVideoAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyStrokeAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyAutoShapeAvailable;
        public event MeTLLibEventHandlers.MoveDeltaAvailableEventHandler MoveDeltaAvailable;
        public event MeTLLibEventHandlers.LiveWindowAvailableEventHandler LiveWindowAvailable;
        public event MeTLLibEventHandlers.DiscoAvailableEventHandler DiscoAvailable;
        public event MeTLLibEventHandlers.QuizQuestionAvailableEventHandler QuizQuestionAvailable;
        public event MeTLLibEventHandlers.QuizAnswerAvailableEventHandler QuizAnswerAvailable;
        public event MeTLLibEventHandlers.ChatAvailableEventHandler ChatAvailable;
        public event MeTLLibEventHandlers.ConversationDetailsAvailableEventHandler ConversationDetailsAvailable;
        public event MeTLLibEventHandlers.CommandAvailableEventHandler CommandAvailable;
        public event MeTLLibEventHandlers.SyncMoveRequestedEventHandler SyncMoveRequested;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyLiveWindowAvailable;
    }
    public class DummyGenericAddress : MeTLGenericAddress
    {
    }
    class StubFileConversationDetailsProvider : FileConversationDetailsProvider
    {
        public StubFileConversationDetailsProvider(IWebClientFactory factory, IResourceUploader uploader) : base(factory, uploader) {
        }
        public override ConversationDetails DetailsOf(string id)
        {
            return new ConversationDetails("title", "10", "author", new List<Slide>{
                new Slide(1,"author",Slide.TYPE.SLIDE,0,600f,800f),
                new Slide(2,"author",Slide.TYPE.SLIDE,1,600f,800f),
            }, Permissions.LECTURE_PERMISSIONS, "subject");
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

        public string getStemmedPathForResource(string path, string name)
        {
            throw new NotImplementedException();
        }
    }
}
