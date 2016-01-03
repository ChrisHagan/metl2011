using System;
using System.Collections.Generic;
using System.Linq;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Pedagogicometry;
using System.Drawing;
using TextInformation = SandRibbon.Components.TextInformation;
using SandRibbonObjects;
using SandRibbon.Components.Utility;

namespace SandRibbon.Providers
{
    enum DefaultPageDimensions
    {
        Width = 720,
        Height = 540
    }

    public class Globals
    {
        public const string METLCOLLABORATOR = "MeTL Collaborator";
        public const string METLDEMONSTRATOR = "MeTL Demonstrator";
        public const string METL = "MeTL";
        public const string METLPRESENTER = "MeTL Presenter";

        private static Size canvasSize = new Size();
        private static QuizData quizData = new QuizData();
        public static bool AuthorOnline(string author)
        {
            return PresenceListing.Keys.Where(k => k.Contains(author)).Count() > 0;
        }
        public static bool AuthorInRoom(string author, string jid)
        {
            
            var authorStatus = PresenceListing.Keys.Where(k => k.Contains(author));
            if (authorStatus.Count() == 0)
                return false;
            var rooms = authorStatus.Aggregate(new List<string>(), (acc, item) =>
                                                                       {
                                                                           acc.AddRange(PresenceListing[item]);
                                                                           return acc;
                                                                       });
            return rooms.Contains(jid);
        }
        public static void UpdatePresenceListing(MeTLPresence presence)
        {
            if (!PresenceListing.ContainsKey(presence.Who))
            {
                if(presence.Joining)
                    PresenceListing.Add(presence.Who, new List<string> {presence.Where});
            }
            else
            {
                if (presence.Joining)
                {
                    var list = PresenceListing[presence.Who];
                    list.Add(presence.Where);
                    PresenceListing[presence.Who] = list.Distinct().ToList();
                }
                else
                {
                    PresenceListing[presence.Who].Remove(presence.Where);
                }
            }
        }
        public static Dictionary<string, List<string>> PresenceListing = new Dictionary<string, List<string>>();
        public static readonly string PUBLIC = "public";
        public static readonly string PRIVATE = "private";
        public static readonly string PROJECTOR = "projector";
        
        public static string generateId()
        {
            return string.Format("{0}:{1}", Globals.me, DateTimeFactory.Now().Ticks);
        }
        public static string generateId(string seed)
        {
            return string.Format("{0}:{1}:{2}", Globals.me, DateTimeFactory.Now().Ticks, seed);
        }
        public static bool isAuthor
        {
            get
            {
                if (me == null || conversationDetails.ValueEquals(ConversationDetails.Empty)) return false;
                return me == conversationDetails.Author;
            }
        }
        public static UserOptions UserOptions 
        {
            get 
            {
                return SandRibbon.Commands.SetUserOptions.IsInitialised ? (UserOptions)SandRibbon.Commands.SetUserOptions.LastValue() : UserOptions.DEFAULT;
            }
        }
        public static string MeTLType
        {
            get
            {
                try
                {
                    return Commands.MeTLType.LastValue().ToString();
                }
                catch (Exception)
                {
                    return METL;
                }
            }

        }
        public static int QuizMargin { get { return 30; } }
        public static Size DefaultCanvasSize
        {
            get
            {
                return new Size((int)DefaultPageDimensions.Width, (int)DefaultPageDimensions.Height);
            }
        }
        public static Size CanvasSize
        {
            get
            {
                return canvasSize;
            }
            set
            {
                canvasSize = value;
            }
        }
        public static PedagogyLevel pedagogy
        {
            get
            {
                return (PedagogyLevel)Commands.SetPedagogyLevel.LastValue();
            }
        }
        public static Location location
        {
            get
            {
                var conversationDetails = Globals.conversationDetails;
                return new Location(conversationDetails.Jid, slide, conversationDetails.Slides.Select(s => s.id).ToList());
            }
        }
        public static List<Slide> slides
        {
            get
            {
                var value = ((ConversationDetails)Commands.UpdateConversationDetails.LastValue()); 
                if (value != null)
                    return value.Slides;
                else throw new NotSetException("Slides not set");
            }
        }
        public static Slide slideDetails
        {
            get
            {
                try
                {
                    return conversationDetails.Slides.Find(s => s.id == slide);
                }
                catch
                {
                    return Slide.Empty;
                }
            }
        }
        public static ConversationDetails conversationDetails
        {
            get
            {
                return Commands.UpdateConversationDetails.IsInitialised ? (ConversationDetails)Commands.UpdateConversationDetails.LastValue() : null;
            }
        }
        public static KeyValuePair<ConversationDetails,Slide> slideDetailsInConversationDetails
        {
            get
            {
                return new KeyValuePair<ConversationDetails, Slide>(conversationDetails, slideDetails);
            }
        }
        public static List<ContentVisibilityDefinition> contentVisibility
        {
            get
            {
                return Commands.SetContentVisibility.IsInitialised ? (List<ContentVisibilityDefinition>)Commands.SetContentVisibility.LastValue() : new List<ContentVisibilityDefinition>();
            }
        }
        public static MeTLLib.DataTypes.QuizData quiz
        {
            get
            {
                return quizData;                
            }
        }
        public static TextInformation currentTextInfo
        {
            get;
            set;
        }
        public static MeTLLib.DataTypes.Credentials credentials
        {
            get
            {
                return (MeTLLib.DataTypes.Credentials) Commands.SetIdentity.LastValue();
            }
        }
        public static List<MeTLLib.DataTypes.AuthorizedGroup> authorizedGroups
        {
            get
            {
                return credentials.authorizedGroups;
            }
        }
        public static List<string> authorizedGroupNames {
            get 
            {
                return authorizedGroups.Select(g => g.groupKey).ToList();
            }
        }
        public static bool synched
        {
            get; set;
        } = false;
        public static int teacherSlide
        {
            get
            {
                return (int)Commands.SyncedMoveRequested.LastValue();
            }
        }
        public static int slide
        {
            get
            {
                return Commands.MoveTo.IsInitialised ? (int)Commands.MoveTo.LastValue() : -1;
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
                return (Commands.SetPrivacy.IsInitialised ? (string)Commands.SetPrivacy.LastValue() : String.Empty);
            }
        }
        public static Policy policy
        {
            get { return new Policy(isAuthor, false); }
        }

        public static UserInformation userInformation
        {
            get { return new UserInformation(credentials, location, policy); }
        }

        public static bool IsBanhammerActive 
        { 
            get 
            {
                return (bool)(Commands.BanhammerActive.IsInitialised ? Commands.BanhammerActive.LastValue() : false);
            } 
        }

        public static bool rememberMe 
        {
            get
            {
                return (bool)Commands.RememberMe.LastValue();
            }
        }

        private static StoredUIState _storedUIState;
        public static StoredUIState StoredUIState
        {
            get
            {
                if (_storedUIState == null)
                {
                    _storedUIState = new StoredUIState();
                }

                return _storedUIState;
            }
        }


        public delegate void CanvasClipboardFocusChangedHandler(object sender, EventArgs e);

        private static object lockCurrentCanvasClipboardFocus = new object();
        private static string currentCanvasClipboardFocus = "";
        public static event CanvasClipboardFocusChangedHandler CanvasClipboardFocusChanged;
        public static string CurrentCanvasClipboardFocus
        {
            get
            {
                return currentCanvasClipboardFocus;
            }
            set
            {
                lock (lockCurrentCanvasClipboardFocus)
                {
                    currentCanvasClipboardFocus = value;
                    if (CanvasClipboardFocusChanged != null)
                    {
                        CanvasClipboardFocusChanged(currentCanvasClipboardFocus, EventArgs.Empty);
                    }
                }
            }
        }
    }
}