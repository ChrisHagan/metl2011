using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using System.Diagnostics;
using System.Threading;

namespace Functional
{
    [TestClass]
    public class ConversationEditingInMultipleUserSpace
    {
        private static Dictionary<String,AutomationElement> windows = new Dictionary<String,AutomationElement>();
        private static string[] users = new string[] {"Bill","Ben"};
        public ConversationEditingInMultipleUserSpace()
        {
        }
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {
            AutomationElementCollection windowsSoFar = null;
            while (true)
            {
                windowsSoFar = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "ribbonWindow"));
                if (windowsSoFar.Count >= users.Length)
                    break;
                Thread.Sleep(250);
            }
            for(int i =0;i<users.Length;i++)
                windows.Add(users[i], windowsSoFar[i]);
            eachUser((name,window)=>new Login(window).username(string.Format("Admirable{0}", name)).password("password"));
            eachUser((name,window)=>new Login(window).submit());
        }
        private static void eachUser(Action<string,AutomationElement> action)
        {
            var keys = windows.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
                action(key, windows[key]);
        }
        [ClassCleanup()]
        public static void MyClassCleanup() { 
        }
        #endregion
        [TestMethod]
        public void BillModifiesBensConversation()
        {
            new ApplicationPopup(windows["Bill"]).AllConversations().enter("Bill's Conversation");
            new ApplicationPopup(windows["Ben"]).AllConversations().enter("Ben's Conversation");
            Assert.AreEqual("lecture", new PresentationSpace(windows["Bill"]).Style());
            Assert.AreEqual("tutorial", new PresentationSpace(windows["Ben"]).Style());
        }
    }
}
