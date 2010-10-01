using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MeTLLibTests
{
    [TestClass()]
    public class MeTLStanzasTest
    {
        [TestMethod()]
        public void videoCorrectlyReconstructsCachedUrl() {
            var videoStanza = 
                new XElement("video", 
                    new XElement("something"));
        }
    }
}
