using MeTLLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for ClientConnectionTest and is intended
    ///to contain all ClientConnectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClientConnectionTest
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
        ///A test for username
        ///</summary>
        [TestMethod()]
        public void usernameTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.username;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for location
        ///</summary>
        [TestMethod()]
        public void locationTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            Location actual;
            actual = target.location;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for isConnected
        ///</summary>
        [TestMethod()]
        public void isConnectedTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.isConnected;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CurrentConversations
        ///</summary>
        [TestMethod()]
        public void CurrentConversationsTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            List<ConversationDetails> actual;
            actual = target.CurrentConversations;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AvailableConversations
        ///</summary>
        [TestMethod()]
        public void AvailableConversationsTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            List<ConversationDetails> actual;
            actual = target.AvailableConversations;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for setIdentity
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void setIdentityTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            Credentials c = null; // TODO: Initialize to an appropriate value
            target.setIdentity(c);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveVideo
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveVideoTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedVideo tv = null; // TODO: Initialize to an appropriate value
            target.receiveVideo(tv);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveTextBox
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveTextBoxTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedTextBox ttb = null; // TODO: Initialize to an appropriate value
            target.receiveTextBox(ttb);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveSubmission
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveSubmissionTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedSubmission ts = null; // TODO: Initialize to an appropriate value
            target.receiveSubmission(ts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveStrokes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveStrokesTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedStroke[] tsa = null; // TODO: Initialize to an appropriate value
            target.receiveStrokes(tsa);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveStroke
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveStrokeTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedStroke ts = null; // TODO: Initialize to an appropriate value
            target.receiveStroke(ts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveQuizAnswer
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveQuizAnswerTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            QuizAnswer qa = null; // TODO: Initialize to an appropriate value
            target.receiveQuizAnswer(qa);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveQuiz
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveQuizTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            QuizQuestion qq = null; // TODO: Initialize to an appropriate value
            target.receiveQuiz(qq);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveImages
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveImagesTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedImage[] tia = null; // TODO: Initialize to an appropriate value
            target.receiveImages(tia);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveImage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveImageTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedImage[] tia = null; // TODO: Initialize to an appropriate value
            target.receiveImage(tia);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveImage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveImageTest1()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedImage ti = null; // TODO: Initialize to an appropriate value
            target.receiveImage(ti);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveFileResource
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveFileResourceTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedFile tf = null; // TODO: Initialize to an appropriate value
            target.receiveFileResource(tf);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveDirtyVideo
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveDirtyVideoTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.receiveDirtyVideo(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveDirtyTextBox
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveDirtyTextBoxTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.receiveDirtyTextBox(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveDirtyStroke
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveDirtyStrokeTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.receiveDirtyStroke(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for receiveDirtyImage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void receiveDirtyImageTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.receiveDirtyImage(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for preParserAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void preParserAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            PreParser pp = null; // TODO: Initialize to an appropriate value
            target.preParserAvailable(pp);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onVideoAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onVideoAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.VideoAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onVideoAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onTextBoxAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onTextBoxAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.TextBoxAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onTextBoxAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onSubmissionAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onSubmissionAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.SubmissionAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onSubmissionAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onStrokeAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onStrokeAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.StrokeAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onStrokeAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onStatusChanged
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onStatusChangedTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.StatusChangedEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onStatusChanged(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onQuizQuestionAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onQuizQuestionAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.QuizQuestionAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onQuizQuestionAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onQuizAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onQuizAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.QuizAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onQuizAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onQuizAnswerAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onQuizAnswerAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.QuizAnswerAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onQuizAnswerAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onPreParserAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onPreParserAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.PreParserAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onPreParserAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onLiveWindowAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onLiveWindowAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.LiveWindowAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onLiveWindowAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onImageAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onImageAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.ImageAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onImageAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onFileAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onFileAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.FileAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onFileAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onDiscoAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onDiscoAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.DiscoAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onDiscoAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onDirtyVideoAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onDirtyVideoAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.DirtyElementAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onDirtyVideoAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onDirtyTextBoxAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onDirtyTextBoxAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.DirtyElementAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onDirtyTextBoxAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onDirtyStrokeAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onDirtyStrokeAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.DirtyElementAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onDirtyStrokeAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onDirtyImageAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onDirtyImageAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.DirtyElementAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onDirtyImageAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onConversationDetailsAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onConversationDetailsAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.ConversationDetailsAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onConversationDetailsAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onCommandAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onCommandAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.CommandAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onCommandAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onChatAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onChatAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.ChatAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onChatAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onBubbleAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onBubbleAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.BubbleAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onBubbleAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onAutoshapeAvailable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onAutoshapeAvailableTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            ClientConnection.AutoshapeAvailableEventArgs e = null; // TODO: Initialize to an appropriate value
            target.onAutoshapeAvailable(e);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for loggedIn
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void loggedInTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            object _unused = null; // TODO: Initialize to an appropriate value
            target.loggedIn(_unused);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for decodeUri
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void decodeUriTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            Uri uri = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.decodeUri(uri);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for attachCommandsToEvents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void attachCommandsToEventsTest()
        {
            ClientConnection_Accessor target = new ClientConnection_Accessor(); // TODO: Initialize to an appropriate value
            target.attachCommandsToEvents();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UploadAndSendVideo
        ///</summary>
        [TestMethod()]
        public void UploadAndSendVideoTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            MeTLStanzas.LocalVideoInformation lvi = null; // TODO: Initialize to an appropriate value
            target.UploadAndSendVideo(lvi);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UploadAndSendImage
        ///</summary>
        [TestMethod()]
        public void UploadAndSendImageTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            MeTLStanzas.LocalImageInformation lii = null; // TODO: Initialize to an appropriate value
            target.UploadAndSendImage(lii);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UploadAndSendFile
        ///</summary>
        [TestMethod()]
        public void UploadAndSendFileTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            MeTLStanzas.LocalFileInformation lfi = null; // TODO: Initialize to an appropriate value
            target.UploadAndSendFile(lfi);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UpdateConversationDetails
        ///</summary>
        [TestMethod()]
        public void UpdateConversationDetailsTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            ConversationDetails details = null; // TODO: Initialize to an appropriate value
            target.UpdateConversationDetails(details);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SneakOutOf
        ///</summary>
        [TestMethod()]
        public void SneakOutOfTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.SneakOutOf(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SneakInto
        ///</summary>
        [TestMethod()]
        public void SneakIntoTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.SneakInto(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendVideo
        ///</summary>
        [TestMethod()]
        public void SendVideoTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedVideo video = null; // TODO: Initialize to an appropriate value
            target.SendVideo(video);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendTextBox
        ///</summary>
        [TestMethod()]
        public void SendTextBoxTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedTextBox textbox = null; // TODO: Initialize to an appropriate value
            target.SendTextBox(textbox);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendSubmission
        ///</summary>
        [TestMethod()]
        public void SendSubmissionTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedSubmission ts = null; // TODO: Initialize to an appropriate value
            target.SendSubmission(ts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendStroke
        ///</summary>
        [TestMethod()]
        public void SendStrokeTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedStroke stroke = null; // TODO: Initialize to an appropriate value
            target.SendStroke(stroke);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendQuizQuestion
        ///</summary>
        [TestMethod()]
        public void SendQuizQuestionTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            QuizQuestion qq = null; // TODO: Initialize to an appropriate value
            target.SendQuizQuestion(qq);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendQuizAnswer
        ///</summary>
        [TestMethod()]
        public void SendQuizAnswerTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            QuizAnswer qa = null; // TODO: Initialize to an appropriate value
            target.SendQuizAnswer(qa);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendImage
        ///</summary>
        [TestMethod()]
        public void SendImageTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedImage image = null; // TODO: Initialize to an appropriate value
            target.SendImage(image);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendFile
        ///</summary>
        [TestMethod()]
        public void SendFileTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedFile tf = null; // TODO: Initialize to an appropriate value
            target.SendFile(tf);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyVideo
        ///</summary>
        [TestMethod()]
        public void SendDirtyVideoTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.SendDirtyVideo(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyTextBox
        ///</summary>
        [TestMethod()]
        public void SendDirtyTextBoxTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.SendDirtyTextBox(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyStroke
        ///</summary>
        [TestMethod()]
        public void SendDirtyStrokeTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.SendDirtyStroke(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyImage
        ///</summary>
        [TestMethod()]
        public void SendDirtyImageTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement tde = null; // TODO: Initialize to an appropriate value
            target.SendDirtyImage(tde);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RetrieveHistoryOf
        ///</summary>
        [TestMethod()]
        public void RetrieveHistoryOfTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            PreParser expected = null; // TODO: Initialize to an appropriate value
            PreParser actual;
            actual = target.RetrieveHistoryOf(room);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for MoveTo
        ///</summary>
        [TestMethod()]
        public void MoveToTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            int slide = 0; // TODO: Initialize to an appropriate value
            target.MoveTo(slide);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Disconnect
        ///</summary>
        [TestMethod()]
        public void DisconnectTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Disconnect();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        public void ConnectTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Connect(username, password);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AsyncRetrieveHistoryOf
        ///</summary>
        [TestMethod()]
        public void AsyncRetrieveHistoryOfTest()
        {
            ClientConnection target = new ClientConnection(); // TODO: Initialize to an appropriate value
            int room = 0; // TODO: Initialize to an appropriate value
            target.AsyncRetrieveHistoryOf(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ClientConnection Constructor
        ///</summary>
        [TestMethod()]
        public void ClientConnectionConstructorTest()
        {
            Uri server = null; // TODO: Initialize to an appropriate value
            ClientConnection target = new ClientConnection(server);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ClientConnection Constructor
        ///</summary>
        [TestMethod()]
        public void ClientConnectionConstructorTest1()
        {
            ClientConnection target = new ClientConnection();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
