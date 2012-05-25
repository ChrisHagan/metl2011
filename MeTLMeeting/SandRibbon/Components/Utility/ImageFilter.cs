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
    public class ImageFilter : ContentFilter<List<Image>, Image>
    {
        protected override bool Equals(Image item1, Image item2)
        {
            return item1.tag().id == item2.tag().id; 
        }

        protected override bool CollectionContains(Image item)
        {
            var imageTagId = item.tag().id;
            return contentCollection.Where(img => img.tag().id == imageTagId).Count() > 0;
        }

        protected override string AuthorFromTag(Image element)
        {
            return element.tag().author;
        }

        List<Image> Images
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
