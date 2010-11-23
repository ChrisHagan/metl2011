using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeTLLib.Providers;

namespace MeTLLibTests
{
    [TestClass]
    public class FixingINodes
    {
        [TestMethod]
        public void SimpleStemming(){
            Assert.AreEqual("40", INodeFix.Stem("40101")); 
            Assert.AreEqual("00", INodeFix.Stem("40")); 
            Assert.AreEqual("00", INodeFix.Stem("4")); 
            Assert.AreEqual("00", INodeFix.Stem("400000000")); 
            Assert.AreEqual("12", INodeFix.Stem("400012345")); 
            Assert.AreEqual("00", INodeFix.Stem("4000alpha00000")); 

            Assert.AreEqual("at", INodeFix.Stem("thatguy/40101")); 
            Assert.AreEqual("0a", INodeFix.Stem("aguy/40")); 
            Assert.AreEqual("00", INodeFix.Stem("he/4")); 
            Assert.AreEqual("hc", INodeFix.Stem("authcate/40101")); 
            Assert.AreEqual("hc", INodeFix.Stem("authcate/40")); 
            Assert.AreEqual("hc", INodeFix.Stem("authcate/4")); 
            Assert.AreEqual("hc", INodeFix.Stem("authcate/400000000")); 
            Assert.AreEqual("hc", INodeFix.Stem("authcate/400012345")); 
            Assert.AreEqual("hc", INodeFix.Stem("authcate/4000alpha00000")); 
        }
        [TestMethod]
        public void StemmingBeneathPrefix() { 
            Assert.AreEqual("/Resource/40/40101/image.png", INodeFix.StemBeneath("/Resource/", "40101/image.png")); 
            Assert.AreEqual("/Resource/40/40101/image.png", INodeFix.StemBeneath("/Resource/", "/Resource/40101/image.png")); 
            Assert.AreEqual("/Resource/12/4000012345/image.png", INodeFix.StemBeneath("/Resource/", "4000012345/image.png")); 
            Assert.AreEqual("/Resource/12/4000012345/image.png", INodeFix.StemBeneath("/Resource/", "/Resource/4000012345/image.png")); 
        }
        [TestMethod]
        public void DeStemming() { 
            Assert.AreEqual("40101/image.png", INodeFix.DeStem("40101/image.png")); 
            Assert.AreEqual("40101/image.png", INodeFix.DeStem("40/40101/image.png")); 
            Assert.AreEqual("4000012345/image.png", INodeFix.DeStem("12/4000012345/image.png")); 
            Assert.AreEqual("/4000012345/image.png", INodeFix.DeStem("/12/4000012345/image.png")); 
            Assert.AreEqual("4000012345/image.png", INodeFix.DeStem("4000012345/image.png")); 
            Assert.AreEqual("/4000012345/image.png", INodeFix.DeStem("/4000012345/image.png")); 
            Assert.AreEqual("101/image.png", INodeFix.DeStem("00/101/image.png")); 
            Assert.AreEqual("/101/image.png", INodeFix.DeStem("/00/101/image.png")); 
            Assert.AreEqual("101/image.png", INodeFix.DeStem("101/image.png")); 
            Assert.AreEqual("/101/image.png", INodeFix.DeStem("/101/image.png")); 
        }
        [TestMethod]
        public void StrippingServer(){
            Assert.AreEqual("/Resource/Whatever.png", INodeFix.StripServer("http://nowhere.nothing.com/Resource/Whatever.png"));
        }
    }
}
