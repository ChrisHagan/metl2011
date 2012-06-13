using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using Ninject;

// REMOVEME: Testing commit hook for MeTLLib changes
// REMOVEME: Will email when a file has been modified in MeTLLib in a push/pull or unbundle

namespace MeTLLib
{
    partial class Constants
    {
        private static string server_property;//This is a property so that we can point automation tests at different DBs
        public static string MUC = "conference." + SERVER;
        public static Jid GLOBAL = new Jid("global@" + MUC);
        public const string MeTlPresenter = "MeTL Presenter";
        public const string MeTL = "MeTL";
        public const string MeTLCollaborator = "MeTL Collaborator";
        public const string MeTLDemonstrator = "MeTL Demonstrator";
        public static string SERVER
        {
            get
            {
                //uncomment this line to lock the application to a specific server.
                //return "reifier.adm.monash.edu.au";
                return server_property;
            }
            set
            {
                server_property = value;
                MUC = "conference." + SERVER;
                GLOBAL = new Jid("global@" + MUC);
            }
        }
    }
}
