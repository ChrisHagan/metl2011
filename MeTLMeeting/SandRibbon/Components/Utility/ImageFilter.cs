using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MeTLLib.DataTypes;
using SandRibbon.Pages;

namespace SandRibbon.Components.Utility
{
    public class ImageFilter : ContentFilter<List<UIElement>, UIElement>
    {
        public ImageFilter(SlideAwarePage page) : base(page) { }
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
