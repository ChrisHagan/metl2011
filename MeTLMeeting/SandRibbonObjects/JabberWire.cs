using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;

namespace Constants
{
    public partial class JabberWire
    {
        private static string server_property;//This is a property so that we can point automation tests at different DBs
        public static string MUC = "conference." + SERVER;
        public static Jid GLOBAL = new Jid("global@" + MUC);
        public static string SERVER
        {
            get
            {
                //uncomment this line to lock the application to a specific server.
                //return "reifier.adm.monash.edu.au";
                return server_property;
            }
            set{
                server_property=value;
                MUC = "conference." + SERVER;
                GLOBAL = new Jid("global@" + MUC);
            }
        }
    }
}
