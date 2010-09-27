using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;
using MeTLLib.DataTypes;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for PreParserTest and is intended
    ///to contain all PreParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PreParserTest
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
        ///A test for PreParser Constructor
        ///</summary>
        [TestMethod()]
        public void PreParserConstructorTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ParentRoom
        ///</summary>
        [TestMethod()]
        public void ParentRoomTest()
        {
            string room = string.Empty; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = PreParser.ParentRoom(room);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReceiveCommand
        ///</summary>
        [TestMethod()]
        public void ReceiveCommandTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            target.ReceiveCommand(message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Regurgitate
        ///</summary>
        [TestMethod()]
        public void RegurgitateTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            target.Regurgitate();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ToVisual
        ///</summary>
        [TestMethod()]
        public void ToVisualTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            InkCanvas expected = null; // TODO: Initialize to an appropriate value
            InkCanvas actual;
            actual = target.ToVisual();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for actOnAutoShapeReceived
        ///</summary>
        [TestMethod()]
        public void actOnAutoShapeReceivedTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyAutoshape element = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyAutoshapeReceived(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyImageReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyImageReceivedTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyImage image = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyImageReceived(image);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyLiveWindowReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyLiveWindowReceivedTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyInk dirtyInk = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyStrokeReceived(dirtyInk);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyTextReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyTextReceivedTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyText element = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyTextReceived(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnDirtyVideoReceived
        ///</summary>
        [TestMethod()]
        public void actOnDirtyVideoReceivedTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            MeTLStanzas.DirtyVideo element = null; // TODO: Initialize to an appropriate value
            target.actOnDirtyVideoReceived(element);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnFileResource
        ///</summary>
        [TestMethod()]
        public void actOnFileResourceTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            QuizQuestion quizDetails = null; // TODO: Initialize to an appropriate value
            target.actOnQuizReceived(quizDetails);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for actOnScreenshotSubmission
        ///</summary>
        [TestMethod()]
        public void actOnScreenshotSubmissionTest()
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
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
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            TargettedVideo video = null; // TODO: Initialize to an appropriate value
            target.actOnVideoReceived(video);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for merge
        ///</summary>
        public void mergeTestHelper<T>()
            where T : PreParser
        {
            int slide = 0; // TODO: Initialize to an appropriate value
            PreParser target = new PreParser(slide); // TODO: Initialize to an appropriate value
            T otherParser = default(T); // TODO: Initialize to an appropriate value
            T expected = default(T); // TODO: Initialize to an appropriate value
            T actual;
            actual = target.merge<T>(otherParser);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void mergeTest()
        {
            Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
                    "Please call mergeTestHelper<T>() with appropriate type parameters.");
        }
    }
}
