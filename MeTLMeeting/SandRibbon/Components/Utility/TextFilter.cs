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
    public class TextFilter : ContentFilter<List<TextBox>, TextBox>
    {
        protected override bool Equals(TextBox item1, TextBox item2)
        {
            return item1.tag().id == item2.tag().id; 
        }

        protected override bool CollectionContains(TextBox item)
        {
            var textTagId = item.tag().id;
            return contentCollection.Where(txt => txt.tag().id == textTagId).Count() > 0;
        }

        protected override string AuthorFromTag(TextBox element)
        {
            return element.tag().author;
        }

        List<TextBox> TextBoxes
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
