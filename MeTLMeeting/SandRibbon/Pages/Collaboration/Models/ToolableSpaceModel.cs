using SandRibbon.Profiles;

using MeTLLib;

namespace SandRibbon.Pages.Collaboration.Models
{
    public class ToolableSpaceModel
    {
        public MetlConfiguration backend { get; protected set; }
        public ToolableSpaceModel(MetlConfiguration _backend)
        {
            backend = _backend;
        }
        public Profile profile { get; set; }
        public VisibleSpaceModel context {get;set;}
    }
}
