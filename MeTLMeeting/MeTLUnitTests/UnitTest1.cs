using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SandRibbon;

namespace MeTLUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private static Window1 metlWindow;

        [TestMethod]
        public void TestMethod1()
        {
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext testContext)
        {
            metlWindow = new Window1();
            //metlWindow.Show();
        }
    }
}
