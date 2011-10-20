using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;

namespace Functional
{
    [TestClass]
    public class SearchConversationTests
    {
        private UITestHelper metlWindow;
        
        [TestInitialize]
        public void Setup()
        {
            metlWindow = new UITestHelper();
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            var success = metlWindow.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);
        }
        
        [TestMethod]
        public void SearchForOwnedAndJoin()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("TEST_FindAndJoinConversationOwned");
            search.Search();

            search.JoinFirstFound();
        }

        [TestMethod]
        public void SearchForHavePermissionAndJoin()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("TEST_FindAndJoinConversationNotOwned");
            search.Search();

            search.JoinFirstFound();
        }
    }
}
