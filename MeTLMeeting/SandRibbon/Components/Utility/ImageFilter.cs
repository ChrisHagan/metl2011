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
    public class ImageFilter : ContentFilter<List<UIElement>, UIElement>
    {
        protected override bool Equals(UIElement item1, UIElement item2)
        {
            return (item1 as Image).tag().id == (item2 as Image).tag().id; 
        }

        protected override bool CollectionContains(UIElement item)
        {
            var imageTagId = (item as Image).tag().id;
            return contentCollection.Where(img => (img as Image).tag().id == imageTagId).Count() > 0;
        }

        protected override string AuthorFromTag(UIElement element)
        {
            return (element as Image).tag().author;
        }

        List<UIElement> Images
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
