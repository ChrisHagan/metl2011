using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using MeTLLib.DataTypes;
using MeTLLib.Utilities;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components.Utility
{
    public class StrokeFilter : ContentFilter<List<PrivateAwareStroke>, PrivateAwareStroke>
    {
        public StrokeFilter(DataContextRoot root) : base(root) { }
        protected override bool Equals(PrivateAwareStroke item1, PrivateAwareStroke item2)
        {
            return item1.tag().id == item2.tag().id;
        }

        protected override bool CollectionContains(PrivateAwareStroke item)
        {
            return contentCollection.Where(s => Equals(s.tag().id, item.tag().id)).Count() != 0;
        }

        protected override string AuthorFromTag(PrivateAwareStroke element)
        {
            return element.tag().author;
        }

        protected override Privacy PrivacyFromTag(PrivateAwareStroke element)
        {
            return element.tag().privacy;
        }

        public List<PrivateAwareStroke> Strokes
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
