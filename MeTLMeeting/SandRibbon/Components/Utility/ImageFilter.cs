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
            return (item1 as MeTLImage).tag().id == (item2 as MeTLImage).tag().id; 
        }

        protected override bool CollectionContains(UIElement item)
        {
            var imageTagId = (item as MeTLImage).tag().id;
            return contentCollection.Where(img => (img as MeTLImage).tag().id == imageTagId).Count() > 0;
        }

        protected override string AuthorFromTag(UIElement element)
        {
            return (element as MeTLImage).tag().author;
        }

        protected override Privacy PrivacyFromTag(UIElement element)
        {
            return (element as MeTLImage).tag().privacy;
        }

        public List<UIElement> Images
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
