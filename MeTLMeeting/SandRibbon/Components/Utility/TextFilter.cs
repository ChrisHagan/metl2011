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
            return (item1 as TextBox).tag().id == (item2 as TextBox).tag().id; 
        }

        protected override bool CollectionContains(UIElement item)
        {
            var textTagId = (item as TextBox).tag().id;
            return contentCollection.Where(txt => (txt as TextBox).tag().id == textTagId).Count() > 0;
        }

        protected override string AuthorFromTag(UIElement element)
        {
            return (element as TextBox).tag().author;
        }

        protected override Privacy PrivacyFromTag(UIElement element)
        {
            return (element as TextBox).tag().privacy;
        }

        List<UIElement> TextBoxes
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
