using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandRibbonObjects;

namespace SandRibbon.Providers
{
    public class Globals
    {
        public static SandRibbon.Utils.Connection.JabberWire.Location location{
            get{
                var conversationDetails = Globals.conversationDetails;
                return new SandRibbon.Utils.Connection.JabberWire.Location
                {
                    activeConversation=conversationDetails.Jid,
                    currentSlide=slide,
                    availableSlides=conversationDetails.Slides.Select(s=>s.id).ToList()
                };
            }
        }
        public static List<Slide> slides {
            get {
                return ((ConversationDetails)Commands.UpdateConversationDetails.lastValue()).Slides;
            }
        }
        public static ConversationDetails conversationDetails {
            get {
                return (ConversationDetails)Commands.UpdateConversationDetails.lastValue();
            }
        }
        public static SandRibbon.Utils.Connection.JabberWire.Credentials credentials {
            get {
                var credentials = Commands.SetIdentity.lastValue();
                return (SandRibbon.Utils.Connection.JabberWire.Credentials)credentials;
            }
        }
        public static List<Utils.Connection.JabberWire.AuthorizedGroup> authorizedGroups
        {
            get
            {
                var credentials = Commands.SetIdentity.lastValue();
                return ((Utils.Connection.JabberWire.Credentials) credentials).authorizedGroups;
            }
        }
        public static int slide 
        {
            get {
                return (int) Commands.MoveTo.lastValue();
            }
        }
        public static string me{
            get {
                return credentials.name;
            }
        }
        public static string privacy {
            get {
                return (string)Commands.SetPrivacy.lastValue();
            }
        }
    }
}