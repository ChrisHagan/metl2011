using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandRibbon.Utils.Connection;

namespace ThoughtIdeas
{
    public class ThoughtWire : JabberWire
    {
        public ThoughtWire(Credentials credentials)
            : base(credentials.name, null, credentials)
        {
        }
    }
}
