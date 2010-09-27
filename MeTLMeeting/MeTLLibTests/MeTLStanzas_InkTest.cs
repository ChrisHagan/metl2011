using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Ink;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_InkTest and is intended
    ///to contain all MeTLStanzas_InkTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_InkTest
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
        ///A test for Ink Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_InkConstructorTest()
        {
            TargettedStroke stroke = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.Ink target = new MeTLStanzas.Ink(stroke);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Ink Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_InkConstructorTest1()
        {
            MeTLStanzas.Ink target = new MeTLStanzas.Ink();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for colorToString
        ///</summary>
        [TestMethod()]
        public void colorToStringTest()
        {
            Color color = new Color(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = MeTLStanzas.Ink.colorToString(color);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for stringToColor
        ///</summary>
        [TestMethod()]
        public void stringToColorTest()
        {
            string s = string.Empty; // TODO: Initialize to an appropriate value
            Color expected = new Color(); // TODO: Initialize to an appropriate value
            Color actual;
            actual = MeTLStanzas.Ink.stringToColor(s);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for stringToPoints
        ///</summary>
        [TestMethod()]
        public void stringToPointsTest()
        {
            string s = string.Empty; // TODO: Initialize to an appropriate value
            StylusPointCollection expected = null; // TODO: Initialize to an appropriate value
            StylusPointCollection actual;
            actual = MeTLStanzas.Ink.stringToPoints(s);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for strokeToColor
        ///</summary>
        [TestMethod()]
        public void strokeToColorTest()
        {
            Stroke s = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = MeTLStanzas.Ink.strokeToColor(s);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for strokeToPoints
        ///</summary>
        [TestMethod()]
        public void strokeToPointsTest()
        {
            Stroke s = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = MeTLStanzas.Ink.strokeToPoints(s);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Stroke
        ///</summary>
        [TestMethod()]
        public void StrokeTest()
        {
            MeTLStanzas.Ink target = new MeTLStanzas.Ink(); // TODO: Initialize to an appropriate value
            TargettedStroke expected = null; // TODO: Initialize to an appropriate value
            TargettedStroke actual;
            target.Stroke = expected;
            actual = target.Stroke;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
