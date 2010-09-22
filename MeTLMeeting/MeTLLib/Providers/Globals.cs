using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;

namespace MeTLLib.Providers
{
    class Globals
    {
        public static bool isAuthor
        {
            get
            {
                return me == conversationDetails.Author;
            }
        }
        public static Location location
        {
            get
            {
                try
                {
                    var conversationDetails = Globals.conversationDetails;
                    return new Location
                    {
                        activeConversation = conversationDetails.Jid,
                        currentSlide = slide,
                        availableSlides = conversationDetails.Slides.Select(s => s.id).ToList()
                    };
                }
                catch (NotSetException e)
                {
                    throw e;
                }
            }
        }
        public static DrawingAttributes drawingAttributes
        {
            get
            {
                try { return ((DrawingAttributes)Commands.ReportDrawingAttributes.lastValue()); }
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
        public static Credentials credentials
        {
            get
            {
                var credentials = Commands.SetIdentity.lastValue();
                return (Credentials)credentials;
            }
        }
        public static List<AuthorizedGroup> authorizedGroups
        {
            get
            {
                var credentials = Commands.SetIdentity.lastValue();
                return ((Credentials)credentials).authorizedGroups;
            }
        }
        public static bool synched
        {
            get
            {
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