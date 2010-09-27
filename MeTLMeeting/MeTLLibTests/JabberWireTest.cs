using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MeTLLib.DataTypes;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP;
using System.Collections.Generic;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for JabberWireTest and is intended
    ///to contain all JabberWireTest Unit Tests
    ///</summary>
    [TestClass()]
    public class JabberWireTest
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
        ///A test for JabberWire Constructor
        ///</summary>
        [TestMethod()]
        public void JabberWireConstructorTest()
        {
            JabberWire target = new JabberWire();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for JabberWire Constructor
        ///</summary>
        [TestMethod()]
        public void JabberWireConstructorTest1()
        {
            Credentials credentials = null; // TODO: Initialize to an appropriate value
            string SERVER = string.Empty; // TODO: Initialize to an appropriate value
            JabberWire target = new JabberWire(credentials, SERVER);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ActOnUntypedMessage
        ///</summary>
        [TestMethod()]
        public void ActOnUntypedMessageTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            Element message = null; // TODO: Initialize to an appropriate value
            target.ActOnUntypedMessage(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CanWakeUp
        ///</summary>
        [TestMethod()]
        public void CanWakeUpTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string _param = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanWakeUp(_param);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CommandBoardToMoveTo
        ///</summary>
        [TestMethod()]
        public void CommandBoardToMoveToTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string board = string.Empty; // TODO: Initialize to an appropriate value
            string slide = string.Empty; // TODO: Initialize to an appropriate value
            target.CommandBoardToMoveTo(board, slide);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ElementError
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ElementErrorTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            Element element = null; // TODO: Initialize to an appropriate value
            target.ElementError(sender, element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GetHistory
        ///</summary>
        [TestMethod()]
        public void GetHistoryTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            int where = 0; // TODO: Initialize to an appropriate value
            target.GetHistory(where);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GoToSleep
        ///</summary>
        [TestMethod()]
        public void GoToSleepTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.GoToSleep(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for HandlerError
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void HandlerErrorTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            Exception ex = null; // TODO: Initialize to an appropriate value
            target.HandlerError(sender, ex);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for IsConnected
        ///</summary>
        [TestMethod()]
        public void IsConnectedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsConnected();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for JoinConversation
        ///</summary>
        [TestMethod()]
        public void JoinConversationTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.JoinConversation(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Login
        ///</summary>
        [TestMethod()]
        public void LoginTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            Location location = null; // TODO: Initialize to an appropriate value
            target.Login(location);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Logout
        ///</summary>
        [TestMethod()]
        public void LogoutTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            target.Logout();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for LookupServer
        ///</summary>
        [TestMethod()]
        public void LookupServerTest()
        {
            JabberWire.LookupServer();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for MoveTo
        ///</summary>
        [TestMethod()]
        public void MoveToTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            int where = 0; // TODO: Initialize to an appropriate value
            target.MoveTo(where);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for OnAuthError
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void OnAuthErrorTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object _sender = null; // TODO: Initialize to an appropriate value
            Element error = null; // TODO: Initialize to an appropriate value
            target.OnAuthError(_sender, error);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for OnClose
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void OnCloseTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            target.OnClose(sender);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for OnLogin
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void OnLoginTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object o = null; // TODO: Initialize to an appropriate value
            target.OnLogin(o);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for OnMessage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void OnMessageTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            Message message = null; // TODO: Initialize to an appropriate value
            target.OnMessage(sender, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for OnPresence
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void OnPresenceTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            Element element = null; // TODO: Initialize to an appropriate value
            target.OnPresence(sender, element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ReadXml
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void ReadXmlTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            string xml = string.Empty; // TODO: Initialize to an appropriate value
            target.ReadXml(sender, xml);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ReceiveCommand
        ///</summary>
        [TestMethod()]
        public void ReceiveCommandTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.ReceiveCommand(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ReceivedMessage
        ///</summary>
        [TestMethod()]
        public void ReceivedMessageTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            object obj = null; // TODO: Initialize to an appropriate value
            target.ReceivedMessage(obj);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Reset
        ///</summary>
        [TestMethod()]
        public void ResetTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string caller = string.Empty; // TODO: Initialize to an appropriate value
            target.Reset(caller);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendAutoShape
        ///</summary>
        [TestMethod()]
        public void SendAutoShapeTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedAutoShape autoshape = null; // TODO: Initialize to an appropriate value
            target.SendAutoShape(autoshape);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendChat
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void SendChatTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            TargettedTextBox message = null; // TODO: Initialize to an appropriate value
            target.SendChat(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyAutoShape
        ///</summary>
        [TestMethod()]
        public void SendDirtyAutoShapeTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            target.SendDirtyAutoShape(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyConversationDetails
        ///</summary>
        [TestMethod()]
        public void SendDirtyConversationDetailsTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string jid = string.Empty; // TODO: Initialize to an appropriate value
            target.SendDirtyConversationDetails(jid);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyImage
        ///</summary>
        [TestMethod()]
        public void SendDirtyImageTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            target.SendDirtyImage(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyLiveWindow
        ///</summary>
        [TestMethod()]
        public void SendDirtyLiveWindowTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement dirty = null; // TODO: Initialize to an appropriate value
            target.SendDirtyLiveWindow(dirty);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyText
        ///</summary>
        [TestMethod()]
        public void SendDirtyTextTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            target.SendDirtyText(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendDirtyVideo
        ///</summary>
        [TestMethod()]
        public void SendDirtyVideoTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            target.SendDirtyVideo(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendImage
        ///</summary>
        [TestMethod()]
        public void SendImageTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedImage image = null; // TODO: Initialize to an appropriate value
            target.SendImage(image);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendLiveWindow
        ///</summary>
        [TestMethod()]
        public void SendLiveWindowTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            LiveWindowSetup window = null; // TODO: Initialize to an appropriate value
            target.SendLiveWindow(window);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendMoveBoardToSlide
        ///</summary>
        [TestMethod()]
        public void SendMoveBoardToSlideTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            BoardMove boardMove = null; // TODO: Initialize to an appropriate value
            target.SendMoveBoardToSlide(boardMove);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendNewBubble
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void SendNewBubbleTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            TargettedBubbleContext selection = null; // TODO: Initialize to an appropriate value
            target.SendNewBubble(selection);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendPing
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void SendPingTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string who = string.Empty; // TODO: Initialize to an appropriate value
            target.SendPing(who);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendQuiz
        ///</summary>
        [TestMethod()]
        public void SendQuizTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            QuizQuestion parameters = null; // TODO: Initialize to an appropriate value
            target.SendQuiz(parameters);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendQuizAnswer
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void SendQuizAnswerTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            QuizAnswer parameters = null; // TODO: Initialize to an appropriate value
            target.SendQuizAnswer(parameters);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendScreenshotSubmission
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void SendScreenshotSubmissionTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            TargettedSubmission submission = null; // TODO: Initialize to an appropriate value
            target.SendScreenshotSubmission(submission);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendStroke
        ///</summary>
        [TestMethod()]
        public void SendStrokeTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedStroke stroke = null; // TODO: Initialize to an appropriate value
            target.SendStroke(stroke);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendSyncMoveTo
        ///</summary>
        [TestMethod()]
        public void SendSyncMoveToTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            int where = 0; // TODO: Initialize to an appropriate value
            target.SendSyncMoveTo(where);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendTextbox
        ///</summary>
        [TestMethod()]
        public void SendTextboxTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedTextBox box = null; // TODO: Initialize to an appropriate value
            target.SendTextbox(box);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendVideo
        ///</summary>
        [TestMethod()]
        public void SendVideoTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedVideo video = null; // TODO: Initialize to an appropriate value
            target.SendVideo(video);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendWormMove
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void SendWormMoveTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            WormMove move = null; // TODO: Initialize to an appropriate value
            target.SendWormMove(move);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SneakInto
        ///</summary>
        [TestMethod()]
        public void SneakIntoTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.SneakInto(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SneakOutOf
        ///</summary>
        [TestMethod()]
        public void SneakOutOfTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.SneakOutOf(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SwitchServer
        ///</summary>
        [TestMethod()]
        public void SwitchServerTest()
        {
            string target = string.Empty; // TODO: Initialize to an appropriate value
            JabberWire.SwitchServer(target);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WakeUp
        ///</summary>
        [TestMethod()]
        public void WakeUpTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.WakeUp(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteXml
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void WriteXmlTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            string xml = string.Empty; // TODO: Initialize to an appropriate value
            target.WriteXml(sender, xml);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnAutoShapeReceived
        ///</summary>
        [TestMethod()]
        public void actOnAutoShapeReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedAutoShape autoshape = null; // TODO: Initialize to an appropriate value
            target.actOnAutoShapeReceived(autoshape);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnBubbleReceived
        ///</summary>
        [TestMethod()]
        public void actOnBubbleReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedBubbleContext bubble = null; // TODO: Initialize to an appropriate value
            target.actOnBubbleReceived(bubble);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyAutoshapeReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyAutoshapeReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyAutoshape dirtyAutoShape = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyAutoshapeReceived(dirtyAutoShape);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyImageReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyImageReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyImage dirtyImage = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyImageReceived(dirtyImage);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyLiveWindowReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyLiveWindowReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyLiveWindowReceived(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyStrokeReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyStrokeReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyInk element = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyStrokeReceived(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyTextReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyTextReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyText dirtyText = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyTextReceived(dirtyText);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyVideoReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyVideoReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyVideo dirtyVideo = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyVideoReceived(dirtyVideo);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnFileResource
        ///</summary>
        [TestMethod()]
        public void actOnFileResourceTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            MeTLStanzas.FileResource resource = null; // TODO: Initialize to an appropriate value
            target.actOnFileResource(resource);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnImageReceived
        ///</summary>
        [TestMethod()]
        public void actOnImageReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedImage image = null; // TODO: Initialize to an appropriate value
            target.actOnImageReceived(image);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnLiveWindowReceived
        ///</summary>
        [TestMethod()]
        public void actOnLiveWindowReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            LiveWindowSetup window = null; // TODO: Initialize to an appropriate value
            target.actOnLiveWindowReceived(window);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnQuizAnswerReceived
        ///</summary>
        [TestMethod()]
        public void actOnQuizAnswerReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            QuizAnswer answer = null; // TODO: Initialize to an appropriate value
            target.actOnQuizAnswerReceived(answer);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnQuizReceived
        ///</summary>
        [TestMethod()]
        public void actOnQuizReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            QuizQuestion quiz = null; // TODO: Initialize to an appropriate value
            target.actOnQuizReceived(quiz);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnScreenshotSubmission
        ///</summary>
        [TestMethod()]
        public void actOnScreenshotSubmissionTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedSubmission submission = null; // TODO: Initialize to an appropriate value
            target.actOnScreenshotSubmission(submission);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnStrokeReceived
        ///</summary>
        [TestMethod()]
        public void actOnStrokeReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedStroke stroke = null; // TODO: Initialize to an appropriate value
            target.actOnStrokeReceived(stroke);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnTextReceived
        ///</summary>
        [TestMethod()]
        public void actOnTextReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedTextBox box = null; // TODO: Initialize to an appropriate value
            target.actOnTextReceived(box);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnVideoReceived
        ///</summary>
        [TestMethod()]
        public void actOnVideoReceivedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedVideo video = null; // TODO: Initialize to an appropriate value
            target.actOnVideoReceived(video);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for command
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void commandTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.command(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for command
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void commandTest1()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string where = string.Empty; // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.command(where, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for createJid
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void createJidTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            Jid expected = null; // TODO: Initialize to an appropriate value
            Jid actual;
            actual = target.createJid(username);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for directCommand
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void directCommandTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string target1 = string.Empty; // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.directCommand(target1, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for dontDoAnything
        ///</summary>
        [TestMethod()]
        public void dontDoAnythingTest()
        {
            int _obj = 0; // TODO: Initialize to an appropriate value
            int _obj2 = 0; // TODO: Initialize to an appropriate value
            JabberWire.dontDoAnything(_obj, _obj2);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for dontDoAnything
        ///</summary>
        [TestMethod()]
        public void dontDoAnythingTest1()
        {
            JabberWire.dontDoAnything();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for getCurrentClasses
        ///</summary>
        [TestMethod()]
        public void getCurrentClassesTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            object _unused = null; // TODO: Initialize to an appropriate value
            target.getCurrentClasses(_unused);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleConversationDetailsUpdated
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void handleConversationDetailsUpdatedTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleConversationDetailsUpdated(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleGoToConversation
        ///</summary>
        [TestMethod()]
        public void handleGoToConversationTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleGoToConversation(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleGoToSlide
        ///</summary>
        [TestMethod()]
        public void handleGoToSlideTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleGoToSlide(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handlePing
        ///</summary>
        [TestMethod()]
        public void handlePingTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handlePing(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handlePong
        ///</summary>
        [TestMethod()]
        public void handlePongTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handlePong(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleSleep
        ///</summary>
        [TestMethod()]
        public void handleSleepTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleSleep(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleSyncMoveReceived
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void handleSyncMoveReceivedTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleSyncMoveReceived(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleUnknownMessage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void handleUnknownMessageTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.handleUnknownMessage(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleWakeUp
        ///</summary>
        [TestMethod()]
        public void handleWakeUpTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleWakeUp(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for handleWormMoved
        ///</summary>
        [TestMethod()]
        public void handleWormMovedTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string[] parts = null; // TODO: Initialize to an appropriate value
            target.handleWormMoved(parts);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for isCurrentConversation
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void isCurrentConversationTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string jid = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.isCurrentConversation(jid);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for joinRoom
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void joinRoomTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            Jid room = null; // TODO: Initialize to an appropriate value
            target.joinRoom(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for joinRooms
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void joinRoomsTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            target.joinRooms();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onProgress
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onProgressTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            int upTo = 0; // TODO: Initialize to an appropriate value
            int outOf = 0; // TODO: Initialize to an appropriate value
            target.onProgress(upTo, outOf);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for onStart
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void onStartTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            target.onStart();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for openConnection
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void openConnectionTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            target.openConnection(username);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for retrieveHistory
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void retrieveHistoryTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string room = string.Empty; // TODO: Initialize to an appropriate value
            target.retrieveHistory(room);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for send
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void sendTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            Message message = null; // TODO: Initialize to an appropriate value
            target.send(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for send
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void sendTest1()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            string target1 = string.Empty; // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.send(target1, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for sendDirtyStroke
        ///</summary>
        [TestMethod()]
        public void sendDirtyStrokeTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            TargettedDirtyElement element = null; // TODO: Initialize to an appropriate value
            target.sendDirtyStroke(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for sendFileResource
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void sendFileResourceTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            TargettedFile file = null; // TODO: Initialize to an appropriate value
            target.sendFileResource(file);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for setUpWire
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void setUpWireTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            target.setUpWire();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for stanza
        ///</summary>
        [TestMethod()]
        public void stanzaTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            string target1 = string.Empty; // TODO: Initialize to an appropriate value
            Element stanza = null; // TODO: Initialize to an appropriate value
            target.stanza(target1, stanza);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for stanza
        ///</summary>
        [TestMethod()]
        public void stanzaTest1()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            Element stanza = null; // TODO: Initialize to an appropriate value
            target.stanza(stanza);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for unregisterHandlers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void unregisterHandlersTest()
        {
            JabberWire_Accessor target = new JabberWire_Accessor(); // TODO: Initialize to an appropriate value
            target.unregisterHandlers();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CurrentClasses
        ///</summary>
        [TestMethod()]
        public void CurrentClassesTest()
        {
            JabberWire target = new JabberWire(); // TODO: Initialize to an appropriate value
            List<ConversationDetails> actual;
            actual = target.CurrentClasses;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
