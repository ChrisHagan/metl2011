﻿using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ninject;
using MeTLLib;
using System.Net;
using System.Text;

namespace MeTLLibTests
{
    [TestClass()]
    public class ResourceUploaderIntegrationTest
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
            kernel.Bind<ICredentials>().To<MeTLCredentials>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}

        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        IKernel kernel;

        /*
         * Unreliable to deploy a source file into the tests under varying contexts on people's machines
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void uploadResourceTest()
        {
            string path = "101";
            static readonly string file = @"/Wildlife.wmv";
            string expected = "https://madam.adm.monash.edu.au:1188/Resource/101/Wildlife.wmv";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, true);
            Assert.AreEqual(expected, actual);
        }
         */
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void uploadResourceToPathTest()
        {
            byte[] data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            string path = "Resource/101";
            string name = "test.test";
            string expected = "https://madam.adm.monash.edu.au:1188/Resource/101/test.test";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(data, path, name, true);
            Assert.IsTrue(actual.EndsWith(":1188/ou/Resource/101/test.test"));
        }
    }
    [TestClass()]
    public class ResourceUploaderTest
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IWebClientFactory>().To<StubWebClientFactory>().InSingletonScope();
            kernel.Bind<IResourceUploader>().To<ProductionResourceUploader>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        IKernel kernel;
        [TestMethod()]
        public void ResourceUploaderConstructorTest()
        {
            IResourceUploader target = kernel.Get<IResourceUploader>();
            Assert.IsInstanceOfType(target, typeof(ProductionResourceUploader));
        }
        public readonly static string xml = "<file url='https://nowhere.adm.monash.edu/resources/something.ext' />";
        public readonly static string secureUrl = "https://nowhere.adm.monash.edu/resources/something.ext";
        public readonly static string insecureUrl = "http://nowhere.adm.monash.edu/resources/something.ext";
        [TestMethod()]
        public void uploadResourceTest()
        {
            string path = "//whereever//";
            string file = "whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(secureUrl, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenPathIsEmpty()
        {
            string path = String.Empty;
            string file = "whatever.ext";
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenPathIsNull()
        {
            string path = null;
            string file = "whatever.ext";
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenFileIsEmpty()
        {
            string path = "//whereever//";
            string file = String.Empty;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenFileIsNull()
        {
            string path = "//whereever//";
            string file = null;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenBothFileAndPathAreEmpty()
        {
            string path = String.Empty;
            string file = String.Empty;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenBothFileAndPathAreNull()
        {
            string path = null;
            string file = null;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenBothFileIsEmptyAndPathIsNull()
        {
            string path = null;
            string file = String.Empty;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestFailsWhenBothFileIsNullAndPathIsEmpty()
        {
            string path = String.Empty;
            string file = null;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void uploadResourceTestSpecifyingOverwrite()
        {
            string path = "//whereever//";
            string file = "whatever.ext";
            bool overwrite = false;
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(secureUrl, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedEmptyPath()
        {
            string path = String.Empty;
            string file = "whatever.ext";
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedNullPath()
        {
            string path = null;
            string file = "whatever.ext";
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedEmptyFile()
        {
            string path = "//whereever//";
            string file = String.Empty;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedNullFile()
        {
            string path = "//whereever//";
            string file = null;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedEmptyPathAndEmptyFile()
        {
            string path = String.Empty;
            string file = String.Empty;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedNullPathAndNullFile()
        {
            string path = null;
            string file = null;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedEmptyFileAndNullPath()
        {
            string path = null;
            string file = String.Empty;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceTestSpecifyingOverwriteFailsWhenPassedNullFileAndEmptyPath()
        {
            string path = String.Empty;
            string file = null;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResource(path, file, overwrite);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void uploadResourceToPathTest()
        {
            string localFile = "//whereever//";
            string remotePath = "101";
            string name = "whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(insecureUrl, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestFailsWhenPassedNullLocalFile()
        {
            string localFile = null;
            string remotePath = "101";
            string name = "whatever.ext";
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestFailsWhenPassedEmptyLocalFile()
        {
            string localFile = String.Empty;
            string remotePath = "101";
            string name = "whatever.ext";
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestFailsWhenPassedEmptyRemotePath()
        {
            string localFile = "whatever.ext";
            string remotePath = String.Empty;
            string name = "whatever.ext";
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestFailsWhenPassedNullRemotePath()
        {
            string localFile = "whatever.ext";
            string remotePath = null;
            string name = "whatever.ext";
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestFailsWhenPassedEmptyName()
        {
            string localFile = "whatever.ext";
            string remotePath = "101";
            string name = String.Empty;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestFailsWhenPassedNullName()
        {
            string localFile = "whatever.ext";
            string remotePath = "101";
            string name = String.Empty;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void uploadResourceToPathTestSpecifyingOverwrite()
        {
            string localFile = "//whereever//";
            string remotePath = "101";
            string name = "whatever.ext";
            bool overwrite = false;
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(insecureUrl, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestSpecifyingOverwriteFailsWhenPassedEmptyFile()
        {
            string localFile = String.Empty;
            string remotePath = "101";
            string name = "whatever.ext";
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestSpecifyingOverwriteFailsWhenPassedNullFile()
        {
            string localFile = null;
            string remotePath = "101";
            string name = "whatever.ext";
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestSpecifyingOverwriteFailsWhenPassedEmptyRemotePath()
        {
            string localFile = "//whereever//";
            string remotePath = String.Empty;
            string name = "whatever.ext";
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestSpecifyingOverwriteFailsWhenPassedNullRemotePath()
        {
            string localFile = "//whereever//";
            string remotePath = null;
            string name = "whatever.ext";
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestSpecifyingOverwriteFailsWhenPassedEmptyName()
        {
            string localFile = "//whereever//";
            string remotePath = "101";
            string name = String.Empty;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void uploadResourceToPathTestSpecifyingOverwriteFailsWhenPassedNullName()
        {
            string localFile = "//whereever//";
            string remotePath = "101";
            string name = null;
            bool overwrite = false;
            string expected = "https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(localFile, remotePath, name, overwrite);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void uploadResourceToPathUsingByteArrayTest()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            string remotePath = "101";
            string name = "whatever.ext";
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(data, remotePath, name);
            Assert.AreEqual(insecureUrl, actual);
        }
        [TestMethod()]
        public void uploadResourceToPathUsingByteArrayTestSpecifyingOverwrite()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            string remotePath = "101";
            string name = "whatever.ext";
            bool overwrite = false;
            IResourceUploader target = kernel.Get<IResourceUploader>();
            string actual = target.uploadResourceToPath(data, remotePath, name, overwrite);
            Assert.AreEqual(insecureUrl, actual);
        }
        class ResourceUploaderStubWebClientFactory : IWebClientFactory
        {
            public IWebClient client()
            {
                return new ResourceUploaderWebClient();
            }
        }
        class ResourceUploaderWebClient : IWebClient
        {
            public long getSize(Uri resource)
            {
                throw new NotImplementedException();
            }

            public bool exists(Uri resource)
            {
                throw new NotImplementedException();
            }

            public void downloadStringAsync(Uri resource)
            {
                throw new NotImplementedException();
            }

            public string downloadString(Uri resource)
            {
                throw new NotImplementedException();
            }

            public byte[] downloadData(Uri resource)
            {
                throw new NotImplementedException();
            }

            public string uploadData(Uri resource, byte[] data)
            {
                return "<resource url=\"https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext\"/>";
            }

            public void uploadDataAsync(Uri resource, byte[] data)
            {
                throw new NotImplementedException();
            }

            public byte[] uploadFile(Uri resource, string filename)
            {
                return Encoding.UTF8.GetBytes("<resource url=\"https://nowhere.adm.monash.edu.au/Resource/101/whatever.ext\"/>");
            }

            public void uploadFileAsync(Uri resource, string filename)
            {
                throw new NotImplementedException();
            }
        }
    }
}