using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Divan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MeTLLib.Providers;

namespace MeTLLib.DataTypes
{
    public class ConversationDetails : INotifyPropertyChanged
    {
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
            this.LastAccessed = lastAccessed;
        }
        public ConversationDetails(String title, String jid, String author, String tag, List<Slide> slides, Permissions permissions, String subject, DateTime created, DateTime lastAccessed)
            : this(title, jid, author, slides, permissions, subject, created, lastAccessed)
        {
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
        public System.DateTime LastAccessed;
        public string Tag { get; set; }
        public string Subject { get; set; }
        public List<Slide> Slides = new List<Slide>();
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
            get { return Title != null && Author != null && Author != "Happenstance"; }
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
                && (foreignConversationDetails.LastAccessed == LastAccessed)
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
            return new ConversationDetails(Title, Jid, Author, Tag, Slides, internalPermissions, Subject, Created, LastAccessed);
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
                    new XElement(TYPE_TAG, (s.type == null ? Slide.TYPE.SLIDE : s.type).ToString()))));
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
    public class Slide
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
        public static Slide Empty
        {
            get
            {
                return new Slide(0, "", TYPE.SLIDE, 0, 0f, 0f);
            }
        }
        public static int conversationFor(int id)
        {
            var sId = id.ToString();
            return Int32.Parse(string.Format("{0}00", sId.Substring(0, sId.Length - 2)));
        }
        public enum TYPE { SLIDE, POLL, THOUGHT };

        public float defaultWidth;
        public float defaultHeight;
        public string author;
        public int id;
        public int index;
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