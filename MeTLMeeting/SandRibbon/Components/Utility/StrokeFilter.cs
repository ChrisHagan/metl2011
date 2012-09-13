using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using MeTLLib.DataTypes;
using MeTLLib.Utilities;

namespace SandRibbon.Components.Utility
{
    public class StrokeFilter : ContentFilter<StrokeCollection, Stroke>
    {
        protected override bool Equals(Stroke item1, Stroke item2)
        {
            return item1.tag().id == item2.tag().id;
        }

        protected override bool CollectionContains(Stroke item)
        {
            return contentCollection.Where(s => Equals(s.tag().id, item.tag().id)).Count() != 0;
        }

        protected override string AuthorFromTag(Stroke element)
        {
            return element.tag().author;
        }

        protected override Privacy PrivacyFromTag(Stroke element)
        {
            return element.tag().privacy;
        }

        public StrokeCollection Strokes
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
