using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Divan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SandRibbonObjects
{
    public class ConversationDetails : INotifyPropertyChanged
    {
        public ConversationDetails() : base()
        {
            this.Permissions = new Permissions();
        }
        public int NextAvailableSlideId()
        {
            return Slides.Select(s => s.id).Max() + 1;
        }
        public string Title {get;set;}
        public string Jid { get; set;}/*The jid is a valid Xmpp jid.  If, for instance, you want
                                       * to create a room specific to this conversation so that
                                       * you can restrict broadcast, this is safe to work in Jabber 
                                       * and on the filesystem, whereas the Title is NOT. 
                                       * (If anybody finds another character that breaks it - 
                                       * obvious when history stops working - add it to the illegals 
                                       * string in generateJid).  Never mind that, we're just using a number.*/
        public string Author;
        public Permissions Permissions{get;set;}
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
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Permissions"));
        }
        public bool IsValid
        {
            get { return Title != null && Author != null && Author != "Happenstance"; }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is ConversationDetails)) return false;
            return ((ConversationDetails)obj).Jid == Jid;
        }
        public override int GetHashCode()
        {
            if (Jid == null) return 0;
            return Jid.GetHashCode();
        }
        private static readonly string TITLE_TAG = "title";
        private static readonly string AUTHOR_TAG = "author";
        private static readonly string CREATED_TAG = "created";
        private static readonly string TAG_TAG = "tag";
        private static readonly string SUBJECT_TAG = "subject";
        private static readonly string JID_TAG = "jid";
        private static readonly string ID_TAG = "id";
        private static readonly string INDEX_TAG = "index";
        private static readonly string SLIDE_TAG = "slide";
        private static readonly string TYPE_TAG = "type";
        private static readonly string EXPOSED_TAG = "exposed";
        public ConversationDetails ReadXml(XElement doc)
        {
            Title = doc.Element(TITLE_TAG).Value;
            Author = doc.Element(AUTHOR_TAG).Value;
            Created = DateTimeFactory.Parse(doc.Element(CREATED_TAG).Value);
            Tag = doc.Element(TAG_TAG).Value;
            if (doc.Element(SUBJECT_TAG) == null)
                Subject = "";
            else
                Subject = doc.Element(SUBJECT_TAG).Value;
            Jid = doc.Element(JID_TAG).Value;
            Permissions = new Permissions().ReadXml(doc.Element(Permissions.PERMISSIONS_TAG));
            Slides = doc.Descendants(SLIDE_TAG).Select(d => new Slide
            {
                author = d.Element(AUTHOR_TAG).Value,
                id = Int32.Parse(d.Element(ID_TAG).Value),
                index = Int32.Parse(d.Element(INDEX_TAG).Value),
                type = d.Element(TYPE_TAG) == null ? Slide.TYPE.SLIDE : (Slide.TYPE)Enum.Parse(typeof(Slide.TYPE), d.Element(TYPE_TAG).Value),
                exposed = d.Element(EXPOSED_TAG) != null ? Boolean.Parse(d.Element(EXPOSED_TAG).Value) : true
            }).ToList();
            return this;
        }
        public XElement WriteXml()
        {
            return 
            new XElement("conversation",
                new XElement(TITLE_TAG, Title),
                new XElement(AUTHOR_TAG, Author),
                new XElement(CREATED_TAG, Created.ToString()),
                new XElement(TAG_TAG, Tag),
                new XElement(SUBJECT_TAG, Subject),
                new XElement(JID_TAG, Jid),
                Permissions.WriteXml(),
                Slides.Select(s=>new XElement(SLIDE_TAG, 
                    new XElement(AUTHOR_TAG, s.author),
                    new XElement(ID_TAG, s.id.ToString()),
                    new XElement(INDEX_TAG, s.index.ToString()),
                    new XElement(EXPOSED_TAG, s.exposed.ToString()),
                    new XElement(TYPE_TAG, (s.type == null? Slide.TYPE.SLIDE:s.type).ToString()))));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Permissions
    {
        public static Permissions InferredTypeOf(Permissions permissions)
        {
            var typeOfPermissions = new[]{LECTURE_PERMISSIONS, LABORATORY_PERMISSIONS, TUTORIAL_PERMISSIONS, MEETING_PERMISSIONS}.Where(
                   p=>p.studentCanOpenFriends==permissions.studentCanOpenFriends &&
                       p.studentCanPublish == permissions.studentCanPublish &&
                       p.usersAreCompulsorilySynced == permissions.usersAreCompulsorilySynced).FirstOrDefault();
            if(typeOfPermissions != null) return typeOfPermissions;
            return CUSTOM_PERMISSIONS;
        }
        public static Permissions CUSTOM_PERMISSIONS = new Permissions{
            Label="custom",
            studentCanPublish = false,
            studentCanOpenFriends = false,
            usersAreCompulsorilySynced = false
        };
        public static Permissions LECTURE_PERMISSIONS = new Permissions
        {
            Label="lecture",
            studentCanPublish = false,
            studentCanOpenFriends = false,
            usersAreCompulsorilySynced = true
        };
        public static Permissions LABORATORY_PERMISSIONS = new Permissions
        {
            Label="laboratory",
            studentCanPublish = false,
            studentCanOpenFriends = true,
            usersAreCompulsorilySynced = false
        };
        public static Permissions TUTORIAL_PERMISSIONS = new Permissions
        {
            Label="tutorial",
            studentCanPublish = true,
            studentCanOpenFriends = true,
            usersAreCompulsorilySynced = false
        };
        public static Permissions MEETING_PERMISSIONS = new Permissions
        {
            Label="meeting",
            studentCanPublish = true,
            studentCanOpenFriends = true,
            usersAreCompulsorilySynced = true
        };
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
        public Permissions ReadXml(XElement doc)
        {
            studentCanPublish = Boolean.Parse(doc.Element(CANSHOUT).Value);
            studentCanOpenFriends = Boolean.Parse(doc.Element(CANFRIEND).Value);
            usersAreCompulsorilySynced = Boolean.Parse(doc.Element(ALLSYNC).Value);
            return this;
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
        public static int conversationFor(int id) {
            var sId = id.ToString();
            return Int32.Parse(string.Format("{0}00",sId.Substring(0,sId.Length-2)));
        }
        public enum TYPE { SLIDE, POLL, THOUGHT };
        public string author;
        public int id;
        public int index;
        public TYPE type;
        public bool exposed;
    }
    public class ApplicationLevelInformation
    {
        public int currentId;
    }
}