using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using MeTLLib.DataTypes;
using SandRibbon.Utils;
using System.Collections.ObjectModel;

namespace SandRibbon.Components.Utility
{
    public class StrokeFilter : ContentFilter<StrokeCollection, Stroke>
    {
        protected override bool Equals(Stroke item1, Stroke item2)
        {
            return MeTLMath.ApproxEqual(item1.sum().checksum, item2.sum().checksum);
        }

        protected override bool CollectionContains(Stroke item)
        {
            return contentCollection.Where(s => MeTLMath.ApproxEqual(s.sum().checksum, item.sum().checksum)).Count() != 0;
        }

        protected override string AuthorFromTag(Stroke element)
        {
            return element.tag().author;
        }

        public IEnumerable<Stroke> StrokesWithChecksums(IEnumerable<string> checksums)
        {
            return contentCollection.Where(s => checksums.Contains(s.sum().checksum.ToString())).ToList();
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
