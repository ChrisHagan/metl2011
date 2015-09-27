using SandRibbon.Pages.Collaboration.Palettes;
using System.Collections.Generic;

namespace SandRibbon.Profiles
{
    public class Profile
    {
        public string logicalName { get; set; }
        public IEnumerable<Bar> castBars { get; set; }
    }
}
