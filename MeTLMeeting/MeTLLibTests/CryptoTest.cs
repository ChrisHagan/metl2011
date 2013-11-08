using MeTLLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Linq;
using MeTLLib;

namespace MeTLLibTests
{
    [TestClass()]
    public class CryptoTest
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
        [TestMethod()]
        public void CryptoConstructorTest()
        {
            Crypto target = new Crypto();
            Assert.IsInstanceOfType(target, typeof(Crypto));
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void bytestostringTest()
        {
            byte[] p = new byte[] { 97, 32, 116, 101, 115, 116, 32, 115, 116, 114, 105, 110, 103, 32, 111, 102, 32, 51, 50, 32, 99, 104, 97, 114, 97, 99, 116, 101, 114, 115 };
            Encoding encoding = Encoding.UTF8; 
            string expected = "a test string of 32 characters";
            string actual;
            actual = Crypto_Accessor.bytestostring(p, encoding);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        [ExpectedException(typeof(NullReferenceException))]
        public void bytestostringTestFailsWhenPassedNull()
        {
            byte[] p = null;
            Encoding encoding = Encoding.UTF8;
            Crypto_Accessor.bytestostring(p, encoding);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void bytestostringTestReturnsEmptyStringWhenPassedEmptyArray()
        {
            byte[] p = new byte[]{};
            Encoding encoding = Encoding.UTF8;
            string expected = String.Empty;
            string actual;
            actual = Crypto_Accessor.bytestostring(p, encoding);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void decryptTest()
        {
            string input = "HTA-Vxp96TaXFfqUi5wroFufBL7Sp7q/aGU5Q2fM6VjZ61z89a9Kyg__"; // TODO: Initialize to an appropriate value
            string expected = "a test string of 32 characters"; // TODO: Initialize to an appropriate value
            string actual;
            actual = Crypto.decrypt(input);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void decryptTestReturnsEmptyStringWhenPassedNull()
        {
            string input = null;
            string expected = String.Empty;
            string actual;
            actual = Crypto.decrypt(input);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void decryptTestReturnsEmptyStringWhenPassedEmptyString()
        {
            string input = String.Empty;
            string expected = String.Empty;
            string actual;
            actual = Crypto.decrypt(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void decryptFromByteArrayTest()
        {
            byte[] input = new byte[] { 29, 48, 62, 87, 26, 125, 233, 54, 151, 21, 250, 148, 139, 156, 43, 160, 91, 159, 4, 190, 210, 167, 186, 191, 104, 101, 57, 67, 103, 204, 233, 88 };
            Encoding encoding = Encoding.UTF8;
            byte[] Key = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            byte[] IV = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            string expected = "a test string of 32 characters\0\0"; 
            string actual;
            actual = Crypto_Accessor.decryptFromByteArray(input, encoding, Key, IV);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void decryptFromByteArrayTestFailsWhenPassedNull()
        {
            byte[] input = null;
            Encoding encoding = Encoding.UTF8;
            byte[] Key = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            byte[] IV = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            Crypto_Accessor.decryptFromByteArray(input, encoding, Key, IV);
        }
        [TestMethod()]
        public void encryptTest()
        {
            string input = "a test string of 32 characters"; // TODO: Initialize to an appropriate value
            string expected = "HTA-Vxp96TaXFfqUi5wroFufBL7Sp7q/aGU5Q2fM6VjZ61z89a9Kyg__"; // TODO: Initialize to an appropriate value
            string actual;
            actual = Crypto.encrypt(input);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void encryptTestReturnsAnEmptyStringWhenPassedAnEmptyString()
        {
            string input = String.Empty;
            string expected = String.Empty;
            string actual;
            actual = Crypto.encrypt(input);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void encryptTestReturnsEmptyStringWhenPassedANullString()
        {
            string input = null;
            string expected = String.Empty;
            string actual;
            actual = Crypto.encrypt(input);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void encryptToByteArrayTest()
        {
            string input = "a test string of 32 characters"; 
            Encoding encoding = Encoding.UTF8;
            byte[] Key = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            byte[] IV = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            byte[] expected = new byte[] { 29, 48, 62, 87, 26, 125, 233, 54, 151, 21, 250, 148, 139, 156, 43, 160, 91, 159, 4, 190, 210, 167, 186, 191, 104, 101, 57, 67, 103, 204, 233, 88 };
            byte[] actual;
            actual = Crypto_Accessor.encryptToByteArray(input, encoding, Key, IV);
            Assert.IsTrue(TestExtensions.comparedCollection<byte>(expected.ToList(), actual.ToList()));
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void encryptToByteArrayTestReturnsEmptyArrayWhenPassedEmptyString()
        {
            string input = String.Empty; 
            Encoding encoding = Encoding.UTF8;
            byte[] Key = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            byte[] IV = new byte[] { 48, 49, 50, 51, 52, 53, 54, 55 };
            byte[] expected = new byte[] {};
            byte[] actual;
            actual = Crypto_Accessor.encryptToByteArray(input, encoding, Key, IV);
            Assert.IsTrue(TestExtensions.comparedCollection<byte>(expected.ToList(), actual.ToList()));
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void getLastBytesTest()
        {
            byte[] input = new byte[] { 29, 48, 62, 87, 26, 125, 233, 54, 151, 21, 250, 148, 139, 156, 43, 160, 91, 159, 4, 190, 210, 167, 186, 191, 104, 101, 57, 67, 103, 204, 233, 88 };
            int numberOfBytesToGet = 8;
            byte[] expected = new byte[] { 104, 101, 57, 67, 103, 204, 233, 88 };                
            byte[] actual;
            actual = Crypto_Accessor.getLastBytes(input, numberOfBytesToGet);
            Assert.IsTrue(TestExtensions.comparedCollection<byte>(expected.ToList(),actual.ToList()));
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        [ExpectedException(typeof(NullReferenceException))]
        public void getLastBytesFailsWhenPassedNullByteArray()
        {
            byte[] input = null;
            int numberOfBytesToGet = 8;
            Crypto_Accessor.getLastBytes(input, numberOfBytesToGet);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void getLastBytesReturnsEmptyArrayWhenPassedZeroAsNumberOfBytes()
        {
            byte[] input = new byte[] { 29, 48, 62, 87, 26, 125, 233, 54, 151, 21, 250, 148, 139, 156, 43, 160, 91, 159, 4, 190, 210, 167, 186, 191, 104, 101, 57, 67, 103, 204, 233, 88 };
            int numberOfBytesToGet = 0;
            byte[] expected = new byte[] {};
            byte[] actual;
            actual = Crypto_Accessor.getLastBytes(input, numberOfBytesToGet);
            Assert.IsTrue(TestExtensions.comparedCollection<byte>(expected.ToList(), actual.ToList()));
        }
    }
}
