using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonContentAutomationTesting
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            /*
            var applicationDirectory = TestContext.CurrentContext.TestDirectory;
            var applicationPath = Path.Combine(applicationDirectory, "MeTL.exe");
            var application = Application.Launch(applicationPath);
            var window = application.GetWindow("bar", InitializeOption.NoCache);
            */
            Assert.Pass("Your first passing test");
        }
    }
}
