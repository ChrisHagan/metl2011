using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Divan;
using MeTLLib.Providers;

namespace MeTLLib.DataTypes
{
    public class SearchConversationDetails:ConversationDetails
    {
        public const int DEFAULT_MAX_SEARCH_RESULTS = 50;

        public int relevance { get; set; }
        public long LastModified { get; set; }
        private static readonly string TITLE_TAG = "title";
        private static readonly string AUTHOR_TAG = "author";
        private static readonly string CREATED_TAG = "created";
        private static readonly string JID_TAG = "jid";
        private static readonly string RESTRICTION_TAG = "restriction";
        private static readonly string RELEVANCE_TAG = "relevance";
        private static readonly string LASTMODIFIED_TAG = "modified";

        public SearchConversationDetails(ConversationDetails conv) : base(conv.Title, conv.Jid, conv.Author, conv.Slides, conv.Permissions, conv.Subject)
        {
            Created = conv.Created;
        }
        public SearchConversationDetails(string title, string author, string created, int relevance, string restriction, string jid, string lastModified):base(title, jid, author, new List<Slide>(), Permissions.Empty, restriction)
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
            if(match.Success)
            {
                var year = Int32.Parse(match.Groups[3].Value);
                var month =Int32.Parse(match.Groups[2].Value);
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
            var cd = new SearchConversationDetails(doc.Element(TITLE_TAG).Value,doc.Element(AUTHOR_TAG).Value, ParseDate(doc, CREATED_TAG), Int32.Parse(doc.Element(RELEVANCE_TAG).Value),doc.Element(RESTRICTION_TAG).Value,doc.Element(JID_TAG).Value, doc.Element(LASTMODIFIED_TAG).Value);
            return cd;
        }

        public static SearchConversationDetails HydrateFromServer(ConversationDetails con)
        {
            if (con == null)
                throw new ArgumentNullException();

            return HydrateFromServer(new SearchConversationDetails(con));
        }
        public static SearchConversationDetails HydrateFromServer(SearchConversationDetails scd)
        {
            if (scd == null) 
                throw new ArgumentNullException("scd", "Probably ConversationDetails is being cast as SearchConversationDetails");

            var conversation = MeTLLib.ClientFactory.Connection().DetailsOf(scd.Jid);

            scd.blacklist = conversation.blacklist;
            scd.CreatedAsTicks = conversation.CreatedAsTicks;
            scd.Permissions = conversation.Permissions;
            scd.Slides = conversation.Slides;
            scd.Tag = conversation.Tag;

            return scd;
        }
    }

    public class ConversationDetails : INotifyPropertyChanged
    {
       public bool isDeleted
        {
            get 
            {
                return Subject.ToLower() == "deleted";
            }
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
            var now = DateTimeFactory.Now();
            return string.Format("{0} at {1}", author, now);
        }
        public ConversationDetails(String title, String jid, String author, List<Slide> slides, Permissions permissions, String subject)
            : base()
        {
            this.Title = title;
            this.Jid = jid;
            this.Author = author;
            this.Slides = slides;
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
        public int NextAvailableSlideId()
        {
            return Slides.Select(s => s.id).Max() + 1;
        }
        public ConversationDetails Clone() {
            return ReadXml(WriteXml());
        }
        public string Title { get; set; }
        public string Jid { get; set; }/*The jid is a valid Xmpp jid.  If, for instance, you want
                                       * to create a room specific to this conversation so that
                                       * you can restrict broadcast, this is safe to work in Jabber 
                                       * and on the filesystem, whereas the Title is NOT. 
                                       * (If anybody finds another character that breaks it - 
                                       * obvious when history stops working - add it to the illegals 
                                       * string in generateJid).  Never mind that, we're just using a number.*/
        public string Author;
        public Permissions Permissions { get; set; }
        public System.DateTime Created;
        public long CreatedAsTicks;
        public System.DateTime LastAccessed;
        public string Tag { get; set; }
        public string Subject { get; set; }
        public List<Slide> Slides = new List<Slide>();
        public List<string> blacklist = new List<string>();
        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(WriteXml().ToString(SaveOptions.DisableFormatting));
        }
        public void Refresh()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Permissions"));
        }
        public bool IsValid
        {
            get
            {
                if (String.IsNullOrEmpty(Jid) || (String.IsNullOrEmpty(Subject)) || String.IsNullOrEmpty(Title) || String.IsNullOrEmpty(Author)) return false;
                else
                    return true;
            }
        }
 
        public bool UserHasPermission(Credentials credentials)
        {
            if (!(credentials.authorizedGroups.Select(g => g.groupKey).Contains(Subject))
                && Subject != "Unrestricted"
                && !(String.IsNullOrEmpty(Subject))
                && !(credentials.authorizedGroups.Select(su => su.groupKey).Contains("Superuser"))) return false;
            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ConversationDetails)) return false;
            return ((ConversationDetails)obj).Jid == Jid;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is ConversationDetails)) return false;
            var foreignConversationDetails = ((ConversationDetails)obj);
            return ((foreignConversationDetails.Author == Author)
                && (foreignConversationDetails.Created == Created)
                && (foreignConversationDetails.IsValid == IsValid)
                && (foreignConversationDetails.Jid == Jid)
                && (foreignConversationDetails.LastAccessed.ToString() == LastAccessed.ToString())
                && (foreignConversationDetails.Permissions.ValueEquals(Permissions))
                && (foreignConversationDetails.Slides.All(s => s.ValueEquals(Slides[foreignConversationDetails.Slides.IndexOf(s)])))
                && (foreignConversationDetails.Subject == Subject)
                && (foreignConversationDetails.Tag == Tag)
                && (foreignConversationDetails.Title == Title));
        }
        public static ConversationDetails Empty
        {
            get
            {
                return new ConversationDetails("", "", "", new List<Slide>(), Permissions.Empty, "", new DateTime(), new DateTime());
            }
        }
        public override int GetHashCode()
        {
            if (Jid == null) return 0;
            return Jid.GetHashCode();
        }
        private static readonly string TITLE_TAG = "title";
        private static readonly string AUTHOR_TAG = "author";
        private static readonly string CREATED_TAG = "created";
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
        public static ConversationDetails ReadXml(XElement doc)
        {
            var Title = doc.Element(TITLE_TAG).Value;
            var Author = doc.Element(AUTHOR_TAG).Value;
            var Created = DateTimeFactory.Parse(doc.Element(CREATED_TAG).Value);
            var Tag = doc.Element(TAG_TAG).Value;
            DateTime LastAccessed = new DateTime();
            if (doc.Element(LAST_ACCESSED_TAG) != null)
                LastAccessed = DateTimeFactory.Parse(doc.Element(LAST_ACCESSED_TAG).Value);
            var Subject = "";
            if (doc.Element(SUBJECT_TAG) != null)
                Subject = doc.Element(SUBJECT_TAG).Value;
            var Jid = doc.Element(JID_TAG).Value;
            var internalPermissions = Permissions.ReadXml(doc.Element(Permissions.PERMISSIONS_TAG));
            var Slides = doc.Descendants(SLIDE_TAG).Select(d => new Slide(
                Int32.Parse(d.Element(ID_TAG).Value),
                d.Element(AUTHOR_TAG).Value,
                d.Element(TYPE_TAG) == null ? Slide.TYPE.SLIDE : (Slide.TYPE)Enum.Parse(typeof(Slide.TYPE), d.Element(TYPE_TAG).Value),
                Int32.Parse(d.Element(INDEX_TAG).Value),
                d.Element(DEFAULT_WIDTH) != null ? float.Parse(d.Element(DEFAULT_WIDTH).Value) : 720,
                d.Element(DEFAULT_HEIGHT) != null ? float.Parse(d.Element(DEFAULT_HEIGHT).Value) : 540,
                d.Element(EXPOSED_TAG) != null ? Boolean.Parse(d.Element(EXPOSED_TAG).Value) : true
            )).ToList();
            var blacklistElements = doc.Elements(BLACKLIST_TAG);
            var blacklist = new List<string>();
            if(blacklistElements.Count() > 0)
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
                    new XElement(TYPE_TAG, (s.type == null ? Slide.TYPE.SLIDE : s.type).ToString())))
                    , blacklist.Select(b => new XElement(BLACKLIST_TAG, b)));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Permissions
    {
        public Permissions(String newLabel, bool newStudentsCanOpenFriends, bool newStudentsCanPublish, bool newUsersAreCompulsorilySynced)
        {
            Label = newLabel;
            studentCanOpenFriends = newStudentsCanOpenFriends;
            studentCanPublish = newStudentsCanPublish;
            usersAreCompulsorilySynced = newUsersAreCompulsorilySynced;
        }
        public Permissions(String newLabel, bool newStudentsCanOpenFriends, bool newStudentsCanPublish, bool newUsersAreCompulsorilySynced, String newConversationGroup)
        {
            Label = newLabel;
            studentCanOpenFriends = newStudentsCanOpenFriends;
            studentCanPublish = newStudentsCanPublish;
            usersAreCompulsorilySynced = newUsersAreCompulsorilySynced;
            conversationGroup = newConversationGroup;
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
        private static readonly Permissions[] OPTIONS = new[]{
            LECTURE_PERMISSIONS,
            LABORATORY_PERMISSIONS,
            TUTORIAL_PERMISSIONS,
            MEETING_PERMISSIONS};
        public static readonly string PERMISSIONS_TAG = "permissions";
        public string Label;
        public bool studentCanPublish = false;
        private static string CANSHOUT = "studentCanPublish";
        public bool studentCanOpenFriends = false;
        private static string CANFRIEND = "studentCanOpenFriends";
        public bool usersAreCompulsorilySynced = true;
        private static string ALLSYNC = "usersAreCompulsorilySynced";
        public string conversationGroup = "";
        private static string CONVERSATIONGROUP = "conversationGroup";
        public static Permissions ReadXml(XElement doc)
        {
            var studentCanPublish = Boolean.Parse(doc.Element(CANSHOUT).Value);
            var studentCanOpenFriends = Boolean.Parse(doc.Element(CANFRIEND).Value);
            var usersAreCompulsorilySynced = Boolean.Parse(doc.Element(ALLSYNC).Value);
            return new Permissions(null, studentCanOpenFriends, studentCanPublish, usersAreCompulsorilySynced);
        }
        public XElement WriteXml()
        {
            return new XElement(PERMISSIONS_TAG,
                new XElement(CANSHOUT, studentCanPublish),
                new XElement(CANFRIEND, studentCanOpenFriends),
                new XElement(ALLSYNC, usersAreCompulsorilySynced));
        }
    }
    public class Slide : INotifyPropertyChanged
    {
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Slide)) return false;
            return ((Slide)obj).id == id;
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
        public Slide(int newId, String newAuthor, TYPE newType, int newIndex, float newDefaultWidth, float newDefaultHeight, bool newExposed)
        {
            id = newId;
            author = newAuthor;
            type = newType;
            index = newIndex;
            defaultWidth = newDefaultWidth;
            defaultHeight = newDefaultHeight;
            exposed = newExposed;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void refresh() {
            PropertyChanged(this,new PropertyChangedEventArgs("id"));
        }
        public void refreshIndex() {
            PropertyChanged(this,new PropertyChangedEventArgs("index"));
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
        public enum TYPE { SLIDE, POLL, THOUGHT };

        public float defaultWidth;
        public float defaultHeight;
        public string author;
        public int id{
            get;set;
        }
        public int index{get;set;}
        public TYPE type;
        public bool exposed;

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