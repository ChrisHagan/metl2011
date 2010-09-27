using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for MeTLStanzas_TextBoxTest and is intended
    ///to contain all MeTLStanzas_TextBoxTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MeTLStanzas_TextBoxTest
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
        ///A test for TextBox Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_TextBoxConstructorTest()
        {
            TargettedTextBox textBox = null; // TODO: Initialize to an appropriate value
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(textBox);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for TextBox Constructor
        ///</summary>
        [TestMethod()]
        public void MeTLStanzas_TextBoxConstructorTest1()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for forceEvaluation
        ///</summary>
        [TestMethod()]
        public void forceEvaluationTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            TextBox expected = null; // TODO: Initialize to an appropriate value
            TextBox actual;
            actual = target.forceEvaluation();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Box
        ///</summary>
        [TestMethod()]
        public void BoxTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            TargettedTextBox expected = null; // TODO: Initialize to an appropriate value
            TargettedTextBox actual;
            target.Box = expected;
            actual = target.Box;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for caret
        ///</summary>
        [TestMethod()]
        public void caretTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.caret = expected;
            actual = target.caret;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for color
        ///</summary>
        [TestMethod()]
        public void colorTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            Brush expected = null; // TODO: Initialize to an appropriate value
            Brush actual;
            target.color = expected;
            actual = target.color;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for decoration
        ///</summary>
        [TestMethod()]
        public void decorationTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            TextDecorationCollection expected = null; // TODO: Initialize to an appropriate value
            TextDecorationCollection actual;
            target.decoration = expected;
            actual = target.decoration;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for family
        ///</summary>
        [TestMethod()]
        public void familyTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            FontFamily expected = null; // TODO: Initialize to an appropriate value
            FontFamily actual;
            target.family = expected;
            actual = target.family;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for height
        ///</summary>
        [TestMethod()]
        public void heightTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.height = expected;
            actual = target.height;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for size
        ///</summary>
        [TestMethod()]
        public void sizeTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.size = expected;
            actual = target.size;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for style
        ///</summary>
        [TestMethod()]
        public void styleTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            FontStyle expected = new FontStyle(); // TODO: Initialize to an appropriate value
            FontStyle actual;
            target.style = expected;
            actual = target.style;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for tag
        ///</summary>
        [TestMethod()]
        public void tagTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.tag = expected;
            actual = target.tag;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for text
        ///</summary>
        [TestMethod()]
        public void textTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.text = expected;
            actual = target.text;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for weight
        ///</summary>
        [TestMethod()]
        public void weightTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            FontWeight expected = new FontWeight(); // TODO: Initialize to an appropriate value
            FontWeight actual;
            target.weight = expected;
            actual = target.weight;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for width
        ///</summary>
        [TestMethod()]
        public void widthTest()
        {
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
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
            MeTLStanzas.TextBox target = new MeTLStanzas.TextBox(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            target.y = expected;
            actual = target.y;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
