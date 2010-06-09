using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandRibbonObjects;
using System.Windows.Ink;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Pedagogicometry;

namespace SandRibbon.Providers
{
    public class Globals
    {
        public static bool isAuthor 
        {
            get 
            {
                return me == conversationDetails.Author;
            }
        }
        public static PedagogyLevel pedagogy {
            get {
                try
                {
                    Commands.SetPedagogyLevel.lastValue();
                }
                catch (NotSetException)
                {
                    Pedagogicometer.SetDefaultPedagogyLevel();
                }
                return (PedagogyLevel)Commands.SetPedagogyLevel.lastValue();
            }
        }
        public static SandRibbon.Utils.Connection.JabberWire.Location location
        {
            get
            {
                try
                {
                    var conversationDetails = Globals.conversationDetails;
                    return new SandRibbon.Utils.Connection.JabberWire.Location
                    {
                        activeConversation = conversationDetails.Jid,
                        currentSlide = slide,
                        availableSlides = conversationDetails.Slides.Select(s => s.id).ToList()
                    };
                }
                catch (NotSetException e) {
                    throw e;
                }
            }
        }
        public static DrawingAttributes drawingAttributes
        {
            get
            {
                try{return ((DrawingAttributes)Commands.ReportDrawingAttributes.lastValue());}
                catch (NotSetException)
                {
                }
                return new DrawingAttributes();
            }
        }
        public static List<Slide> slides
        {
            get
            {
                return ((ConversationDetails)Commands.UpdateConversationDetails.lastValue()).Slides;
            }
        }
        public static ConversationDetails conversationDetails
        {
            get
            {
                return (ConversationDetails)Commands.UpdateConversationDetails.lastValue();
            }
        }
        public static SandRibbon.Utils.Connection.JabberWire.Credentials credentials
        {
            get
            {
                var credentials = Commands.SetIdentity.lastValue();
                return (SandRibbon.Utils.Connection.JabberWire.Credentials)credentials;
            }
        }
        public static List<Utils.Connection.JabberWire.AuthorizedGroup> authorizedGroups
        {
            get
            {
                var credentials = Commands.SetIdentity.lastValue();
                return ((Utils.Connection.JabberWire.Credentials)credentials).authorizedGroups;
            }
        }
        public static bool synched{
            get {
                try
                {
                    return (bool)Commands.SetSync.lastValue();
                }
                catch (NotSetException)
                {
                    return true;
                }
            }
        }
        public static int teacherSlide 
        {
            get 
            {
                return (int)Commands.SyncedMoveRequested.lastValue();
            }
        }
        public static int slide
        {
            get
            {
                return (int)Commands.MoveTo.lastValue();
            }
        }
        public static string me
        {
            get
            {
                return credentials.name;
            }
        }
        public static string privacy
        {
            get
            {
                return (string)Commands.SetPrivacy.lastValue();
            }
        }
    }
}