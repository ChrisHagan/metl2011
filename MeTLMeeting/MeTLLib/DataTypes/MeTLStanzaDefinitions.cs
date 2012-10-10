using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using agsXMPP.Xml.Dom;
using System.Collections.Generic;
using Path = System.IO.Path;
using MeTLLib.Providers;
using System.Threading;
using MeTLLib.Providers.Connection;
using System.Diagnostics;
using MeTLLib.Utilities;
using System.Collections.ObjectModel;


namespace MeTLLib.DataTypes
{
    public class MeTLStanzasConstructor
    {
        //Any new metlStanzas need to be instantiated here to ensure that the xml parser registers them.
        public MeTLStanzasConstructor()
        {
            new CommandParameterProvider();
            new MeTLStanzas.Ink();
            new MeTLStanzas.Quiz();
            new MeTLStanzas.Image();
            new MeTLStanzas.TextBox();
            new MeTLStanzas.DirtyInk();
            new MeTLStanzas.DirtyText();
            new MeTLStanzas.DirtyImage();
            new MeTLStanzas.LiveWindow();
            new MeTLStanzas.QuizOption();
            new MeTLStanzas.FileResource();
            new MeTLStanzas.QuizResponse();
            new MeTLStanzas.DirtyElement();
            new MeTLStanzas.DirtyAutoshape();
            new MeTLStanzas.DirtyLiveWindow();
            new MeTLStanzas.TeacherStatusStanza();
            new MeTLStanzas.ScreenshotSubmission();
            new MeTLStanzas.BlackList();
            new MeTLStanzas.MoveDeltaStanza();
            new MeTLStanzas.TextBoxIdentityStanza();
            new MeTLStanzas.InkIdentityStanza();
            new MeTLStanzas.ImageIdentityStanza();
        }
    }

    public class TimeStampedMessage
    {
        public static long getTimestamp(Element message)
        {
            var timestamp = 0L;

            if (message.GetAttribute("type") == "error")
            {
                Trace.TraceError("Wire received error message: {0}", message);
                return timestamp;
            }
            if (message.HasAttribute("timestamp"))
            {
                timestamp = message.GetAttributeLong("timestamp");
            }
            else
            {
                NodeList subMessage = message.ChildNodes;
                if (subMessage != null)
                {
                    foreach (Node subNode in subMessage)
                    {
                        var tempNode = subNode as Element;
                        if (tempNode != null)
                        {
                            if (tempNode.TagName == "metlMetaData")
                            {
                                subMessage = (tempNode as Element).ChildNodes;
                                foreach (Node subsubNode in subMessage)
                                {
                                    if ((subsubNode as Element) != null)
                                    {
                                        if ((subsubNode as Element).TagName == "timestamp")
                                        {
                                            timestamp = long.Parse(subsubNode.Value);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return timestamp;
        }
    }

    public class TargettedElement
    {
        public TargettedElement(int Slide, string Author, string Target, Privacy Privacy, string Identity, long Timestamp)
        {
            slide = Slide;
            author = Author;
            target = Target;
            privacy = Privacy;
            identity = Identity;
            timestamp = Timestamp;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedElement)) return false;
            var foreignTE = (TargettedElement)obj;
            return (HasSameIdentity(foreignTE.identity) && HasSameAuthor(foreignTE.author) && HasSamePrivacy(foreignTE.privacy) && foreignTE.slide == slide && HasSameTarget(foreignTE.target));
        }

        public bool HasSameIdentity(string theirIdentity)
        {
            return String.Compare(identity, theirIdentity, true) == 0;
        }

        public bool HasSamePrivacy(Privacy theirPrivacy)
        {
            return privacy == theirPrivacy;
        }

        public bool HasSameAuthor(string theirAuthor)
        {
            return String.Compare(author, theirAuthor, true) == 0;
        }

        public bool HasSameTarget(string theirTarget)
        {
            return String.Compare(target, theirTarget, true) == 0;
        }

        public string author { get; set; }
        public string identity { get; protected set; }
        public string target;
        public Privacy privacy;
        public int slide;
        public long timestamp;

    }
    public class TargettedMoveDelta : TargettedElement
    {
        public TargettedMoveDelta(int slide, string author, string target, Privacy privacy, string identity, long timestamp)
            : base(slide, author, target, privacy, identity,timestamp)
        {
            Initialise();
        }

        /// <summary>
        /// Shallow copy constructor
        /// </summary>
        /// <param name="copyTmd"></param>
        private TargettedMoveDelta(TargettedMoveDelta copyTmd)
            : base(copyTmd.slide, copyTmd.author, copyTmd.target, copyTmd.privacy, copyTmd.identity, copyTmd.timestamp)
        {
            xTranslate = copyTmd.xTranslate;
            yTranslate = copyTmd.yTranslate;
            xScale = copyTmd.xScale;
            yScale = copyTmd.yScale;
            newPrivacy = copyTmd.newPrivacy;
            isDeleted = copyTmd.isDeleted;
            timestamp = copyTmd.timestamp;
        }

        public double xTranslate { get; set; }
        public double yTranslate { get; set; }
        public double xScale { get; set; }
        public double yScale { get; set; }
        public Privacy newPrivacy { get; set; }
        public bool isDeleted { get; set; }

        private void Initialise()
        {
            // set defaults
            xTranslate = yTranslate = 0;
            xScale = yScale = 1;
            newPrivacy = Privacy.NotSet;
        }

        private readonly HashSet<MeTLStanzas.ElementIdentity> _inkIds = new HashSet<MeTLStanzas.ElementIdentity>();
        private ReadOnlyCollection<MeTLStanzas.ElementIdentity> _inkIdsView;
        public ReadOnlyCollection<MeTLStanzas.ElementIdentity> inkIds
        {
            get
            {
                if (_inkIdsView == null)
                {
                    _inkIdsView = new ReadOnlyCollection<MeTLStanzas.ElementIdentity>(_inkIds.ToList());
                }
                return _inkIdsView;
            }
        }
        internal void AddInkId(MeTLStanzas.ElementIdentity elemId)
        {
            _inkIds.Add(elemId);
        }
        private void AddInkId(string identity)
        {
            _inkIds.Add(new MeTLStanzas.ElementIdentity(identity));
        }

        private readonly HashSet<MeTLStanzas.ElementIdentity> _textIds = new HashSet<MeTLStanzas.ElementIdentity>();
        private ReadOnlyCollection<MeTLStanzas.ElementIdentity> _textIdsView;
        public ReadOnlyCollection<MeTLStanzas.ElementIdentity> textIds
        {
            get
            {
                if (_textIdsView == null)
                {
                    _textIdsView = new ReadOnlyCollection<MeTLStanzas.ElementIdentity>(_textIds.ToList());
                }
                return _textIdsView;
            }
        }
        internal void AddTextId(MeTLStanzas.ElementIdentity elemId)
        {
            _textIds.Add(elemId);
        }
        private void AddTextId(string identity)
        {
            _textIds.Add(new MeTLStanzas.ElementIdentity(identity));
        }

        private readonly HashSet<MeTLStanzas.ElementIdentity> _imageIds = new HashSet<MeTLStanzas.ElementIdentity>();
        private ReadOnlyCollection<MeTLStanzas.ElementIdentity> _imageIdsView;
        public ReadOnlyCollection<MeTLStanzas.ElementIdentity> imageIds
        {
            get
            {
                if (_imageIdsView == null)
                {
                    _imageIdsView = new ReadOnlyCollection<MeTLStanzas.ElementIdentity>(_imageIds.ToList());
                }
                return _imageIdsView;
            }
        }
        internal void AddImageId(MeTLStanzas.ElementIdentity elemId)
        {
            _imageIds.Add(elemId);
        }
        private void AddImageId(string identity)
        {
            _imageIds.Add(new MeTLStanzas.ElementIdentity(identity));
        }

        public new bool ValueEquals(object obj)
        {
            var moveDelta = obj as TargettedMoveDelta;
            if (moveDelta == null) return false;

            return (base.ValueEquals(obj) &&
                MeTLMath.ApproxEqual(moveDelta.xTranslate, xTranslate) &&
                MeTLMath.ApproxEqual(moveDelta.yTranslate, yTranslate) &&
                MeTLMath.ApproxEqual(moveDelta.xScale, xScale) &&
                MeTLMath.ApproxEqual(moveDelta.yScale, yScale) &&
                moveDelta.newPrivacy == newPrivacy &&
                IsNullOrEqual(moveDelta._inkIds, _inkIds) &&
                IsNullOrEqual(moveDelta._textIds, _textIds) &&
                IsNullOrEqual(moveDelta._imageIds, _imageIds));
        }

        private bool IsNullOrEqual(HashSet<MeTLStanzas.ElementIdentity> elemIdsA, HashSet<MeTLStanzas.ElementIdentity> elemIdsB)
        {
            return (elemIdsA == null && elemIdsB == null) || elemIdsA.SetEquals(elemIdsB);
        }

        public static TargettedMoveDelta Create(int slide, string author, string target, Privacy privacy, long timestamp, IEnumerable<Stroke> moveStrokes, IEnumerable<TextBox> moveTexts, IEnumerable<Image> moveImages)
        {
            // identity is set in the MoveDeltaStanza constructor
            var targettedMoveDelta = new TargettedMoveDelta(slide, author, target, privacy, string.Empty, timestamp);

            AddFromCollection<Stroke>(moveStrokes, (s) => targettedMoveDelta.AddInkId(s.tag().id));
            AddFromCollection<TextBox>(moveTexts, (t) => targettedMoveDelta.AddTextId(t.tag().id));
            AddFromCollection<Image>(moveImages, (i) => targettedMoveDelta.AddImageId(i.tag().id));

            return targettedMoveDelta;
        }

        public static TargettedMoveDelta CreateAdjuster(TargettedMoveDelta tmd, Privacy replacementPrivacy, IEnumerable<TargettedStroke> strokes, IEnumerable<TargettedTextBox> texts, IEnumerable<TargettedImage> images)
        {
            var targettedMoveDelta = new TargettedMoveDelta(tmd);
            targettedMoveDelta.newPrivacy = replacementPrivacy;

            AddFromCollection<TargettedStroke>(strokes, (s) => targettedMoveDelta.AddInkId(s.identity));
            AddFromCollection<TargettedTextBox>(texts, (t) => targettedMoveDelta.AddTextId(t.identity));
            AddFromCollection<TargettedImage>(images, (i) => targettedMoveDelta.AddImageId(i.identity));

            return targettedMoveDelta;
        }

        public static TargettedMoveDelta CreateDirtier(TargettedMoveDelta tmd, Privacy replacementPrivacy, IEnumerable<TargettedStroke> strokes, IEnumerable<TargettedTextBox> texts, IEnumerable<TargettedImage> images)
        {
            var targettedMoveDelta = CreateAdjuster(tmd, replacementPrivacy, strokes, texts, images);
            targettedMoveDelta.isDeleted = true;

            return targettedMoveDelta;
        }

        private static void AddFromCollection<T>(IEnumerable<T> elements, Action<T> addElem)
        {
            foreach (var elem in elements)
            {
                addElem(elem);
            }
        }
    }
    public class TargettedSubmission : TargettedElement
    {
        public TargettedSubmission(int Slide, string Author, string Target, Privacy Privacy, long Timestamp, string Identity, string Url, string Title, long Time, List<MeTLStanzas.BlackListedUser> Blacklisted)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            url = Url;
            title = Title;
            time = Time;
            blacklisted = Blacklisted;
        }
        public new bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedSubmission)) return false;
            var submission = obj as TargettedSubmission;
            return (base.ValueEquals(obj) && submission.url == url && submission.title == title && submission.time == time && ((submission.blacklisted == null && blacklisted == null) ||
                submission.blacklisted.Where(x => !blacklisted.Any(x1 => x1 == x)).Union(blacklisted.Where(x => !submission.blacklisted.Any(x1 => x1 == x))).Count() == 0));

        }
        public string url { get; set; }
        public string title { get; set; }
        public long time { get; set; }
        public List<MeTLStanzas.BlackListedUser> blacklisted { get; set; }
    }
    public class TargettedStroke : TargettedElement
    {
        public TargettedStroke(int Slide, string Author, string Target, Privacy Privacy, string Identity, long Timestamp, Stroke Stroke, double StartingChecksum)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            stroke = Stroke;
            startingChecksum = StartingChecksum;
        }
        public TargettedStroke(int Slide, string Author, string Target, Privacy Privacy, string Identity, long Timestamp, Stroke Stroke, double StartingChecksum, string strokeStartingColor)
            : this(Slide, Author, Target, Privacy, Identity, Timestamp, Stroke, StartingChecksum)
        {
            startingColor = strokeStartingColor;
        }

        public TargettedStroke AlterPrivacy(Privacy newPrivacy)
        {
            var newStroke = (TargettedStroke)this.MemberwiseClone();
            if (newPrivacy == privacy || newPrivacy == Privacy.NotSet)
                return this;

            newStroke.privacy = newPrivacy;

            return newStroke;
        }

        public TargettedStroke AdjustVisual(double xTranslate, double yTranslate, double xScale, double yScale)
        {
            var newStroke = (TargettedStroke)this.MemberwiseClone();
            if (xTranslate == 0.0 && yTranslate == 0.0 && xScale == 1.0 && yScale == 1.0)
                return this;

            var transformMatrix = new Matrix();
            transformMatrix.Scale(xScale, yScale);
            transformMatrix.Translate(xTranslate, yTranslate);
            var newS = newStroke.stroke.Clone();
            newS.Transform(transformMatrix, false);
            return newStroke;
        }

        public new bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedStroke)) return false;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj) && ((TargettedStroke)obj).stroke.Equals(stroke) && ((TargettedStroke)obj).startingChecksum == startingChecksum);
        }
        public Stroke stroke;
        public double startingChecksum;
        public string startingColor;
    }
    public class TargettedFile : TargettedElement
    {
        public TargettedFile(int Slide, string Author, string Target, Privacy Privacy, string Identity, long Timestamp, string Url, string UploadTime, long Size, string Name)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            url = Url;
            uploadTime = UploadTime;
            size = Size;
            name = Name;
            conversationJid = slide - ((Slide % 1000) % 400);
        }
        public new bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedFile)) return false;
            var foreignFile = (TargettedFile)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && foreignFile.url == url
                && foreignFile.uploadTime == uploadTime
                && foreignFile.size == size
                && foreignFile.name == name);
        }
        public string url { get; set; }
        public int conversationJid { get; set; }
        public string uploadTime { get; set; }
        public long size { get; set; }
        public string name { get; set; }
    }
    public class TargettedImage : TargettedElement
    {
        public TargettedImage(int Slide, string Author, string Target, Privacy Privacy, string Identity, Image Image, long Timestamp)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            image = Image;
        }
        public TargettedImage(int Slide, string Author, string Target, Privacy Privacy, MeTLStanzas.Image ImageSpecification, string Identity, long Timestamp)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            imageSpecification = ImageSpecification;
        }

        public TargettedImage AlterPrivacy(Privacy newPrivacy)
        {
            var newImage = (TargettedImage)this.MemberwiseClone();
            if (newPrivacy == privacy || newPrivacy == Privacy.NotSet)
                return this;

            newImage.privacy = newPrivacy;

            //return this;
            return newImage;
        }

        public TargettedImage AdjustVisual(double xTranslate, double yTranslate, double xScale, double yScale)
        {
            var newImage = (TargettedImage)this.MemberwiseClone();
            newImage.imageSpecification.x += xTranslate;
            newImage.imageSpecification.y += yTranslate;
            newImage.imageSpecification.width *= xScale;
            newImage.imageSpecification.height *= yScale;
            return newImage;
            //return this;
        }

        public System.Windows.Controls.Image imageProperty;
        public MeTLStanzas.Image imageSpecification;
        public MeTLServerAddress server;
        private IWebClient downloader;
        private HttpResourceProvider provider;
        public void injectDependencies(MeTLServerAddress server, IWebClient downloader, HttpResourceProvider provider)
        {
            if (imageSpecification == null) imageSpecification = new MeTLStanzas.Image(this);
            this.server = server;
            this.downloader = downloader;
            this.provider = provider;
            imageSpecification.injectDependencies(server, downloader, provider);
        }
        public System.Windows.Controls.Image image
        {
            get
            {
                if (server != null) imageSpecification.injectDependencies(server, downloader, provider);
                if (imageSpecification == null) imageSpecification = new MeTLStanzas.Image(this);
                return imageSpecification.forceEvaluation();
            }
            set
            {
                if (String.IsNullOrEmpty(identity))
                {
                    try
                    {
                        identity = value.tag().id;
                    }
                    catch
                    {
                        identity = string.Format("{0}:{1}", author, DateTimeFactory.Now());
                    }
                }
                value.tag(new ImageTag(author, privacy, identity, value.tag().isBackground,value.tag().timestamp));
                imageProperty = value;
            }
        }
    }

    public class TargettedTextBox : TargettedElement
    {
        public TargettedTextBox(int Slide, string Author, string Target, Privacy Privacy, string Identity, TextBox TextBox, long Timestamp)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            box = TextBox;
            boxProperty = box;
        }
        public TargettedTextBox(int Slide, string Author, string Target, Privacy Privacy, MeTLStanzas.TextBox BoxSpecification, string Identity, long Timestamp)
            : base(Slide, Author, Target, Privacy, Identity, Timestamp)
        {
            boxSpecification = BoxSpecification;
        }
        public new bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedTextBox)) return false;
            var foreign = (TargettedTextBox)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && foreign.identity == identity
                && foreign.boxProperty.Equals(boxProperty)
                && foreign.boxSpecification == boxSpecification
                && foreign.box.Equals(box));
        }

        public TargettedTextBox AlterPrivacy(Privacy newPrivacy)
        {
            var newBox = (TargettedTextBox)this.MemberwiseClone();
            if (newPrivacy == privacy || newPrivacy == Privacy.NotSet)
                return this;

            newBox.privacy = newPrivacy;
            return newBox;
            //return this;
        }

        public TargettedTextBox AdjustVisual(double xTranslate, double yTranslate, double xScale, double yScale)
        {
            var newBox = (TargettedTextBox)this.MemberwiseClone();
            newBox.boxSpecification.x += xTranslate;
            newBox.boxSpecification.y += yTranslate;
            newBox.boxSpecification.width *= xScale;
            newBox.boxSpecification.height *= yScale;
            return newBox;
            //return this;
        }

        public TextBox boxProperty;
        public MeTLStanzas.TextBox boxSpecification;
        public System.Windows.Controls.TextBox box
        {
            get
            {
                if (boxSpecification == null) boxSpecification = new MeTLStanzas.TextBox(this);
                System.Windows.Controls.TextBox reified = null;
                reified = boxSpecification.forceEvaluation();
                identity = reified.tag().id;
                return reified;
            }
            set
            {
                string internalIdentity;
                try
                {
                    internalIdentity = value.tag().id;
                }
                catch (Exception)
                {
                    if (String.IsNullOrEmpty(identity))
                        identity = string.Format("{0}:{1}", author, DateTimeFactory.Now());
                    value.tag(new TextTag(author, privacy, identity, timestamp));
                    internalIdentity = value.tag().id;
                }
                identity = internalIdentity;
                boxProperty = value;
            }
        }
    }
    public class TargettedDirtyElement : TargettedElement
    {
        public TargettedDirtyElement(int Slide, string Author, string Target, Privacy Privacy, string Identifier, long Timestamp)
            : base(Slide, Author, Target, Privacy, Identifier, Timestamp)
        {
        }
        public new bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedDirtyElement)) return false;
            var foreign = (TargettedDirtyElement)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && HasSameIdentity(foreign.identity));
        }
    }
    public class SelectedIdentity
    {
        public SelectedIdentity(string Id, string Target)
        {
            id = Id;
            target = Target;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is SelectedIdentity)) return false;
            var foreign = (SelectedIdentity)obj;
            return (foreign.id == id && foreign.target == target);
        }
        public string id;
        public string target;
    }
    public class MeTLStanzas
    {
        public static string METL_NS = "monash:metl";
        public static readonly int PRECISION = 1;
        public static readonly string tagTag = "tag";
        public static readonly string xTag = "x";
        public static readonly string yTag = "y";
        public static readonly string targetTag = "target";
        public static readonly string privacyTag = "privacy";
        public static readonly string authorTag = "author";
        public static readonly string timestampTag = "timestamp";
        public static readonly string slideTag = "slide";
        public static readonly string answererTag = "answerer";
        public static readonly string identityTag = "identity";

        public class MeTLElement : Element
        {
            public Element inner { get; private set; }
            public MeTLElement()
            {
            }
            public MeTLElement(Element foreign)
            {
                inner = foreign;
                timestamp = getTimestamp();
            }
            public long timestamp;
            private long getTimestamp()
            {
                var timestamp = 0L;
                var message = inner;
                if (message.GetAttribute("type") == "error")
                {
                    Trace.TraceError("Wire received error message: {0}", message);
                    return timestamp;
                }
                if (message.HasAttribute("timestamp"))
                    timestamp = message.GetAttributeLong("timestamp");
                else
                    long.Parse(GetTag("timestamp"));
                return timestamp;
            }
        }

        public class TimestampedMeTLElement 
        {
            public long timestamp { get; private set; }
            public Element element { get; private set; }

            private TimestampedMeTLElement()
            {

            }

            public TimestampedMeTLElement(Element foreign)
            {
                element = foreign;
                timestamp = getTimestamp(foreign);
            }

            public TimestampedMeTLElement(Element foreign, long foreigntimestamp)
            {
                element = foreign;
                timestamp = foreigntimestamp;
            }

            private static long getTimestamp(Element foreign)
            {
                var timestamp = 0L;                
                var message = foreign as Element;
                if (message.GetAttribute("type") == "error")
                {
                    Trace.TraceError("Wire received error message: {0}", message);
                    return timestamp;
                }
                if (message.HasAttribute("timestamp"))
                    timestamp = message.GetAttributeLong("timestamp");
                else
                {
                    var metaData = message.SelectSingleElement("metlMetaData",true);
                    if (metaData != null)
                        timestamp = long.Parse(metaData.GetTag("timestamp", true));
                }
                return timestamp;
            }

        }

        public class TeacherStatusStanza : Element
        {
            static TeacherStatusStanza()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TeacherStatusStanza.TAG, METL_NS, typeof(TeacherStatusStanza));
            }
            public static readonly string TAG = "teacherstatus";
            public TeacherStatusStanza()
            {
                this.TagName = TAG;
                this.Namespace = METL_NS;
            }
            public TeacherStatusStanza(TeacherStatus status)
                : this()
            {
                this.status = status;
            }

            public static readonly string whereTag = "where";

            public TeacherStatus status
            {
                get
                {
                    return new TeacherStatus
                               {
                                   Conversation = GetTag(whereTag),
                                   Teacher = GetTag(identityTag),
                                   Slide = GetTag(slideTag)
                               };
                }
                set
                {
                    SetTag(identityTag, value.Teacher);
                    SetTag(slideTag, value.Slide);
                    SetTag(whereTag, value.Conversation);

                }
            }
        }
        public class Ink : Element
        {
            static Ink()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(Ink.TAG, METL_NS, typeof(Ink));
            }
            public readonly static string TAG = "ink";
            public Ink()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Ink(TargettedStroke stroke)
                : this()
            {
                this.Stroke = stroke;
            }
            private string pointsTag = "points";
            private string colorTag = "color";
            private string thicknessTag = "thickness";
            private string highlighterTag = "highlight";
            private string sumTag = "checksum";
            private string startingSumTag = "startingSum";
            //private string startingColorTag = "startingColor";

            public TargettedStroke Stroke
            {
                get
                {
                    var points = stringToPoints(GetTag(pointsTag));
                    if (points.Count == 0) points = new StylusPointCollection(
                        new StylusPoint[]{
                           new StylusPoint(0,0,0f)
                        }
                    );
                    var stroke = new Stroke(points, new DrawingAttributes { Color = stringToColor(GetTag(colorTag)) });

                    stroke.DrawingAttributes.IsHighlighter = Boolean.Parse(GetTag(highlighterTag));
                    stroke.DrawingAttributes.Width = Double.Parse(GetTag(thicknessTag));
                    stroke.DrawingAttributes.Height = Double.Parse(GetTag(thicknessTag));

                    double startingSum = 0;
                    double sum = 0;
                    if (HasTag(sumTag))
                    {
                        sum = Double.Parse(GetTag(sumTag));
                        startingSum = sum;
                    }
                    if (HasTag(startingSumTag))
                    {
                        startingSum = Double.Parse(GetTag(startingSumTag));
                    }

                    stroke.AddPropertyData(stroke.sumId(), sum);
                    stroke.AddPropertyData(stroke.startingId(), startingSum);

                    string identity = HasTag(identityTag) ? GetTag(identityTag) : GetTag(startingSumTag);
                    long timestamp = HasTag(timestampTag) ? long.Parse(GetTag(timestampTag)) : 0L;

                    Privacy privacy = (Privacy)GetTagEnum(privacyTag, typeof(Privacy));
                    stroke.tag(new StrokeTag(
                        GetTag(authorTag), privacy, identity,
                        GetTag(startingSumTag) == null ? stroke.sum().checksum : Double.Parse(GetTag(startingSumTag)),
                        Boolean.Parse(GetTag(highlighterTag)), timestamp));
                    var targettedStroke = new TargettedStroke(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), privacy, identity, timestamp, stroke, startingSum);
                    return targettedStroke;
                }
                set
                {
                    double startingSum;
                    try
                    {
                        startingSum = value.stroke.startingSum();
                    }
                    catch (Exception)
                    {
                        startingSum = value.stroke.sum().checksum;
                    }
                    this.SetTag(identityTag, value.stroke.tag().id);
                    this.SetTag(sumTag, value.stroke.sum().checksum.ToString());
                    this.SetTag(startingSumTag, startingSum);
                    this.SetTag(pointsTag, strokeToPoints(value.stroke));
                    this.SetTag(colorTag, strokeToColor(value.stroke));
                    this.SetTag(thicknessTag, value.stroke.DrawingAttributes.Width.ToString());
                    this.SetTag(highlighterTag, value.stroke.DrawingAttributes.IsHighlighter.ToString());
                    this.SetTag(authorTag, value.author);
                    this.SetTag(timestampTag, value.timestamp);
                    this.SetTag(targetTag, value.target);
                    this.SetTag(privacyTag, value.privacy.ToString());
                    this.SetTag(slideTag, value.slide);
                }
            }
            public static string strokeToPoints(Stroke s)
            {
                return string.Join(" ", s.StylusPoints.Select(
                    p => string.Format("{0} {1} {2}",
                        Math.Round(p.X, PRECISION),
                        Math.Round(p.Y, PRECISION),
                        (int)(255 * p.PressureFactor))).ToArray());
            }
            public static StylusPointCollection stringToPoints(string s)
            {
                var pointInfo = s.Split(' ');
                if (pointInfo.Count() % 3 != 0) throw new InvalidDataException("The point info in a compressed string must be in groups of three numbers, x, y and pressure.");
                var points = new StylusPointCollection();
                foreach (var p in pointInfo)
                {
                    Double.Parse(p);
                }
                for (int i = 0; i < pointInfo.Count(); )
                {
                    var X = Double.Parse(pointInfo[i++]);
                    var Y = Double.Parse(pointInfo[i++]);
                    var PressureFactor = (Double.Parse(pointInfo[i++]) / 255.0);
                    if (!Double.IsNaN(X) && !Double.IsNaN(Y) && !Double.IsNaN(PressureFactor))
                        points.Add(new StylusPoint
                        {
                            X = X,
                            Y = Y,
                            PressureFactor = (float)PressureFactor
                        });
                }
                return points;
            }
            public static string strokeToColor(Stroke s)
            {
                return colorToString(s.DrawingAttributes.Color);
            }
            public static string colorToString(Color color)
            {
                return string.Format("{0} {1} {2} {3}", color.R, color.G, color.B, color.A);
            }
            public static Color stringToColor(string s)
            {
                try
                {
                    if (s.StartsWith("#"))
                    {
                        return (Color)ColorConverter.ConvertFromString(s);
                    }
                    var colorInfo = s.Split(' ');
                    if (colorInfo.Count() % 4 != 0) throw new InvalidDataException("The color info in a compressed stroke should consist of four integers between 0 and 255 (bytes), space separated and representing RGBA in that order.");
                    return new Color
                    {
                        R = Byte.Parse(colorInfo[0]),
                        G = Byte.Parse(colorInfo[1]),
                        B = Byte.Parse(colorInfo[2]),
                        A = Byte.Parse(colorInfo[3])
                    };
                }
                catch (Exception)
                {
                    return Colors.Black;
                }
            }
        }
        public class MoveDeltaStanza : Element
        {
            #region xml element tags
            static readonly string TAG = "moveDelta";
            static readonly string XTRANSLATE_TAG = "xTranslate";
            static readonly string YTRANSLATE_TAG = "yTranslate";
            static readonly string XSCALE_TAG = "xScale";
            static readonly string YSCALE_TAG = "yScale";
            static readonly string NEWPRIVACY_TAG = "newPrivacy";
            static readonly string ISDELETED_TAG = "isDeleted";
            static readonly string INKIDS_TAG = "inkIds";
            static readonly string TEXTIDS_TAG = "textIds";
            static readonly string IMAGEIDS_TAG = "imageIds";
            #endregion

            static MoveDeltaStanza()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(MoveDeltaStanza));
            }

            public MoveDeltaStanza()
            {
                this.TagName = TAG;
                this.Namespace = METL_NS;
            }

            public MoveDeltaStanza(TargettedMoveDelta moveDelta)
                : this()
            {
                this.parameters = moveDelta;
            }

            public TargettedMoveDelta parameters
            {
                get
                {
                    var timestamp = HasTag(timestampTag) ? GetTag(timestampTag) : "-1L";
                    var moveDelta = new TargettedMoveDelta(int.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), (Privacy)GetTagEnum(privacyTag, typeof(Privacy)), GetTag(identityTag), long.Parse(timestamp));

                    moveDelta.xTranslate = GetTagDouble(XTRANSLATE_TAG);
                    moveDelta.yTranslate = GetTagDouble(YTRANSLATE_TAG);
                    moveDelta.xScale = GetTagDouble(XSCALE_TAG);
                    moveDelta.yScale = GetTagDouble(YSCALE_TAG);
                    moveDelta.newPrivacy = (Privacy)GetTagEnum(NEWPRIVACY_TAG, typeof(Privacy));
                    moveDelta.isDeleted = GetTagBool(ISDELETED_TAG);

                    GetChildren<InkIdentityStanza>(moveDelta, INKIDS_TAG, (elemId) => moveDelta.AddInkId(elemId));
                    GetChildren<TextBoxIdentityStanza>(moveDelta, TEXTIDS_TAG, (elemId) => moveDelta.AddTextId(elemId));
                    GetChildren<ImageIdentityStanza>(moveDelta, IMAGEIDS_TAG, (elemId) => moveDelta.AddImageId(elemId));

                    return moveDelta;
                }
                set
                {
                    SetTag(identityTag, TAG);
                    SetTag(slideTag, value.slide);
                    SetTag(authorTag, value.author);
                    SetTag(targetTag, value.target);
                    SetTag(privacyTag, value.privacy.ToString());
                    SetTag(timestampTag, value.timestamp);

                    SetTag(XTRANSLATE_TAG, value.xTranslate);
                    SetTag(YTRANSLATE_TAG, value.yTranslate);
                    SetTag(XSCALE_TAG, value.xScale);
                    SetTag(YSCALE_TAG, value.yScale);
                    SetTag(NEWPRIVACY_TAG, value.newPrivacy.ToString());
                    SetTag(ISDELETED_TAG, value.isDeleted);

                    SetChildren<InkIdentityStanza>(INKIDS_TAG, value.inkIds, (elemId) => new InkIdentityStanza(elemId));
                    SetChildren<TextBoxIdentityStanza>(TEXTIDS_TAG, value.textIds, (elemId) => new TextBoxIdentityStanza(elemId));
                    SetChildren<ImageIdentityStanza>(IMAGEIDS_TAG, value.imageIds, (elemId) => new ImageIdentityStanza(elemId));
                }
            }

            void SetChildren<T>(string tagName, IEnumerable<ElementIdentity> elementIds, Func<ElementIdentity, T> creator) where T : ElementIdentityStanza, new()
            {
                var childElem = new Element(tagName);
                foreach (var elemId in elementIds)
                {
                    var elemStanza = creator(elemId);
                    childElem.AddChild(elemStanza);
                }
                AddChild(childElem);
            }

            void GetChildren<T>(TargettedMoveDelta moveDelta, string elementTag, Action<MeTLStanzas.ElementIdentity> addElem) where T : ElementIdentityStanza, new()
            {
                var elementIds = SelectSingleElement(elementTag);
                if (elementIds != null)
                {
                    foreach (var elemId in elementIds.SelectElements<T>())
                    {
                        addElem(elemId.identity);
                    }
                }
            }
        }


        public class TextBox : Element
        {
            static TextBox()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(TextBox));
            }
            public readonly static string TAG = "textbox";
            public TextBox()
            {
                this.TagName = TAG;
                this.Namespace = METL_NS;
            }
            public TextBox(TargettedTextBox textBox)
                : this()
            {
                Box = textBox;
            }
            public System.Windows.Controls.TextBox forceEvaluation()
            {
                System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox
                {
                    FontWeight = weight,
                    FontFamily = family,
                    FontSize = size,
                    FontStyle = style,
                    Foreground = color,
                    TextDecorations = decoration,
                    Tag = tag,
                    Text = text,
                    Height = height,
                    Width = width,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.WrapWithOverflow                    
                };
                textBox.tag(new TextTag(Box.author, Box.privacy, Box.identity, Box.timestamp));                
                InkCanvas.SetLeft(textBox, x);                
                InkCanvas.SetTop(textBox, y);
                return textBox;
            }
            public TargettedTextBox Box
            {
                get
                {
                    var box = new TargettedTextBox(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), (Privacy)GetTagEnum(privacyTag, typeof(Privacy)), this, GetTag(identityTag), long.Parse(GetTag(timestampTag)));
                    return box;
                }
                set
                {
                    if (value.boxProperty == null)
                        SetWithTextStanza(value);
                    else
                        SetWithTextControl(value);

                    this.SetTag(authorTag, value.author);
                    this.SetTag(identityTag, value.identity); // value.boxProperty.tag().id
                    this.SetTag(targetTag, value.target);
                    this.SetTag(privacyTag, value.privacy.ToString());
                    this.SetTag(slideTag, value.slide);
                    this.SetTag(timestampTag, value.timestamp);
                }
            }

            private void SetWithTextStanza(TargettedTextBox targettedTextBox)
            {
                var textSpec = targettedTextBox.boxSpecification;
                this.height = textSpec.height;
                this.width = textSpec.width;
                this.caret = textSpec.caret;
                this.x = textSpec.x;
                this.y = textSpec.y;
                this.text = textSpec.text;
                this.tag = textSpec.tag;
                this.style = textSpec.style;
                this.family = textSpec.family;
                this.weight = textSpec.weight;
                this.size = textSpec.size;
                this.decoration = textSpec.decoration;
                this.color = textSpec.color;
            }

            private void SetWithTextControl(TargettedTextBox targettedTextBox)
            {
                var textCtrl = targettedTextBox.boxProperty;

                var width = textCtrl.Width;
                this.width = Double.IsNaN(width) ? textCtrl.ActualWidth : width;
                var height = textCtrl.Height;
                this.height = Double.IsNaN(height) ? textCtrl.ActualHeight : height;
                //this.height = textCtrl.Height;
                //this.width = textCtrl.Width;
                this.caret = textCtrl.CaretIndex;
                this.x = InkCanvas.GetLeft(textCtrl);
                this.y = InkCanvas.GetTop(textCtrl);
                this.text = textCtrl.Text;
                this.tag = textCtrl.Tag.ToString();
                this.style = textCtrl.FontStyle;
                this.family = textCtrl.FontFamily;
                this.weight = textCtrl.FontWeight;
                this.size = textCtrl.FontSize;
                this.decoration = textCtrl.TextDecorations;
                this.color = textCtrl.Foreground;
            }

            public static readonly string widthTag = "width";
            public static readonly string heightTag = "height";
            public static readonly string caretTag = "caret";
            public static readonly string textTag = "text";
            public static readonly string styleTag = "style";
            public static readonly string familyTag = "family";
            public static readonly string weightTag = "weight";
            public static readonly string sizeTag = "size";
            public static readonly string decorationTag = "decoration";
            public static readonly string colorTag = "color";

            public Brush color
            {
                get { return (Brush)(new BrushConverter().ConvertFromString(GetTag(colorTag))); }
                set { SetTag(colorTag, value.ToString()); }
            }
            public int caret
            {
                get { return Int32.Parse(GetTag(caretTag)); }
                set { SetTag(caretTag, value.ToString()); }
            }
            public double width
            {
                get { return Double.Parse(GetTag(widthTag)); }
                set { SetTag(widthTag, value.ToString()); }
            }
            public double height
            {
                get { return Double.Parse(GetTag(heightTag)); }
                set { SetTag(heightTag, value.ToString()); }
            }
            public double x
            {
                get { return Double.Parse(GetTag(xTag)); }
                set { SetTag(xTag, value.ToString()); }
            }
            public double y
            {
                get { return Double.Parse(GetTag(yTag)); }
                set { SetTag(yTag, value.ToString()); }
            }
            public string text
            {
                get { return GetTag(textTag); }
                set { SetTag(textTag, value); }
            }
            public string tag
            {
                get { return GetTag(tagTag); }
                set { SetTag(tagTag, value); }
            }
            public FontStyle style
            {
                get { return (FontStyle)new FontStyleConverter().ConvertFromString(GetTag(styleTag)); }
                set { SetTag(styleTag, value.ToString()); }
            }
            public FontFamily family
            {
                get { return new FontFamily(GetTag(familyTag)); }
                set { SetTag(familyTag, value.ToString()); }
            }
            public FontWeight weight
            {
                get { return (FontWeight)new FontWeightConverter().ConvertFromString(GetTag(weightTag)); }
                set { SetTag(weightTag, value.ToString()); }
            }
            public double size
            {
                get { return Double.Parse(GetTag(sizeTag)); }
                set { SetTag(sizeTag, value.ToString()); }
            }
            public TextDecorationCollection decoration
            {
                get
                {
                    return new TextDecorationCollection(
                      GetTag(decorationTag).Split(' ').Select(d =>
                      {
                          switch (d)
                          {
                              case "Underline":
                                  return TextDecorations.Underline.First();
                              case "Strikethrough":
                                  return TextDecorations.Strikethrough.First();
                              default:
                                  return null;
                          }
                      }).Where(d => d != null));
                }
                set
                {
                    SetTag(decorationTag, value.Count() > 0 ? value[0].Location.ToString() : "None");
                }
            }
        }
        public class FileResource : Element
        {

            public static string TAG = "fileResource";
            public static string AUTHOR = "author";
            public static string URL = "url";
            public static string TIME = "time";
            public static string SIZE = "size";
            public static string NAME = "name";
            static FileResource()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(FileResource));
            }
            public FileResource()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public FileResource(TargettedFile file)
                : this()
            {
                fileResource = file;
            }
            public FileResource injectDependencies(MeTLServerAddress server)
            {
                this.server = server;
                return this;
            }
            private MeTLServerAddress server;
            public TargettedFile fileResource
            {
                get
                {
                    var fileuploadTime = HasTag(TIME) ? GetTag(TIME) : DateTimeFactory.Now().ToString();
                    var filesize = HasTag(SIZE) ? long.Parse(GetTag(SIZE)) : 0;
                    var filename = HasTag(NAME) ? GetTag(NAME) : Path.GetFileNameWithoutExtension(GetTag(URL));
                    var slide = HasTag(slideTag) ? GetTag(slideTag) : "0";
                    var target = HasTag(targetTag) ? GetTag(targetTag) : "";
                    var privacy = HasTag(privacyTag) ? (Privacy)GetTagEnum(privacyTag, typeof(Privacy)) : Privacy.Public;
                    var timestamp = HasTag(timestampTag) ? GetTag(timestampTag) : "-1L";
                    var url = "https://" + server.host + ":1188" + INodeFix.StemBeneath("/Resource/", INodeFix.StripServer(GetTag(URL)));
                    var identity = HasTag(identityTag) ? GetTag(identityTag) : url + fileuploadTime + filename;
                    var file = new TargettedFile(Int32.Parse(slide), GetTag(authorTag), target, privacy, identity, long.Parse(timestamp), url, fileuploadTime, filesize, filename);
                    return file;
                }
                set
                {
                    SetTag(AUTHOR, value.author);
                    SetTag(URL, INodeFix.StripServer(value.url));
                    SetTag(TIME, value.uploadTime);
                    SetTag(SIZE, value.size);
                    SetTag(NAME, value.name);
                    SetTag(slideTag, value.conversationJid);
                }
            }
        }
        public class LocalFileInformation
        {
            public LocalFileInformation(int Slide, string Author, string Target, Privacy Privacy, long Timestamp, string File, string Name, bool Overwrite, long Size, string UploadTime, string Identity)
            {
                slide = Slide;
                author = Author;
                target = Target;
                privacy = Privacy;
                file = File;
                name = Name;
                overwrite = Overwrite;
                size = Size; 
                timestamp = Timestamp;
                identity = Identity;
                uploadTime = UploadTime;
            }
            public string author;
            public string file;
            public bool overwrite;
            public string name;
            public Privacy privacy;
            public long size;
            public int slide;
            public string target;
            public string uploadTime;
            public string identity;
            public long timestamp;

            public LocalFileInformation()
            {
            }
        }
        public class LocalImageInformation
        {
            public LocalImageInformation(int Slide, string Author, string Target, Privacy Privacy, System.Windows.Controls.Image Image, string File, bool Overwrite)
            {
                slide = Slide;
                author = Author;
                target = Target;
                privacy = Privacy;
                image = Image;
                file = File;
                overwrite = Overwrite;
            }
            public string author;
            public System.Windows.Controls.Image image;
            public string file;
            public bool overwrite;
            public Privacy privacy;
            public int slide;
            public string target;

            public LocalImageInformation()
            {
            }
        }

        public class BlackListedUser
        {
            public string UserName { get; private set; }
            public string Color { get; private set; }

            public BlackListedUser(string username, Color color)
            {
                UserName = username;
                Color = Ink.colorToString(color);
            }
        }

        public class ElementIdentity
        {
            public string Identity { get; private set; }

            public ElementIdentity(string identity)
            {
                Identity = identity;
            }

            #region Equality methods and overrides
            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                // if parameter can't be cast. note cast to object to avoid infinite recursion 
                var elemId = obj as ElementIdentity;
                if ((object)elemId == null)
                {
                    return false;
                }

                // ignoring case for the string compare
                return (string.Compare(elemId.Identity, Identity, true) == 0);
            }

            public bool Equals(ElementIdentity elemId)
            {
                // if parameter is null return false
                if ((object)elemId == null)
                {
                    return false;
                }

                // ignoring case for the string compare
                return (string.Compare(elemId.Identity, Identity, true) == 0);
            }

            public override int GetHashCode()
            {
                return Identity == null ? 0 : Identity.ToLower().GetHashCode();
            }

            public static bool operator ==(ElementIdentity elemA, ElementIdentity elemB)
            {
                if (object.ReferenceEquals(elemA, elemB))
                {
                    return true;
                }

                if (((object)elemA == null) || ((object)elemB == null))
                {
                    return false;
                }

                return (string.Compare(elemA.Identity, elemB.Identity, true) == 0);
            }

            public static bool operator !=(ElementIdentity elemA, ElementIdentity elemB)
            {
                return !(elemA == elemB);
            }

            public static implicit operator string(ElementIdentity elemId)
            {
                return elemId.Identity;
            }
            #endregion
        }

        public class ElementIdentityStanza : Element
        {
            public ElementIdentityStanza(string tagName)
            {
                this.Namespace = METL_NS;
                this.TagName = tagName;
            }

            public ElementIdentityStanza(ElementIdentity elementId, string tagName)
                : this(tagName)
            {
                this.identity = elementId;
            }

            public ElementIdentity identity
            {
                get
                {
                    return new ElementIdentity(Value);
                }
                set
                {
                    Value = value.Identity;
                }
            }
        }

        public class TextBoxIdentityStanza : ElementIdentityStanza
        {
            static readonly string TAG = "textId";

            static TextBoxIdentityStanza()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TextBoxIdentityStanza.TAG, METL_NS, typeof(TextBoxIdentityStanza));
            }

            public TextBoxIdentityStanza()
                : base(TAG)
            {
            }

            public TextBoxIdentityStanza(ElementIdentity elementId)
                : base(elementId, TAG)
            {
            }
        }

        public class InkIdentityStanza : ElementIdentityStanza
        {
            static readonly string TAG = "inkId";

            static InkIdentityStanza()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(InkIdentityStanza.TAG, METL_NS, typeof(InkIdentityStanza));
            }

            public InkIdentityStanza()
                : base(TAG)
            {
            }

            public InkIdentityStanza(ElementIdentity elementId)
                : base(elementId, TAG)
            {
            }
        }

        public class ImageIdentityStanza : ElementIdentityStanza
        {
            static readonly string TAG = "imageId";

            static ImageIdentityStanza()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(ImageIdentityStanza.TAG, METL_NS, typeof(ImageIdentityStanza));
            }

            public ImageIdentityStanza()
                : base(TAG)
            {
            }

            public ImageIdentityStanza(ElementIdentity elementId)
                : base(elementId, TAG)
            {
            }
        }

        public class LocalSubmissionInformation
        {
            public LocalSubmissionInformation(int Slide, string Author, string Target, Privacy Privacy, long Timestamp, string File, string CurrentConversationName, Dictionary<string, Color> Blacklisted, string Identity)
            {
                slide = Slide;
                author = Author;
                target = Target;
                privacy = Privacy;
                file = File;
                identity = Identity;
                timestamp = Timestamp;
                currentConversationName = CurrentConversationName;

                blacklisted = new List<BlackListedUser>();
                foreach (var user in Blacklisted)
                {
                    blacklisted.Add(new BlackListedUser(user.Key, user.Value));
                }
            }
            public string author;
            public string file;
            public Privacy privacy;
            public int slide;
            public string identity;
            public string target;
            public string currentConversationName;
            public List<BlackListedUser> blacklisted;
            public long timestamp;
        }
        public class LocalVideoInformation
        {
            public LocalVideoInformation(int Slide, string Author, string Target, string Privacy, MeTLLib.DataTypes.Video Video, string File, bool Overwrite)
            {
                slide = Slide;
                author = Author;
                target = Target;
                privacy = Privacy;
                video = Video;
                file = File;
                overwrite = Overwrite;
            }
            public string author;
            public MeTLLib.DataTypes.Video video;
            public string file;
            public bool overwrite;
            public string privacy;
            public int slide;
            public string target;

            public LocalVideoInformation()
            {
            }
        }
        public class ScreenshotSubmission : Element
        {
            static ScreenshotSubmission()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(ScreenshotSubmission));
            }

            public static string TAG = "screenshotSubmission";
            public static string AUTHOR = "author";
            public static string URL = "url";
            public static string TITLE = "title";
            public static string SLIDE = "slide";
            public static string TIME = "time";
            public static string BLACKLIST = "blackList";

            public ScreenshotSubmission()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public ScreenshotSubmission(TargettedSubmission submission)
                : this()
            {
                this.parameters = submission;
            }
            public ScreenshotSubmission injectDependencies(MeTLServerAddress server)
            {
                this.server = server;
                return this;
            }
            private MeTLServerAddress server;

            public TargettedSubmission parameters
            {
                get
                {
                    var url = "https://" + server.host + ":1188" + INodeFix.StemBeneath("/Resource/", INodeFix.StripServer(GetTag(URL)));
                    var timestamp = HasTag(timestampTag) ? GetTag(timestampTag) : "-1L";
                    var submission = new TargettedSubmission(int.Parse(GetTag(SLIDE)), GetTag(AUTHOR), GetTag(targetTag), (Privacy)GetTagEnum(privacyTag, typeof(Privacy)), long.Parse(timestamp), GetTag(identityTag), url, GetTag(TITLE), long.Parse(GetTag(TIME)), new List<MeTLStanzas.BlackListedUser>());

                    if (HasTag(BLACKLIST))
                    {
                        foreach (var node in ChildNodes)
                        {
                            if (node.GetType() == typeof(BlackList))
                                submission.blacklisted.Add(((BlackList)node).parameters);
                        }
                    }

                    return submission;
                }
                set
                {
                    var strippedUrl = INodeFix.StripServer(value.url);
                    SetTag(AUTHOR, value.author);
                    SetTag(URL, strippedUrl);
                    SetTag(TITLE, value.title);
                    SetTag(targetTag, value.target);
                    SetTag(timestampTag, value.timestamp);
                    SetTag(SLIDE, value.slide.ToString());
                    SetTag(TIME, value.time.ToString());
                    foreach (var banned in value.blacklisted)
                    {
                        var bannedElement = new BlackList(banned);
                        AddChild(bannedElement);
                    }
                }
            }
        }
        public class BlackList : Element
        {
            static BlackList()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(BlackList));
            }
            public static string TAG = "blackList";
            public static readonly string USERNAME = "username";
            public static readonly string HIGHLIGHT = "highlight";

            public BlackList()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public BlackList(BlackListedUser parameters)
                : this()
            {
                this.parameters = parameters;
            }
            public BlackListedUser parameters
            {
                get
                {
                    Color highlight;
                    if (HasTag(HIGHLIGHT))
                    {
                        highlight = Ink.stringToColor(GetTag(HIGHLIGHT));
                    }
                    else
                    {
                        highlight = Colors.Black;
                    }

                    return new BlackListedUser(GetTag(USERNAME), highlight);
                }
                set
                {
                    SetTag(USERNAME, value.UserName);
                    SetTag(HIGHLIGHT, value.Color);
                }
            }
        }
        public class QuizOption : Element
        {
            static QuizOption()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(QuizOption));
            }
            public static string TAG = "quizOption";
            public static readonly string NAME = "name";
            public static readonly string TEXT = "text";
            public static readonly string CORRECT = "correct";
            public static readonly string COLOR = "color";

            public QuizOption()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public QuizOption(Option parameters)
                : this()
            {
                this.parameters = parameters;
            }
            public Option parameters
            {
                get
                {
                    return new Option(GetTag(NAME), GetTag(TEXT), GetTag(CORRECT).ToString().ToLower() == "true", Ink.stringToColor(GetTag(COLOR)));
                }
                set
                {
                    SetTag(NAME, value.name);
                    SetTag(TEXT, value.optionText);
                    SetTag(CORRECT, value.correct.ToString());
                    SetTag(COLOR, Ink.colorToString(value.color));
                }
            }
        }
        public class QuizResponse : Element
        {
            static QuizResponse()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(QuizResponse));
            }

            public static string TAG = "quizResponse";
            public static string ANSWER = "answer";
            public static string ANSWERER = "answerer";
            public static string ANSWERDATE = "answerDate";
            public static string ID = "id";
            public QuizResponse()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public QuizResponse(QuizAnswer parameters)
                : this()
            {
                this.parameters = parameters;
            }
            private long loadDate()
            {
                if (HasTag(ANSWERDATE))
                    return long.Parse(GetTag(ANSWERDATE));
                return 0;
            }
            public QuizAnswer parameters
            {
                get
                {
                    return new QuizAnswer(long.Parse(GetTag(ID)), GetTag(ANSWERER), GetTag(ANSWER), loadDate());
                }
                set
                {
                    SetTag(ANSWERDATE, value.answerTime.ToString());
                    SetTag(ANSWER, value.answer);
                    SetTag(ANSWERER, value.answerer);
                    SetTag(ID, value.id.ToString());
                }
            }
        }
        public class Quiz : Element
        {
            static Quiz()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(Quiz));
            }
            public static string TAG = "quiz";
            public static string CREATED = "created";
            public static readonly string TITLE = "title";
            public static readonly string QUESTION = "question";
            public static readonly string AUTHOR = "author";
            public static readonly string ID = "id";
            public static readonly string URL = "url";
            public static readonly string IS_DELETED = "isdeleted";

            public Quiz()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Quiz(QuizQuestion parameters)
                : this()
            {
                this.parameters = parameters;
            }
            public Quiz injectDependencies(MeTLServerAddress server)
            {
                this.server = server;
                return this;
            }
            private MeTLServerAddress server;

            private string ProcessedUrl()
            {
                string url = HasTag(URL) ? GetTag(URL) : "none";
                if (url.ToLower() != "none")
                {
                    url = "https://" + server.host + ":1188" + INodeFix.StemBeneath("/Resource/", INodeFix.StripServer(url));
                }
                return url;
            }

            public QuizQuestion parameters
            {
                get
                {
                    long created;
                    if (HasTag(CREATED))
                        created = long.Parse(GetTag(CREATED));
                    else
                    {
                        created = DateTimeFactory.Now().Ticks;
                    }
                    var quiz = new QuizQuestion(long.Parse(GetTag(ID)), created, GetTag(TITLE), GetTag(AUTHOR), GetTag(QUESTION), new List<Option>());
                    quiz.Url = ProcessedUrl();
                    if (HasTag(IS_DELETED))
                        quiz.IsDeleted = bool.Parse(GetTag(IS_DELETED));

                    foreach (var node in ChildNodes)
                    {
                        if (node.GetType() == typeof(QuizOption))
                            quiz.Options.Add(((QuizOption)node).parameters);
                    }
                    return quiz;
                }
                set
                {

                    SetTag(CREATED, value.Created.ToString());
                    SetTag(TITLE, value.Title);
                    SetTag(QUESTION, value.Question);
                    SetTag(AUTHOR, value.Author);
                    SetTag(ID, value.Id.ToString());
                    var url = INodeFix.StripServer(value.Url);
                    SetTag(URL, url);
                    SetTag(IS_DELETED, value.IsDeleted.ToString());
                    foreach (var option in value.Options)
                    {
                        var optionElement = new QuizOption(option);
                        AddChild(optionElement);
                    }
                }
            }
        }
        public class LiveWindow : Element
        {
            static LiveWindow()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(LiveWindow));
            }
            public static string TAG = "liveWindow";
            public static string widthTag = "width";
            public static string heightTag = "height";
            public static string destXTag = "destX";
            public static string destYTag = "destY";
            public static string snapshotTag = "snapshot";
            public LiveWindow()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public LiveWindow(LiveWindowSetup window)
                : this()
            {
                this.parameters = window;
                SetTag(privacyTag, "private");
            }
            public LiveWindowSetup parameters
            {
                get
                {
                    return new LiveWindowSetup(
                        Int32.Parse(GetTag(slideTag)),
                        GetTag(authorTag),
                        new Rectangle { Width = Double.Parse(GetTag(widthTag)), Height = Double.Parse(GetTag(heightTag)) },
                        new Point(Double.Parse(GetTag(xTag)), Double.Parse(GetTag(yTag))),
                        new Point(Double.Parse(GetTag(destXTag)), Double.Parse(GetTag(destYTag))),
                        GetTag(snapshotTag));
                }
                set
                {
                    SetTag(xTag, value.origin.X);
                    SetTag(yTag, value.origin.Y);
                    SetTag(widthTag, value.frame.Width);
                    SetTag(heightTag, value.frame.Height);
                    SetTag(destXTag, value.target.X);
                    SetTag(destYTag, value.target.Y);
                    SetTag(snapshotTag, value.snapshotAtTimeOfCreation);
                    SetTag(authorTag, value.author);
                    SetTag(slideTag, value.slide.ToString());
                }
            }
        }
        public class Image : Element
        {
            private IWebClient downloader;
            private MeTLServerAddress server;
            private HttpResourceProvider provider;
            static Image()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(Image));
            }
            public static string TAG = "image";
            public Image()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Image(TargettedImage image)
                : this()
            {
                this.Img = image;
            }
            public Image injectDependencies(MeTLServerAddress server, IWebClient downloader, HttpResourceProvider provider)
            {
                this.provider = provider;
                this.server = server;
                this.downloader = downloader;
                return this;
            }

            protected static ImageSource BackupSource
            {
                get
                {
                    try
                    {
                        return
                            new PngBitmapDecoder(new Uri("Resources\\empty.png", UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None).Frames[0];
                    }
                    catch (Exception)
                    {
                        return new BitmapImage();
                    }
                }
            }

            public Func<System.Windows.Controls.Image> curryEvaluation(MeTLServerAddress server)
            {
                return () => forceEvaluation();
            }
            public System.Windows.Controls.Image forceEvaluationForPrinting()
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                {
                    Tag = "FOR_PRINTING_ONLY::::" + this.tag,
                    Height = this.height,
                    Width = this.width,
                    Source = this.asynchronouslyLoadImageData()
                };
                InkCanvas.SetLeft(image, this.x);
                InkCanvas.SetTop(image, this.y);
                return image;
            }
            public System.Windows.Controls.Image forceEvaluation()
            {
                var sourceString = string.Format("https://{0}:1188{1}", server.host, INodeFix.StemBeneath("/Resource/", GetTag(sourceTag)));
                var dynamicTag = this.tag.StartsWith("NOT_LOADED") ? this.tag : "NOT_LOADED::::" + sourceString + "::::" + this.tag;
                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                    {
                        Tag = dynamicTag,
                        Height = this.height,
                        Width = this.width,
                        Source = BackupSource
                    };
                RoutedEventHandler handler = null;
                handler = delegate
                {
                    image.Loaded -= handler;
                    ThreadPool.UnsafeQueueUserWorkItem(delegate
                    {
                        if (image == null) return;//This might have been GCed if they moved conversations
                        var newSource = asynchronouslyLoadImageData();
                        image.Dispatcher.Invoke((Action)delegate
                        {
                            try
                            {
                                var oldTag = image.Tag;
                                if (oldTag.ToString().StartsWith("NOT_LOADED"))
                                    image.Tag = oldTag.ToString().Split(new[] { "::::" }, StringSplitOptions.RemoveEmptyEntries)[2];
                                image.Source = newSource;
                                // Having next two lines causes bug #1480 but partially fixes #1435
                                //image.Height = newSource.Height;
                                //image.Width = newSource.Width;
                            }
                            catch (InvalidOperationException)
                            {
                                Trace.TraceInformation("CRASH: (Fixed) MeTLStanzaDefinitions::Image::forceEvaluation - couldn't find a dispatcher");
                            }
                        });
                    }, null);
                };
                image.Loaded += handler;
                image.tag(new ImageTag(Img.author,Img.privacy,Img.identity,false,Img.timestamp));
                InkCanvas.SetLeft(image, this.x);
                InkCanvas.SetTop(image, this.y);
                return image;
            }
            public ImageSource asynchronouslyLoadImageData()
            {
                var image = new BitmapImage();
                try
                {
                    var safetiedSourceTag = safetySourceTag(GetTag(sourceTag));
                    var stemmedRelativePath = INodeFix.StemBeneath("/Resource/", safetiedSourceTag);
                    var path = string.Format("https://{0}:1188{1}", server.host, stemmedRelativePath);
                    var bytes = provider.secureGetData(new Uri(path, UriKind.RelativeOrAbsolute));
                    if (bytes.Length == 0) return null;
                    var stream = new MemoryStream(bytes);
                    image.BeginInit();
                    image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();//Going to be handed back to the dispatcher
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("CRASH: MeTLLib::MeTLStanzaDefinitions:Image:source Image instantiation failed at: {0} {1} with {2}", DateTime.Now, DateTime.Now.Millisecond, e.Message);
                    //Who knows what sort of hell is lurking in our history
                }
                return image;
            }
            public TargettedImage Img
            {
                get
                {
                    var timestamp = HasTag(timestampTag) ? GetTag(timestampTag) : "-1L";
                    var targettedImage = new TargettedImage(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), (Privacy)GetTagEnum(privacyTag, typeof(Privacy)), this, GetTag(identityTag), long.Parse(timestamp));
                    return targettedImage;
                }
                set
                {
                    // fall back to using the spec if the image property is not available ie when the targettedimage is pulled out of the cached history. 
                    if (value.imageProperty == null)
                        SetWithImageStanza(value);
                    else
                        SetWithImageControl(value);

                    SetTag(authorTag, value.author);
                    SetTag(targetTag, value.target);
                    SetTag(privacyTag, value.privacy.ToString());
                    SetTag(slideTag, value.slide);
                    SetTag(identityTag, value.identity);
                    SetTag(timestampTag, value.timestamp);
                }
            }

            private void SetWithImageControl(TargettedImage targettedImage)
            {
                var imageCtrl = targettedImage.imageProperty;

                string newTag = imageCtrl.Tag.ToString();
                var absolutePath = imageCtrl.Source.ToString();
                if (newTag.ToString().StartsWith("NOT_LOADED"))
                {
                    var parts = newTag.ToString().Split(new[] { "::::" }, StringSplitOptions.RemoveEmptyEntries);
                    absolutePath = parts[1];
                    newTag = parts[2];
                }
                SetTag(tagTag, newTag);
                var uri = new Uri(absolutePath, UriKind.RelativeOrAbsolute);
                string relativePath;
                if (uri.IsAbsoluteUri)
                    relativePath = uri.LocalPath;
                else
                    relativePath = uri.ToString();
                SetTag(tagTag, imageCtrl.Tag.ToString());
                SetTag(sourceTag, relativePath);
                var width = imageCtrl.Width;
                SetTag(widthTag, Double.IsNaN(width) ? imageCtrl.ActualWidth.ToString() : width.ToString());
                var height = imageCtrl.Height;
                SetTag(heightTag, Double.IsNaN(height) ? imageCtrl.ActualHeight.ToString() : height.ToString());
                var currentX = (InkCanvas.GetLeft(imageCtrl) + (Double.IsNaN(imageCtrl.Margin.Left) ? 0 : imageCtrl.Margin.Left)).ToString();
                var currentY = (InkCanvas.GetTop(imageCtrl) + (Double.IsNaN(imageCtrl.Margin.Top) ? 0 : imageCtrl.Margin.Top)).ToString();
                SetTag(xTag, currentX);
                SetTag(yTag, currentY);
            }
            private void SetWithImageStanza(TargettedImage targettedImage)
            {
                var imageSpec = targettedImage.imageSpecification;

                SetTag(tagTag, imageSpec.GetTag(tagTag));
                SetTag(sourceTag, imageSpec.GetTag(sourceTag));
                SetTag(widthTag, imageSpec.GetTag(widthTag));
                SetTag(heightTag, imageSpec.GetTag(heightTag));
                SetTag(xTag, imageSpec.GetTag(xTag));
                SetTag(yTag, imageSpec.GetTag(yTag));
            }

            protected static readonly string sourceTag = "source";
            protected static readonly string heightTag = "height";
            protected static readonly string widthTag = "width";
            public string tag
            {
                get { return GetTag(tagTag); }
                set { SetTag(tagTag, value); }
            }
            protected string safetySourceTag(String tag)
            {
                if (tag.StartsWith("NOT_LOADED"))
                    tag = tag.Split(new[] { "::::" }, StringSplitOptions.RemoveEmptyEntries)[1];
                return tag;
            }

            public double x
            {
                get { return Double.Parse(GetTag(xTag)); }
                set { SetTag(xTag, value.ToString()); }
            }
            public double y
            {
                get { return Double.Parse(GetTag(yTag)); }
                set { SetTag(yTag, value.ToString()); }
            }
            public double width
            {
                get { return Double.Parse(GetTag(widthTag)); }
                set { SetTag(widthTag, value.ToString()); }
            }
            public double height
            {
                get { return Double.Parse(GetTag(heightTag)); }
                set { SetTag(heightTag, value.ToString()); }
            }
        }
        public class DirtyElement : Element
        {
            public DirtyElement()
            {
                this.Namespace = METL_NS;
            }
            public DirtyElement(TargettedDirtyElement element)
                : this()
            {
                this.element = element;
            }
            public TargettedDirtyElement element
            {
                get
                {
                    var timestamp = HasTag(timestampTag) ? GetTag(timestampTag) : "-1L";
                    return new TargettedDirtyElement(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), (Privacy)GetTagEnum(privacyTag, typeof(Privacy)), GetTag(identityTag), long.Parse(timestamp));
                }
                set
                {
                    SetTag(authorTag, value.author);
                    SetTag(slideTag, value.slide);
                    SetTag(targetTag, value.target);
                    SetTag(privacyTag, value.privacy.ToString());
                    SetTag(identityTag, value.identity);
                    SetTag(timestampTag, value.timestamp);
                }
            }
        }
        public class DirtyInk : DirtyElement
        {
            static DirtyInk() { agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(DirtyInk)); }
            static readonly string TAG = "dirtyInk";
            public DirtyInk() { }
            public DirtyInk(TargettedDirtyElement element)
                : base(element)
            {
                this.TagName = TAG;
            }
        }
        public class DirtyText : DirtyElement
        {
            static readonly string TAG = "dirtyText";
            static DirtyText() { agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(DirtyText)); }
            public DirtyText() { }
            public DirtyText(TargettedDirtyElement element)
                : base(element)
            {
                this.TagName = TAG;
            }
        }
        public class DirtyImage : DirtyElement
        {
            static readonly string TAG;
            static DirtyImage()
            {
                TAG = "dirtyImage";
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(DirtyImage));
            }
            public DirtyImage() { }
            public DirtyImage(TargettedDirtyElement element)
                : base(element)
            {
                this.TagName = TAG;
            }
        }
        public class DirtyAutoshape : DirtyElement
        {
            static readonly string TAG = "dirtyAutoshape";
            static DirtyAutoshape() { agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(DirtyAutoshape)); }
            public DirtyAutoshape() { }
            public DirtyAutoshape(TargettedDirtyElement element)
                : base(element)
            {
                this.TagName = TAG;
            }
        }
        public class DirtyVideo : DirtyElement
        {
            static readonly string TAG = "dirtyVideo";
            static DirtyVideo() { agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(DirtyVideo)); }
            public DirtyVideo() { }
            public DirtyVideo(TargettedDirtyElement element)
                : base(element)
            {
                this.TagName = TAG;
            }
        }
        public class DirtyLiveWindow : DirtyElement
        {
            static DirtyLiveWindow() { agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(DirtyLiveWindow)); }
            static readonly string TAG = "dirtyLiveWindow";
            public DirtyLiveWindow() { }
            public DirtyLiveWindow(TargettedDirtyElement element)
                : base(element)
            {
                this.TagName = TAG;
            }
        }
    }
    public static class TargettedElementExtensions
    {
        public static T timestamp<T>(this TargettedElement elem, long timestamp) where T : TargettedElement
        {
            elem.timestamp = timestamp;
            return (T)elem;
        }
    }
}
