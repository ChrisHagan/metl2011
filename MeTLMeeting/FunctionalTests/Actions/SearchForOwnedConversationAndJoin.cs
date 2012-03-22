using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UITestFramework;
using Functional;
using System.Windows.Automation;
using System.Windows;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests.Actions
{
    /// <summary>
    /// Expected: Instance of MeTL has started, user has logged in successfully and is at the search conversation screen
    /// </summary>
    public class SearchConversation
    {
        public void SearchForConversationAndJoin(UITestHelper window, string conversationTitle)
        {
            var search = new ConversationSearcher(window.AutomationElement);

            search.searchField(conversationTitle);
            search.Search();

            if (!search.ResultsContainQueried(conversationTitle))
            {
                CreateAndRenameConversation(window, conversationTitle);
            }

            search.JoinQueried(conversationTitle);
        }

        public void CreateAndRenameConversation(UITestHelper window, string conversationTitle)
        {
            // create a new conversation with the name of the computer appended
            new ApplicationPopup(window.AutomationElement).CreateConversation();

            UITestHelper.Wait(TimeSpan.FromSeconds(2));

            SwitchToSearchCurrentConversation(window);

            UITestHelper.Wait(TimeSpan.FromSeconds(2));

            var edit = new ConversationEditScreen(window.AutomationElement);

            edit.Rename(conversationTitle).Save();
        }

        public void SwitchToSearchCurrentConversation(UITestHelper window)
        {
            var manualEvent = new ManualResetEvent(false);
            var completedSearch = false;

            var searchBox = new UITestHelper(window.AutomationElement);
            searchBox.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_BOX));
            searchBox.Find();

            Automation.AddAutomationEventHandler(AutomationElement.AsyncContentLoadedEvent, searchBox.AutomationElement, TreeScope.Element, (sender, args) => { completedSearch = true; manualEvent.Set(); });

            new ApplicationPopup(window.AutomationElement).SearchMyConversation();

            UITestHelper.Wait(TimeSpan.FromSeconds(5));
            
            manualEvent.WaitOne(5000, false);
            Assert.IsTrue(completedSearch);

            var currentConversation = new UITestHelper(window);
            currentConversation.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_SEARCH_CURRENT_CONVERSATION_BUTTON));

            currentConversation.Find();

            currentConversation.AutomationElement.Select();

            UITestHelper.Wait(TimeSpan.FromSeconds(5));

            /*var resultsText = new UITestHelper(metlWindow, metlWindow.AutomationElement.Descendant(Constants.ID_METL_SEARCH_RESULTS_TEXT));
            resultsText.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_SEARCH_RESULTS_TEXT));

            var correct = resultsText.WaitForControlCondition((uiControl) =>
            {
                if (uiControl.Current.Name == "Found 1 result.")
                {
                    return false;
                }

                return true;
            });

            Assert.IsTrue(correct);
             */
        }
    }
}
