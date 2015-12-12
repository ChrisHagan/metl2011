using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Divan;
using MeTLLib.Providers;
using System.Globalization;
using System.Windows;

namespace MeTLLib.DataTypes
{
    public enum Privacy
    {
        NotSet,
        Private,
        Public
    }

    public static class PrivacyExtensions
    {
        public static Privacy InvertPrivacy(this Privacy privacy)
        {
            if (privacy == Privacy.Private)
                return Privacy.Public;
            if (privacy == Privacy.Public)
                return Privacy.Private;

            return Privacy.NotSet;
        }
    }

    public class SearchConversationDetails : ConversationDetails
    {
        public const int DEFAULT_MAX_SEARCH_RESULTS = 50;

        public int relevance { get; set; }
        public long LastModified { get; set; }

        public SearchConversationDetails(ConversationDetails conv)
            : base(conv)
        {
            Created = conv.Created;
        }
        public SearchConversationDetails(string title, string author, string created, int relevance, string restriction, string jid, string lastModified)
            : base(title, jid, author, new List<Slide>(), Permissions.Empty, restriction)
        {
            this.relevance = relevance;
            Created = new DateTime(long.Parse(created));
            LastModified = long.Parse(lastModified);
        }
        private static string ParseDate(XElement doc, string elementTag)
        {
            var dateRegex = @"(\d+)/(\d+)/(\d+) (\d+):(\d+):(\d+) (\w+)";
            var date = new DateTime();
            var dateString = doc.Element(CREATED_TAG).Value;
            var match = Regex.Match(dateString, dateRegex);
            if (match.Success)
            {
                var year = Int32.Parse(match.Groups[3].Value);
                var month = Int32.Parse(match.Groups[2].Value);
                var day = Int32.Parse(match.Groups[1].Value);
                var rawHour = Int32.Parse(match.Groups[4].Value);
                int hour = 0;
                var minute = Int32.Parse(match.Groups[5].Value);
                var seconds = Int32.Parse(match.Groups[6].Value);
                var timeOfDay = match.Groups[7];
                if (timeOfDay.ToString().ToLower() == "am" || rawHour == 12)
                    hour = rawHour;
                else
                {
                    hour = rawHour + 12;
                }
                date = new DateTime(year, month, day, hour, minute, seconds);
            }

            return date.Ticks.ToString();
        }

        public static SearchConversationDetails ReadXML(XElement doc)
        {
            var cd = new SearchConversationDetails(ConversationDetails.ReadXml(doc));
            return cd;
        }

        public static SearchConversationDetails HydrateFromServer(IClientBehaviour conn, ConversationDetails conv)
        {
            if (conv == null)
                throw new ArgumentNullException();

            return HydrateFromServer(conn,new SearchConversationDetails(conv));
        }
        public static SearchConversationDetails HydrateFromServer(IClientBehaviour conn,SearchConversationDetails scd)
        {
            if (scd == null)
                throw new ArgumentNullException("scd", "Probably ConversationDetails is being cast as SearchConversationDetails");

            var conversation = conn.DetailsOf(scd.Jid);

            scd.blacklist = conversation.blacklist;
            scd.CreatedAsTicks = conversation.CreatedAsTicks;
            scd.Permissions = conversation.Permissions;
            scd.Slides = conversation.Slides;
            scd.Tag = conversation.Tag;

            return scd;
        }
    }

    public class Attendance
    {
        public bool present { get; protected set; }
        public string author { get; protected set; }
        public string location { get; protected set; }
        public long timestamp { get; set; }
        public Attendance(string _author, string _location, bool _present,long _timestamp)
        {
            author = _author;
            location = _location;
            present = _present;
            timestamp = _timestamp;
        }
    }

        public class ConversationDetails
    {
        public bool isDeleted
        {
            get
            {
                return Subject.ToLower().GetHashCode() == "deleted".GetHashCode();
            }
        }
        public bool isAuthor(string user)
        {
            if (user == null || ValueEquals(Empty)) return false;
            return user.ToLower() == Author.ToLower();
        }

        public bool IsEmpty
        {
            get
            {
                return Equals(ConversationDetails.Empty);
            }
        }
        public static string DefaultName(string author)
        {
            var ci = CultureInfo.GetCultureInfo("en-US");
            var d = DateTime.Now.ToString("r", ci);
            return string.Format("{0} at {1}", author, d);
        }
        public ConversationDetails(String title, String jid, String author, List<Slide> slides, Permissions permissions, String subject)
            : base()
        {
            this.Title = title;
            this.Jid = jid;
            this.Author = author;
            this.Slides = slides.OrderBy(s => s.index).ToList();
            this.Permissions = permissions;
            this.Subject = subject;
        }
        public ConversationDetails(String title, String jid, String author, List<Slide> slides, Permissions permissions, String subject, DateTime created, DateTime lastAccessed)
            : this(title, jid, author, slides, permissions, subject)
        {
            this.Created = created;
            this.CreatedAsTicks = created.Ticks;
            this.LastAccessed = lastAccessed;
        }
        public ConversationDetails(String title, String jid, String author, String tag, List<Slide> slides, Permissions permissions, String subject, DateTime created, DateTime lastAccessed)
            : this(title, jid, author, slides, permissions, subject, created, lastAccessed)
        {
            this.Tag = tag;
        }
        public ConversationDetails(String title, String jid, String author, String tag, List<Slide> slides, Permissions permissions, String subject, DateTime created, DateTime lastAccessed, List<string> blacklist)
            : this(title, jid, author, slides, permissions, subject, created, lastAccessed)
        {
            this.blacklist = blacklist;
            this.Tag = tag;
        }
        public ConversationDetails(String title, String jid, String author, String tag, List<Slide> slides, Permissions permissions, String subject)
            : this(title, jid, author, slides, permissions, subject)
        {
            this.Tag = tag;
        }
        public ConversationDetails(ConversationDetails copyDetails)
        {
            this.Title = copyDetails.Title;
            this.Author = copyDetails.Author;
            this.Created = new DateTime(copyDetails.Created.Ticks);
            this.LastAccessed = new DateTime(copyDetails.LastAccessed.Ticks);
            this.Tag = copyDetails.Tag;
            this.Subject = copyDetails.Subject;
            this.Jid = copyDetails.Jid;
            this.Permissions = new Permissions(copyDetails.Permissions);
            this.Slides = new List<Slide>(copyDetails.Slides);
            this.blacklist = new List<string>(copyDetails.blacklist);
        }
        public int NextAvailableSlideId()
        {
            return Slides.Select(s => s.id).Max() + 1;
        }
        public ConversationDetails Clone()
        {
            return ReadXml(WriteXml());
        }


        public string Title { get; set; }

        public string Jid { get; set; }

        /*The jid is a valid Xmpp jid.  If, for instance, you want
                                       * to create a room specific to this conversation so that
                                       * you can restrict broadcast, this is safe to work in Jabber 
                                       * and on the filesystem, whereas the Title is NOT. 
                                       * (If anybody finds another character that breaks it - 
                                       * obvious when history stops working - add it to the illegals 
                                       * string in generateJid).  Never mind that, we're just using a number.*/


        public string Author { get; set; }
        public Permissions Permissions { get; set; }

        public DateTime Created { get; set; }
        public long CreatedAsTicks { get; set; }
        public DateTime LastAccessed { get; set; }
        public string Tag { get; set; }
        public string Subject { get; set; }

        public List<Slide> Slides { get; set; }
        public List<string> blacklist { get; set; }
        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(WriteXml().ToString(SaveOptions.DisableFormatting));
        }
        public bool IsValid
        {
            get
            {
                return !new[] { Jid, Subject, Title, Author }.Any(String.IsNullOrEmpty);                
            }
        }

        public bool UserIsBlackListed(string userId)
        {
            return blacklist.Contains(userId);
        }

        public bool IsJidEqual(string thatJid)
        {
            return Jid == thatJid;            
        }

        public bool UserHasPermission(Credentials credentials)
        {
            if (!(credentials.name.ToLower() == Author.ToLower())
                && !(credentials.authorizedGroups.Select(g => g.groupKey.ToLower()).Contains(Subject.ToLower()))
                && Subject.ToLower() != "Unrestricted".ToLower()
                && !(String.IsNullOrEmpty(Subject))
                && !(credentials.authorizedGroups.Select(su => su.groupKey.ToLower()).Contains("Superuser".ToLower()))) return false;
            return true;
        }
        
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is ConversationDetails)) return false;
            var foreignConversationDetails = ((ConversationDetails)obj);
            return ((foreignConversationDetails.Author == Author)
                //&& (foreignConversationDetails.Created == Created)
                && (foreignConversationDetails.IsValid == IsValid)
                && (foreignConversationDetails.Jid == Jid)
                //&& (foreignConversationDetails.LastAccessed.ToString() == LastAccessed.ToString())
                && (foreignConversationDetails.Permissions.ValueEquals(Permissions))
                && (foreignConversationDetails.Slides.All(s => s.ValueEquals(Slides[foreignConversationDetails.Slides.IndexOf(s)])))
                && (foreignConversationDetails.Subject == Subject)
                && (foreignConversationDetails.Tag == Tag)
                && (foreignConversationDetails.Title == Title));
        }

        public ConversationDetails()
            : this(String.Empty, String.Empty, String.Empty, new List<Slide>(), Permissions.Empty, String.Empty, new DateTime(), new DateTime())
        {
        }
        
        public static ConversationDetails Empty
        {
            get { return new ConversationDetails(); }
        }
        
        private static readonly string TITLE_TAG = "title";
        private static readonly string AUTHOR_TAG = "author";
        protected static readonly string CREATED_TAG = "created";
        private static readonly string LAST_ACCESSED_TAG = "lastAccessed";
        private static readonly string TAG_TAG = "tag";
        private static readonly string SUBJECT_TAG = "subject";
        private static readonly string JID_TAG = "jid";
        private static readonly string ID_TAG = "id";
        private static readonly string INDEX_TAG = "index";
        private static readonly string SLIDE_TAG = "slide";
        private static readonly string TYPE_TAG = "type";
        private static readonly string EXPOSED_TAG = "exposed";
        private static readonly string DEFAULT_WIDTH = "defaultWidth";
        private static readonly string DEFAULT_HEIGHT = "defaultHeight";
        private static readonly string BLACKLIST_TAG = "blacklist";

        private static readonly string GROUP_SET_TAG = "groupSet";
        private static readonly string GROUP_SET_ID_TAG = "id";
        private static readonly string GROUP_SET_LOCATION_TAG = "location";
        private static readonly string GROUP_SET_SIZE_TAG = "groupSize";
        private static readonly string GROUP_SET_GROUPS_TAG = "groups";
        private static readonly string GROUP_TAG = "group";
        private static readonly string GROUP_ID_TAG = "id";
        private static readonly string GROUP_LOCATION_TAG = "location";
        private static readonly string GROUP_MEMBERS_TAG = "members";
        private static readonly string GROUP_MEMBER_TAG = "member";
        public static ConversationDetails ReadXml(XElement doc)
        {
            var Title = doc.Element(TITLE_TAG).Value;
            var Author = doc.Element(AUTHOR_TAG).Value;
            var Created = DateTimeFactory.TryParse(doc.Element(CREATED_TAG).Value);
            var Tag = "";
            if (doc.Element(TAG_TAG) != null)
                Tag = doc.Element(TAG_TAG).Value;
            DateTime LastAccessed = new DateTime();
            if (doc.Element(LAST_ACCESSED_TAG) != null)
                LastAccessed = DateTimeFactory.TryParse(doc.Element(LAST_ACCESSED_TAG).Value);

            var Subject = "";
            if (doc.Element(SUBJECT_TAG) != null)
                Subject = doc.Element(SUBJECT_TAG).Value;
            var Jid = doc.Element(JID_TAG).Value;
            var internalPermissions = doc.Element(Permissions.PERMISSIONS_TAG) != null ? Permissions.ReadXml(doc.Element(Permissions.PERMISSIONS_TAG)) : Permissions.Empty;
            var Slides = doc.Descendants(SLIDE_TAG).Select(d => new Slide(
                Int32.Parse(d.Element(ID_TAG).Value),
                d.Element(AUTHOR_TAG) == null ? Author : d.Element(AUTHOR_TAG).Value,
                d.Element(TYPE_TAG) == null ? Slide.TYPE.SLIDE : (Slide.TYPE)Enum.Parse(typeof(Slide.TYPE), d.Element(TYPE_TAG).Value),
                Int32.Parse(d.Element(INDEX_TAG).Value),
                d.Element(DEFAULT_WIDTH) != null ? float.Parse(d.Element(DEFAULT_WIDTH).Value) : 720,
                d.Element(DEFAULT_HEIGHT) != null ? float.Parse(d.Element(DEFAULT_HEIGHT).Value) : 540,
                d.Element(EXPOSED_TAG) != null ? Boolean.Parse(d.Element(EXPOSED_TAG).Value) : true,
                d.Element(GROUP_SET_TAG) != null ? d.Descendants(GROUP_SET_TAG).Select(gs => new GroupSet(
                    gs.Element(GROUP_SET_ID_TAG).Value,
                    gs.Element(GROUP_SET_LOCATION_TAG).Value,
                    0,//Int32.Parse(gs.Element(GROUP_SET_SIZE_TAG).Value),
                    gs.Element(GROUP_SET_GROUPS_TAG).Descendants(GROUP_TAG).Select(g => new Group(
                            g.Element(GROUP_ID_TAG).Value,
                            g.Element(GROUP_LOCATION_TAG).Value,
                            g.Descendants(GROUP_MEMBERS_TAG).Select(gm =>
                                gm.Element(GROUP_MEMBER_TAG).Value
                            ).ToList()
                        )
                    ).ToList()
                )).ToList() : new List<GroupSet>()
            )).ToList();
            var blacklistElements = doc.Elements(BLACKLIST_TAG);
            var blacklist = new List<string>();
            if (blacklistElements != null && blacklistElements.Count() > 0)
                blacklist = blacklistElements.Select(d => d.Value).ToList();
            return new ConversationDetails(Title, Jid, Author, Tag, Slides, internalPermissions, Subject, Created, LastAccessed, blacklist.ToList());
        }
        public XElement WriteXml()
        {
            return
            new XElement("conversation",
                new XElement(TITLE_TAG, Title),
                new XElement(AUTHOR_TAG, Author),
                new XElement(CREATED_TAG, Created.ToString()),
                new XElement(LAST_ACCESSED_TAG, LastAccessed.ToString()),
                new XElement(TAG_TAG, Tag),
                new XElement(SUBJECT_TAG, Subject),
                new XElement(JID_TAG, Jid),
                Permissions.WriteXml(),
                Slides.Select(s => new XElement(SLIDE_TAG,
                    new XElement(AUTHOR_TAG, s.author),
                    new XElement(ID_TAG, s.id.ToString()),
                    new XElement(INDEX_TAG, s.index.ToString()),
                    new XElement(DEFAULT_HEIGHT, s.defaultHeight),
                    new XElement(DEFAULT_WIDTH, s.defaultWidth),
                    new XElement(EXPOSED_TAG, s.exposed.ToString()),
                    new XElement(TYPE_TAG, s.type.ToString()),
                    s.GroupSets.Select(gs => new XElement(GROUP_SET_TAG,
                        new XElement(GROUP_SET_ID_TAG,gs.id),
                        new XElement(GROUP_SET_LOCATION_TAG,gs.location),
                        new XElement(GROUP_SET_SIZE_TAG,gs.groupSize.ToString()),
                        new XElement(GROUP_SET_GROUPS_TAG,gs.Groups.Select(g => new XElement(GROUP_TAG,
                            new XElement(GROUP_ID_TAG,g.id),
                            new XElement(GROUP_LOCATION_TAG,g.location),
                            new XElement(GROUP_MEMBERS_TAG,g.GroupMembers.Select(gm => new XElement(GROUP_MEMBER_TAG, gm)))
                        ))))
                ))),
                blacklist.Select(b => new XElement(BLACKLIST_TAG, b)));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Permissions
    {
        public Permissions(string newLabel, bool newStudentsCanOpenFriends, bool newStudentsCanPublish, bool newUsersAreCompulsorilySynced)
        {
            Label = newLabel;
            studentCanOpenFriends = newStudentsCanOpenFriends;
            studentCanPublish = newStudentsCanPublish;
            usersAreCompulsorilySynced = newUsersAreCompulsorilySynced;
        }
        public Permissions(string newLabel, bool newStudentsCanOpenFriends, bool newStudentsCanPublish, bool newUsersAreCompulsorilySynced, String newConversationGroup)
        {
            Label = newLabel;
            studentCanOpenFriends = newStudentsCanOpenFriends;
            studentCanPublish = newStudentsCanPublish;
            usersAreCompulsorilySynced = newUsersAreCompulsorilySynced;
            conversationGroup = newConversationGroup;
        }
        /* copy constructor */
        public Permissions(Permissions copyPermissions)
        {
            this.Label = copyPermissions.Label;
            this.studentCanOpenFriends = copyPermissions.studentCanOpenFriends;
            this.NavigationLocked = copyPermissions.NavigationLocked;
            this.usersAreCompulsorilySynced = copyPermissions.usersAreCompulsorilySynced;
            this.conversationGroup = copyPermissions.conversationGroup;
        }
        public static Permissions InferredTypeOf(Permissions permissions)
        {
            var typeOfPermissions = new[] { LECTURE_PERMISSIONS, LABORATORY_PERMISSIONS, TUTORIAL_PERMISSIONS, MEETING_PERMISSIONS }.Where(
                   p => p.studentCanOpenFriends == permissions.studentCanOpenFriends &&
                       p.studentCanPublish == permissions.studentCanPublish &&
                       p.usersAreCompulsorilySynced == permissions.usersAreCompulsorilySynced).FirstOrDefault();
            if (typeOfPermissions != null) return typeOfPermissions;
            return CUSTOM_PERMISSIONS;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Permissions)) return false;
            return (((Permissions)obj).Label == Label);
        }
        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is Permissions)) return false;
            var foreignPermissions = ((Permissions)obj);
            return ((foreignPermissions.studentCanOpenFriends == studentCanOpenFriends)
                && (foreignPermissions.studentCanPublish == studentCanPublish)
                && (foreignPermissions.usersAreCompulsorilySynced == usersAreCompulsorilySynced));
        }
        public static Permissions CUSTOM_PERMISSIONS = new Permissions("custom", false, false, false);
        public static Permissions LECTURE_PERMISSIONS = new Permissions("lecture", false, false, true);
        public static Permissions LABORATORY_PERMISSIONS = new Permissions("laboratory", false, true, false);
        public static Permissions TUTORIAL_PERMISSIONS = new Permissions("tutorial", true, true, false);
        public static Permissions MEETING_PERMISSIONS = new Permissions("meeting", true, true, true);
        public static Permissions Empty { get { return new Permissions("", false, false, false); } }
        public void applyTuteStyle()
        {
            Label = "tutorial";
            studentCanOpenFriends = true;
            studentCanPublish = true;
            usersAreCompulsorilySynced = false;
        }
        public void applyLectureStyle()
        {
            Label = "lecture";
            studentCanOpenFriends = false;
            studentCanPublish = false;
            usersAreCompulsorilySynced = true;
        }
        private static readonly Permissions[] OPTIONS = new[]{
            LECTURE_PERMISSIONS,
            LABORATORY_PERMISSIONS,
            TUTORIAL_PERMISSIONS,
            MEETING_PERMISSIONS};
        public static readonly string PERMISSIONS_TAG = "permissions";
        public string Label { get; set; }
        public bool studentCanPublish = false;
        private static string CANSHOUT = "studentCanPublish";
        public bool studentCanOpenFriends = false;
        private static string CANFRIEND = "studentCanOpenFriends";
        public bool usersAreCompulsorilySynced = true;
        private static string ALLSYNC = "usersAreCompulsorilySynced";
        public string conversationGroup = "";
        //private static string CONVERSATIONGROUP = "conversationGroup";
        public bool NavigationLocked;
        private static string NAVIGATIONLOCKED = "navigationlocked";
        public static Permissions ReadXml(XElement doc)
        {
            var studentCanPublish = Boolean.Parse(doc.Element(CANSHOUT).ValueOrDefault("false"));
            var studentCanOpenFriends = Boolean.Parse(doc.Element(CANFRIEND).ValueOrDefault("false"));
            var usersAreCompulsorilySynced = false;
            if (doc.Element(ALLSYNC) != null)
                usersAreCompulsorilySynced = Boolean.Parse(doc.Element(ALLSYNC).Value);
            var permission = new Permissions("custom", studentCanOpenFriends, studentCanPublish, usersAreCompulsorilySynced);
            if (doc.Element(NAVIGATIONLOCKED) != null)
                permission.NavigationLocked = Boolean.Parse(doc.Element(NAVIGATIONLOCKED).Value);
            return permission;
        }
        public XElement WriteXml()
        {
            return new XElement(PERMISSIONS_TAG,
                new XElement(NAVIGATIONLOCKED, NavigationLocked),
                new XElement(CANSHOUT, studentCanPublish),
                new XElement(CANFRIEND, studentCanOpenFriends),
                new XElement(ALLSYNC, usersAreCompulsorilySynced));
        }
    }
    public class Group : INotifyPropertyChanged
    {
        public string id;
        public string location;
        public List<string> GroupMembers;
        public Group(string _id, string _location, List<string> _members)
        {
            id = _id;
            location = _location;
            GroupMembers = _members;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void refreshMembers()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("GroupMembers"));
        }
    }
    public class GroupSet : INotifyPropertyChanged
    {
        public string id;
        public string location;
        public int groupSize;
        public List<Group> Groups;
        public GroupSet(string _id, string _location, int _groupSize, List<Group> _groups) {
            id = _id;
            location = _location;
            groupSize = _groupSize;
            Groups = _groups;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void refreshSize()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("groupSize"));
        }
        public void refreshGroups()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Groups"));
        }
    }
    public class Slide : INotifyPropertyChanged
    {
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Slide)) return false;
            return ((Slide)obj).id == id;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is Slide)) return false;
            var foreignSlide = ((Slide)obj);
            return ((foreignSlide.id == id)
                && (foreignSlide.author == author)
                && (foreignSlide.defaultHeight == defaultHeight)
                && (foreignSlide.defaultWidth == defaultWidth)
                && (foreignSlide.exposed == exposed)
                && (foreignSlide.index == index)
                && (foreignSlide.type.Equals(type)));
        }
        public Slide(int newId, String newAuthor, TYPE newType, int newIndex, float newDefaultWidth, float newDefaultHeight)
        {
            id = newId;
            author = newAuthor;
            type = newType;
            index = newIndex;
            defaultWidth = newDefaultWidth;
            defaultHeight = newDefaultHeight;
        }
        public Slide(int newId, String newAuthor, TYPE newType, int newIndex, float newDefaultWidth, float newDefaultHeight, bool newExposed) : this(newId,newAuthor,newType,newIndex,newDefaultWidth,newDefaultHeight)
        {
            exposed = newExposed;
        }
        public Slide(int newId, String newAuthor, TYPE newType, int newIndex, float newDefaultWidth, float newDefaultHeight, bool newExposed,List<GroupSet> newGroups) : this(newId, newAuthor, newType, newIndex, newDefaultWidth, newDefaultHeight,newExposed)
        {
            GroupSets = newGroups;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void refresh()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("id"));
        }
        public void refreshIndex()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("index"));
        }
        public static Slide Empty
        {
            get
            {
                return new Slide(0, "", TYPE.SLIDE, 0, 0f, 0f);
            }
        }
        public static int ConversationFor(int id)
        {
            var sId = id.ToString();
            return Int32.Parse(string.Format("{0}400", sId.Substring(0, sId.Length - 3)));
        }
        public enum TYPE { SLIDE, POLL, THOUGHT, GROUPSLIDE };

        public float defaultWidth;
        public float defaultHeight;
        public string author;
        public int id
        {
            get;
            set;
        }
        public int index { get; set; }
        public TYPE type;
        public bool exposed;
        public List<GroupSet> GroupSets = new List<GroupSet>();
    }
    public class ApplicationLevelInformation
    {
        public ApplicationLevelInformation(int newCurrentId)
        {
            currentId = newCurrentId;
        }
        public int currentId;
    }
}