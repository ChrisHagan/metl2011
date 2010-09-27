using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_VideoTest and is intended
    ///to contain all MeTLStanzas_VideoTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_VideoTest
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
        ///A test for Video Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_VideoConstructorTest()
        {
            TargettedVideo video = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.Video target = new MeTLStanzas.Video(video);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Video Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_VideoConstructorTest1()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for forceEvaluation
        ///</summary>
        [TestMethod()]
        public void forceEvaluationTest()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video(); // TODO: Initialize to an appropriate value
            Video expected = null; // TODO: Initialize to an appropriate value
            Video actual;
            actual = target.forceEvaluation();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getCachedVideo
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void getCachedVideoTest()
        {
            string url = string.Empty; // TODO: Initialize to an appropriate value
            Uri expected = null; // TODO: Initialize to an appropriate value
            Uri actual;
            actual = MeTLStanzas_Accessor.Video.getCachedVideo(url);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Vid
        ///</summary>
        [TestMethod()]
        public void VidTest()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video(); // TODO: Initialize to an appropriate value
            TargettedVideo expected = null; // TODO: Initialize to an appropriate value
            TargettedVideo actual;
            target.Vid = expected;
            actual = target.Vid;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for height
        ///</summary>
        [TestMethod()]
        public void heightTest()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.height = expected;
            actual = target.height;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for source
        ///</summary>
        [TestMethod()]
        public void sourceTest()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video(); // TODO: Initialize to an appropriate value
            Uri expected = null; // TODO: Initialize to an appropriate value
            Uri actual;
            target.source = expected;
            actual = target.source;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.tag = expected;
            actual = target.tag;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for width
        ///</summary>
        [TestMethod()]
        public void widthTest()
        {
            MeTLStanzas.Video target = new MeTLStanzas.Video(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.width = expected;
            actual = target.width;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
