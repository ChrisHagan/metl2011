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
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components.Utility
{
    public class ImageFilter : ContentFilter<List<UIElement>, UIElement>
    {
        public ImageFilter(DataContextRoot page) : base(page) { }
        protected override bool Equals(UIElement item1, UIElement item2)
        {
            return (item1 as MeTLImage).tag().id == (item2 as MeTLImage).tag().id; 
        }

        protected override bool CollectionContains(UIElement item)
        {
            if (item is MeTLImage)
            {
                var imageTagId = (item as MeTLImage).tag().id;
                return contentCollection.Where(img => (img as MeTLImage).tag().id == imageTagId).Count() > 0;
            }
            else return false;
        }

        protected override string AuthorFromTag(UIElement element)
        {
            if (element is MeTLImage)
            {
                return (element as MeTLImage).tag().author;
            }
            else return String.Empty;
        }

        protected override Privacy PrivacyFromTag(UIElement element)
        {
            if (element is MeTLImage)
            {
                return (element as MeTLImage).tag().privacy;
            }
            else return Privacy.NotSet;
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
