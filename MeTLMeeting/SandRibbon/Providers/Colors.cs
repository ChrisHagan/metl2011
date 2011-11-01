using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace SandRibbon.Providers
{
    public class AllColors
    {
        public static List<Color> all{
            get
            {
                return typeof(Colors).GetProperties()
                .Where(f => 
                    f.PropertyType == typeof(Color))
                .Select(f => 
                    (Color)f.GetValue(null, new object[0])).Reverse<Color>().ToList();
            }
        }
    }
}
