using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using MeTLLib.DataTypes;
using SandRibbon.Utils;
using System.Collections.ObjectModel;
using System;

namespace SandRibbon.Components.Utility
{
    public class StrokeChecksumFilter : ContentFilter<List<StrokeChecksum>, StrokeChecksum>
    {
        protected override bool Equals(StrokeChecksum item1, StrokeChecksum item2)
        {
            return MeTLMath.ApproxEqual(item1.checksum, item2.checksum);
        }

        protected override bool CollectionContains(StrokeChecksum item)
        {
            return contentCollection.Where(s => MeTLMath.ApproxEqual(s.checksum, item.checksum)).Count() != 0;
        }

        public List<StrokeChecksum> StrokeChecksums
        {
            get
            {
                return contentCollection;
            }
        }
    }
}
