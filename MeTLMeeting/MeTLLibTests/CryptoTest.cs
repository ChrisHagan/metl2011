using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace MeTLLibTests
{
    
    
    /// <summary>
    ///This is a test class for CryptoTest and is intended
    ///to contain all CryptoTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CryptoTest
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
        ///A test for Crypto Constructor
        ///</summary>
        [TestMethod()]
        public void CryptoConstructorTest()
        {
            Crypto target = new Crypto();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for bytestostring
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void bytestostringTest()
        {
            byte[] p = null; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = Crypto_Accessor.bytestostring(p, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for decrypt
        ///</summary>
        [TestMethod()]
        public void decryptTest()
        {
            string input = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = Crypto.decrypt(input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for decryptFromByteArray
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void decryptFromByteArrayTest()
        {
            byte[] input = null; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            byte[] Key = null; // TODO: Initialize to an appropriate value
            byte[] IV = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = Crypto_Accessor.decryptFromByteArray(input, encoding, Key, IV);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for encrypt
        ///</summary>
        [TestMethod()]
        public void encryptTest()
        {
            string input = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = Crypto.encrypt(input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for encryptToByteArray
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void encryptToByteArrayTest()
        {
            string input = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            byte[] Key = null; // TODO: Initialize to an appropriate value
            byte[] IV = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = Crypto_Accessor.encryptToByteArray(input, encoding, Key, IV);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for getLastBytes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void getLastBytesTest()
        {
            byte[] input = null; // TODO: Initialize to an appropriate value
            int numberOfBytesToGet = 0; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = Crypto_Accessor.getLastBytes(input, numberOfBytesToGet);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
