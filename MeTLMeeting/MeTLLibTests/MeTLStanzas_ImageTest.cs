using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_ImageTest and is intended
    ///to contain all MeTLStanzas_ImageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_ImageTest
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
        ///A test for Image Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_ImageConstructorTest()
        {
            TargettedImage image = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.Image target = new MeTLStanzas.Image(image);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Image Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_ImageConstructorTest1()
        {
            MeTLStanzas.Image target = new MeTLStanzas.Image();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GetCachedImage
        ///</summary>
        [TestMethod()]
        public void GetCachedImageTest()
        {
            string url = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = MeTLStanzas.Image.GetCachedImage(url);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for forceEvaluation
        ///</summary>
        [TestMethod()]
        public void forceEvaluationTest()
        {
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
            Image expected = null; // TODO: Initialize to an appropriate value
            Image actual;
            actual = target.forceEvaluation();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Img
        ///</summary>
        [TestMethod()]
        public void ImgTest()
        {
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
            TargettedImage expected = null; // TODO: Initialize to an appropriate value
            TargettedImage actual;
            target.Img = expected;
            actual = target.Img;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for height
        ///</summary>
        [TestMethod()]
        public void heightTest()
        {
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
            ImageSource expected = null; // TODO: Initialize to an appropriate value
            ImageSource actual;
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
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.width = expected;
            actual = target.width;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for x
        ///</summary>
        [TestMethod()]
        public void xTest()
        {
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.x = expected;
            actual = target.x;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for y
        ///</summary>
        [TestMethod()]
        public void yTest()
        {
            MeTLStanzas.Image target = new MeTLStanzas.Image(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.y = expected;
            actual = target.y;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
