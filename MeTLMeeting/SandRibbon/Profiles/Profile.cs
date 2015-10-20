using SandRibbon.Pages.Collaboration.Palettes;
using System.Collections.Generic;

namespace SandRibbon.Profiles
{
    public class Profile
    {
        public bool identifiesAsOwner { get; set; } = true;
        public string ownerName { get; set; }
        public string logicalName { get; set; }
        public IEnumerable<Bar> castBars { get; set; } = new List<Bar>();
    }
}
