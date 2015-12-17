using NUnit.Framework;
using System.IO;
using TestStack.White;
using TestStack.White.Factory;

namespace NonContentAutomationTesting
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {                        
            var applicationPath = Path.Combine("C:","sandbox","saintLeo","sluBeta","MeTLMeeting","SandRibbon","bin","Debug","MeTL.exe");
            var application = Application.Launch(applicationPath);            
            var window = application.GetWindow("MeTL - Server selection", InitializeOption.NoCache);         
            Assert.Pass("Your first passing test");
        }
    }
}
