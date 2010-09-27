using MeTLLib.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Media;
using Divelements.SandRibbon;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for RadioButtonTest and is intended
    ///to contain all RadioButtonTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RadioButtonTest
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
        ///A test for RadioButton Constructor
        ///</summary>
        [TestMethod()]
        public void RadioButtonConstructorTest()
        {
            RadioButton target = new RadioButton();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for CollapseToMedium
        ///</summary>
        [TestMethod()]
        public void CollapseToMediumTest()
        {
            RadioButton target = new RadioButton(); // TODO: Initialize to an appropriate value
            ButtonSize expected = new ButtonSize(); // TODO: Initialize to an appropriate value
            ButtonSize actual;
            target.CollapseToMedium = expected;
            actual = target.CollapseToMedium;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CollapseToSmall
        ///</summary>
        [TestMethod()]
        public void CollapseToSmallTest()
        {
            RadioButton target = new RadioButton(); // TODO: Initialize to an appropriate value
            ButtonSize expected = new ButtonSize(); // TODO: Initialize to an appropriate value
            ButtonSize actual;
            target.CollapseToSmall = expected;
            actual = target.CollapseToSmall;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Icon
        ///</summary>
        [TestMethod()]
        public void IconTest()
        {
            RadioButton target = new RadioButton(); // TODO: Initialize to an appropriate value
            ImageSource expected = null; // TODO: Initialize to an appropriate value
            ImageSource actual;
            target.Icon = expected;
            actual = target.Icon;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for InternalButtonSize
        ///</summary>
        [TestMethod()]
        public void InternalButtonSizeTest()
        {
            RadioButton target = new RadioButton(); // TODO: Initialize to an appropriate value
            InternalButtonSize expected = new InternalButtonSize(); // TODO: Initialize to an appropriate value
            InternalButtonSize actual;
            target.InternalButtonSize = expected;
            actual = target.InternalButtonSize;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ParentActiveVariant
        ///</summary>
        [TestMethod()]
        public void ParentActiveVariantTest()
        {
            RadioButton target = new RadioButton(); // TODO: Initialize to an appropriate value
            RibbonGroupVariant expected = new RibbonGroupVariant(); // TODO: Initialize to an appropriate value
            RibbonGroupVariant actual;
            target.ParentActiveVariant = expected;
            actual = target.ParentActiveVariant;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Text
        ///</summary>
        [TestMethod()]
        public void TextTest()
        {
            RadioButton target = new RadioButton(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Text = expected;
            actual = target.Text;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
