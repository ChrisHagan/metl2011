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
        public static ConversationDetails conversationDetails
        {
            get;
            set;
        }
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
                    if (conversationDetails == null) throw new NotSetException("ConversationDetails not set in Globals::Location");
                    return new Location(conversationDetails.Jid, slide, conversationDetails.Slides.Select(s => s.id).ToList());
                }
                catch (NotSetException e)
                {
                    throw e;
                }
            }
        }

        public static List<Slide> slides
        {
            get
            {
                if (conversationDetails == null) return new List<Slide>();
                return conversationDetails.Slides;
            }
        }
        public static Credentials credentials
        {
            get;
            set;
        }
        public static List<AuthorizedGroup> authorizedGroups
        {
            get
            {
                if (credentials == null) return new List<AuthorizedGroup>();
                return credentials.authorizedGroups;                    
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
            get;
            set;
        }
        public static string me
        {
            get
            {
                return credentials.name;
            }
        }
    }
}