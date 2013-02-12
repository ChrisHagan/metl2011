using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;
using SandRibbon.Utils;

namespace SandRibbon.Components.Utility
{
    public class TextFilter : ContentFilter<List<UIElement>, UIElement>
    {
        protected override bool Equals(UIElement item1, UIElement item2)
        {
            return item1 is MeTLTextBox && item2 is MeTLTextBox && (item1 as MeTLTextBox).tag().id == (item2 as MeTLTextBox).tag().id; 
        }

        protected override bool CollectionContains(UIElement item)
        {
            var textTagId = (item as MeTLTextBox).tag().id;
            return contentCollection.Any(txt => txt is MeTLTextBox && (txt as MeTLTextBox).tag().id == textTagId);
        }

        protected override string AuthorFromTag(UIElement element)
        {
            if (element is MeTLTextBox)
            {
                return (element as MeTLTextBox).tag().author;
            }
            else return String.Empty;
        }

        protected override Privacy PrivacyFromTag(UIElement element)
        {
            if (element is MeTLTextBox)
            {
                return (element as MeTLTextBox).tag().privacy;
            }
            else return Privacy.NotSet;
        }

        public List<UIElement> TextBoxes
        {
            get
            {
                return contentCollection;
            }
        }
        private bool compareStringContents(string a, string b){
            return a.ToLower().Trim() == b.ToLower().Trim();
        }
        private void possiblyReEnableMyContent<T>(T element){
            if (element is UIElement){
                var boxAuthor = AuthorFromTag(element as UIElement);
                if (compareStringContents(boxAuthor, Globals.me))
                {
                    var boxPrivacy = PrivacyFromTag(element as UIElement);
                    if (boxPrivacy == Privacy.Private)
                    {
                        if (!ContentVisibilityUtils.getMyPrivateVisible(CurrentContentVisibility))
                        {
                            Commands.SetContentVisibility.Execute(ContentVisibilityUtils.setMyPrivateVisible(CurrentContentVisibility, true));
                            // turn on visibilty of myPrivate
                        }
                    }
                    else if (boxPrivacy == Privacy.Public)
                    {
                        if (!ContentVisibilityUtils.getMyPublicVisible(CurrentContentVisibility))
                        {
                            Commands.SetContentVisibility.Execute(ContentVisibilityUtils.setMyPublicVisible(CurrentContentVisibility, true));
                            // turn on visibility of myPublic
                        }
                    }
                }
            }
        }
        public void Push(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            possiblyReEnableMyContent(element);
            base.Add(element, modifyVisibleContainer);
        }
    }
}
