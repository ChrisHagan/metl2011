﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
using MeTLLib.DataTypes;
using MeTLLib.Providers;
using Microsoft.Practices.Composite.Presentation.Commands;
using Ninject;
using System.Threading;

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
            new MeTLStanzas.Video();
            new MeTLStanzas.Bubble();
            new MeTLStanzas.TextBox();
            new MeTLStanzas.DirtyInk();
            new MeTLStanzas.DirtyText();
            new MeTLStanzas.AutoShape();
            new MeTLStanzas.DirtyImage();
            new MeTLStanzas.LiveWindow();
            new MeTLStanzas.QuizOption();
            new MeTLStanzas.ScreenshotSubmission();
            new MeTLStanzas.FileResource();
            new MeTLStanzas.QuizResponse();
            new MeTLStanzas.DirtyElement();
            new MeTLStanzas.DirtyAutoshape();
            new MeTLStanzas.DirtyLiveWindow();
        }
    }
    public class WormMove
    {
        public WormMove(string Conversation, string Direction)
        {
            conversation = Conversation;
            direction = Direction;
        }
        public string conversation;
        public string direction;
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is WormMove)) return false;
            var foreignWormMove = (WormMove)obj;
            return ((foreignWormMove.conversation == conversation) && (foreignWormMove.direction == direction));
        }
    }

    public class TargettedElement
    {
        public TargettedElement(int Slide, string Author, string Target, string Privacy)
        {
            slide = Slide;
            author = Author;
            target = Target;
            privacy = Privacy;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedElement)) return false;
            var foreignTE = (TargettedElement)obj;
            return (foreignTE.author == author && foreignTE.privacy == privacy && foreignTE.slide == slide && foreignTE.target == target);
        }
        public string author { get; set; }
        public string target;
        public string privacy;
        public int slide;
    }
    public class TargettedAutoShape : TargettedElement
    {
        public TargettedAutoShape(int Slide, string Author, string Target, string Privacy, AutoShape Autoshape)
            : base(Slide, Author, Target, Privacy)
        {
            autoshape = Autoshape;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedAutoShape)) return false;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj) && ((TargettedAutoShape)obj).autoshape.ValueEquals(autoshape));

        }
        public MeTLLib.DataTypes.AutoShape autoshape;
    }
    public class TargettedSubmission : TargettedElement
    {
        public TargettedSubmission(int Slide, string Author, string Target, string Privacy, string Url, long Time)
            : base(Slide, Author, Target, Privacy)
        {
            url = Url;
            time = Time;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedSubmission)) return false;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj) && ((TargettedSubmission)obj).url == url && ((TargettedSubmission)obj).time == time);

        }
        public string url { get; set; }
        public long time { get; set; }

    }
    public class TargettedStroke : TargettedElement
    {
        public TargettedStroke(int Slide, string Author, string Target, string Privacy, Stroke Stroke)
            : base(Slide, Author, Target, Privacy)
        {
            stroke = Stroke;
        }
        public TargettedStroke(int Slide, string Author, string Target, string Privacy, Stroke Stroke, double StartingChecksum)
            : this(Slide, Author, Target, Privacy, Stroke)
        {
            startingChecksum = StartingChecksum;
        }
        public TargettedStroke(int Slide, string Author, string Target, string Privacy, Stroke Stroke, double StartingChecksum, string strokeStartingColor)
            : this(Slide, Author, Target, Privacy, Stroke, StartingChecksum)
        {
            startingColor = strokeStartingColor;
        }

        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedStroke)) return false;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj) && ((TargettedStroke)obj).stroke.Equals(stroke) && ((TargettedStroke)obj).startingChecksum == startingChecksum);
        }
        public Stroke stroke;
        public double startingChecksum;
        public string startingColor;
    }
    public class TargettedBubbleContext : TargettedElement
    {
        public TargettedBubbleContext(int Slide, string Author, string Target, string Privacy, List<SelectedIdentity> Context, int ThoughtSlide)
            : base(Slide, Author, Target, Privacy)
        {
            context = Context;
            thoughtSlide = ThoughtSlide;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedBubbleContext)) return false;
            var foreignContext = (TargettedBubbleContext)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj) && foreignContext.thoughtSlide == thoughtSlide && foreignContext.context.All(s => s.ValueEquals(context.ElementAt(foreignContext.context.IndexOf(s)))));
        }
        public List<SelectedIdentity> context;
        public int thoughtSlide;
    }
    public class TargettedFile : TargettedElement
    {
        public TargettedFile(int Slide, string Author, string Target, string Privacy, string Url, string UploadTime, long Size, string Name)
            : base(Slide, Author, Target, Privacy)
        {
            url = Url;
            uploadTime = UploadTime;
            size = Size;
            name = Name;
        }
        public bool ValueEquals(object obj)
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
        public string uploadTime { get; set; }
        public long size { get; set; }
        public string name { get; set; }
    }
    public class TargettedImage : TargettedElement
    {
        public TargettedImage(int Slide, string Author, string Target, string Privacy, Image Image)
            : base(Slide, Author, Target, Privacy)
        {
            image = Image;
        }
        public TargettedImage(int Slide, string Author, string Target, string Privacy, MeTLStanzas.Image ImageSpecification, string Identity)
            : base(Slide, Author, Target, Privacy)
        {
            imageSpecification = ImageSpecification;
            id = Identity;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedImage)) return false;
            var foreign = (TargettedImage)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && foreign.id == id
                && foreign.imageProperty.Equals(imageProperty)
                && foreign.imageSpecification == imageSpecification
                && foreign.image.Equals(image));
        }
        public System.Windows.Controls.Image imageProperty;
        public MeTLStanzas.Image imageSpecification;
        public ResourceCache cache;
        public MeTLServerAddress server;
        public string id;
        public void adoptCache(ResourceCache cache, MeTLServerAddress server)
        {
            if (imageSpecification == null) imageSpecification = new MeTLStanzas.Image(this);
            this.cache = cache;
            this.server = server;
            imageSpecification.adoptCache(cache, server);
        }
        public System.Windows.Controls.Image image
        {
            get
            {
                if (server != null && cache != null) imageSpecification.adoptCache(cache, server);
                if (imageSpecification == null) imageSpecification = new MeTLStanzas.Image(this);
                var reified = imageSpecification.forceEvaluation();
                id = reified.tag().id;
                return reified;
            }
            set
            {
                string internalIdentity;
                try
                {
                    internalIdentity = value.tag().id;
                }
                catch (Exception ex)
                {
                    if (String.IsNullOrEmpty(id))
                        id = string.Format("{0}:{1}", author, DateTimeFactory.Now());
                    value.tag(new ImageTag { author = author, id = id, privacy = privacy });
                    internalIdentity = value.tag().id;
                }
                id = internalIdentity;
                imageProperty = value;
            }
        }
    }
    public class TargettedVideo : TargettedElement
    {
        public TargettedVideo(int Slide, string Author, string Target, string Privacy, Video Video)
            : base(Slide, Author, Target, Privacy)
        {
            video = Video;
        }
        public TargettedVideo(int Slide, string Author, string Target, string Privacy, MeTLStanzas.Video VideoSpecification, string Identity, double VideoX, double VideoY, double VideoWidth, double VideoHeight)
            : base(Slide, Author, Target, Privacy)
        {
            videoSpecification = VideoSpecification;
            id = Identity;
            X = VideoX;
            Y = VideoY;
            Width = VideoWidth;
            Height = VideoHeight;
            if (cache == null && VideoSpecification.resourceCache != null) cache = VideoSpecification.resourceCache;
            if (server == null && VideoSpecification.server != null) server = VideoSpecification.server;
        }
        public ResourceCache cache;
        public MeTLServerAddress server;
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedVideo)) return false;
            var foreign = (TargettedVideo)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && foreign.id == id
                && foreign.Height == Height
                && foreign.Width == Width
                && foreign.X == X
                && foreign.Y == Y
                && foreign.videoSpecification == videoSpecification
                && foreign.videoProperty.Equals(videoProperty)
                && foreign.video.Equals(video));
        }

        public MeTLLib.DataTypes.Video videoProperty;
        public MeTLStanzas.Video videoSpecification;
        public string id;
        public void adoptCache(ResourceCache cache, MeTLServerAddress server)
        {
            if (videoSpecification == null) videoSpecification = new MeTLStanzas.Video(this);
            videoSpecification.adoptCache(cache, server);
        }
        public MeTLLib.DataTypes.Video video
        {
            get
            {
                if (videoSpecification == null) videoSpecification = new MeTLStanzas.Video(this);
                Video reified = null;
                if (server != null && cache != null) videoSpecification.adoptCache(cache, server);
                reified = videoSpecification.forceEvaluation();
                id = reified.tag().id;
                reified.Height = Height;
                reified.Width = Width;
                reified.X = X;
                reified.Y = Y;
                return reified;
            }
            set
            {
                string internalIdentity;
                try
                {
                    internalIdentity = value.tag().id;
                }
                catch (Exception ex)
                {
                    if (String.IsNullOrEmpty(id))
                        id = string.Format("{0}:{1}", author, DateTimeFactory.Now());
                    value.tag(new ImageTag { author = author, id = id, privacy = privacy });
                    internalIdentity = value.tag().id;
                }
                id = internalIdentity;
                X = value.X;
                Y = value.Y;
                Height = value.ActualHeight;
                Width = value.ActualWidth;
                videoProperty = value;
            }
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }
    public class TargettedTextBox : TargettedElement
    {
        public TargettedTextBox(int Slide, string Author, string Target, string Privacy, TextBox TextBox)
            : base(Slide, Author, Target, Privacy)
        {
            box = TextBox;
        }
        public TargettedTextBox(int Slide, string Author, string Target, string Privacy, MeTLStanzas.TextBox BoxSpecification, string Identity)
            : base(Slide, Author, Target, Privacy)
        {
            boxSpecification = BoxSpecification;
            identity = Identity;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedTextBox)) return false;
            var foreign = (TargettedTextBox)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && foreign.identity == identity
                && foreign.boxProperty.Equals(boxProperty)
                && foreign.boxSpecification == boxSpecification
                && foreign.box.Equals(box));
        }
        public TextBox boxProperty;
        public MeTLStanzas.TextBox boxSpecification;
        public string identity;
        public System.Windows.Controls.TextBox box
        {
            get
            {
                if (boxSpecification == null) boxSpecification = new MeTLStanzas.TextBox(this);
                System.Windows.Controls.TextBox reified = boxSpecification.forceEvaluation();
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
                catch (Exception ex)
                {
                    if (String.IsNullOrEmpty(identity))
                        identity = string.Format("{0}:{1}", author, DateTimeFactory.Now());
                    value.tag(new TextTag { author = author, id = identity, privacy = privacy });
                    internalIdentity = value.tag().id;
                }
                identity = internalIdentity;
                boxProperty = value;
            }
        }
    }
    public class TargettedDirtyElement : TargettedElement
    {
        public TargettedDirtyElement(int Slide, string Author, string Target, string Privacy, string Identifier)
            : base(Slide, Author, Target, Privacy)
        {
            identifier = Identifier;
        }
        public string identifier;
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TargettedDirtyElement)) return false;
            var foreign = (TargettedDirtyElement)obj;
            return (((TargettedElement)this).ValueEquals((TargettedElement)obj)
                && foreign.identifier == identifier);
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
        public static readonly string slideTag = "slide";
        public static readonly string answererTag = "answerer";
        public static readonly string identityTag = "identity";

        public class AutoShape : Element
        {
            static AutoShape()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(AutoShape.TAG, METL_NS, typeof(AutoShape));
            }
            public static readonly string TAG = "autoshape";
            public AutoShape()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public AutoShape(TargettedAutoShape autoshape)
                : this()
            {
                this.autoshape = autoshape;
            }

            private string PathDataTag = "pathdata";
            private string StrokeThicknessTag = "strokethickness";
            private string ForegroundTag = "foreground";
            private string BackgroundTag = "background";
            private string ThicknessTag = "thickness";
            private string HeightTag = "height";
            private string WidthTag = "width";
            private string XTag = "x";
            private string YTag = "y";
            public TargettedAutoShape autoshape
            {
                get
                {
                    var targettedAutoShape = new TargettedAutoShape(
                        Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), GetTag(privacyTag),
                        new MeTLLib.DataTypes.AutoShape
                        {
                            Tag = this.tag,
                            PathData = this.pathData,
                            Foreground = this.Foreground,
                            Background = this.Background,
                            StrokeThickness = this.StrokeThickness,
                            Height = this.height,
                            Width = this.width,
                        });
                    InkCanvas.SetLeft(targettedAutoShape.autoshape, this.x);
                    InkCanvas.SetTop(targettedAutoShape.autoshape, this.y);
                    return targettedAutoShape;
                }
                set
                {
                    SetTag(tagTag, value.autoshape.Tag.ToString());
                    if (value.autoshape.PathData is PathGeometry || !(value.autoshape.PathData == null))
                        SetTag(PathDataTag, value.autoshape.PathData.ToString());
                    else throw new InvalidDataException("Trying to create a TargettedAutoShape around invalid pathData: " + value.autoshape.PathData.ToString());
                    SetTag(BackgroundTag, value.autoshape.Background.ToString());
                    SetTag(ForegroundTag, value.autoshape.Foreground.ToString());
                    SetTag(StrokeThicknessTag, value.autoshape.StrokeThickness.ToString());
                    SetTag(widthTag, value.autoshape.Width.ToString());
                    SetTag(heightTag, value.autoshape.Height.ToString());
                    SetTag(xTag, InkCanvas.GetLeft(value.autoshape).ToString());
                    SetTag(yTag, InkCanvas.GetTop(value.autoshape).ToString());
                    SetTag(authorTag, value.author);
                    SetTag(targetTag, value.target);
                    SetTag(privacyTag, value.privacy);
                    SetTag(slideTag, value.slide);
                }
            }
            private static readonly string heightTag = "height";
            private static readonly string widthTag = "width";
            public string tag
            {
                get { return GetTag(tagTag); }
                set { SetTag(tagTag, value); }
            }
            public Brush Foreground
            {
                get
                {
                    if (String.IsNullOrEmpty(ForegroundTag))
                        return Brushes.Black;
                    return (Brush)new BrushConverter().ConvertFromString(GetTag(ForegroundTag));
                }
                set { SetTag(ForegroundTag, value.ToString()); }
            }
            public Brush Background
            {
                get
                {
                    if (String.IsNullOrEmpty(BackgroundTag))
                        return Brushes.Transparent;
                    return (Brush)new BrushConverter().ConvertFromString(GetTag(BackgroundTag));
                }
                set { SetTag(BackgroundTag, value.ToString()); }
            }
            public Double StrokeThickness
            {
                get { return Convert.ToDouble(GetTag(StrokeThicknessTag)); }
                set { SetTag(StrokeThicknessTag, value.ToString()); }
            }
            public PathGeometry pathData
            {
                get
                {
                    var newPathData = new PathGeometry();
                    newPathData.AddGeometry((Geometry)new GeometryConverter().ConvertFromString(GetTag(PathDataTag)));
                    return (PathGeometry)newPathData;
                }
                set { SetTag(PathDataTag, value.ToString()); }
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
        public class Bubble : Element
        {
            static Bubble()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(Bubble.TAG, METL_NS, typeof(Bubble));
            }
            public readonly static string TAG = "bubble";
            public Bubble()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Bubble(TargettedBubbleContext context)
                : this()
            {
                this.context = context;
            }
            private string idsTag = "IDS";
            private string thoughtTag = "THOUGHTSLIDE";
            private string entityIdTag = "ENTITY";
            private string idAttribute = "ID";
            public TargettedBubbleContext context
            {
                get
                {
                    var target = GetTag(targetTag);
                    var context = new TargettedBubbleContext(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), target, GetTag(privacyTag), new List<SelectedIdentity>(), Int32.Parse(GetTag(thoughtTag)));
                    var ids = SelectSingleElement(idsTag).SelectElements(entityIdTag);
                    var identityList = new List<SelectedIdentity>();
                    foreach (var element in ids)
                    {
                        context.context.Add(new SelectedIdentity(((Element)element).GetAttribute(idAttribute), target));
                    }
                    context.context = identityList;
                    return context;
                }
                set
                {
                    this.SetTag(authorTag, value.author);
                    this.SetTag(targetTag, value.target);
                    this.SetTag(privacyTag, value.privacy);
                    this.SetTag(slideTag, value.slide);
                    this.SetTag(thoughtTag, value.thoughtSlide);
                    var ids = new Element(idsTag);
                    SetTag(targetTag, value.context.First().target);
                    foreach (var selectedIdentity in value.context)
                    {
                        var id = new Element(entityIdTag);
                        id.SetAttribute(idAttribute, selectedIdentity.id);
                        ids.AddChild(id);
                    }
                    AddChild(ids);
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
            private string startingColorTag = "startingColor";

            public TargettedStroke Stroke
            {
                get
                {
                    var stroke = new Stroke(stringToPoints(GetTag(pointsTag)), new DrawingAttributes { Color = stringToColor(GetTag(colorTag)) });

                    stroke.DrawingAttributes.IsHighlighter = Boolean.Parse(GetTag(highlighterTag));
                    stroke.DrawingAttributes.Width = Double.Parse(GetTag(thicknessTag));
                    stroke.DrawingAttributes.Height = Double.Parse(GetTag(thicknessTag));
                    if (HasTag(sumTag))
                        stroke.AddPropertyData(stroke.sumId(), Double.Parse(GetTag(sumTag)));
                    if (HasTag(startingSumTag))
                        stroke.AddPropertyData(stroke.startingId(), Double.Parse(GetTag(startingSumTag)));
                    else
                        if (HasTag(sumTag))
                            stroke.AddPropertyData(stroke.startingId(), Double.Parse(GetTag(sumTag)));
                    stroke.tag(new StrokeTag(
                        GetTag(authorTag), GetTag(privacyTag),
                        GetTag(startingSumTag) == null ? stroke.sum().checksum : Double.Parse(GetTag(startingSumTag)),
                        Boolean.Parse(GetTag(highlighterTag))));
                    var targettedStroke = new TargettedStroke(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), GetTag(privacyTag), stroke);
                    return targettedStroke;
                }
                set
                {
                    double startingSum;
                    try
                    {
                        startingSum = value.stroke.startingSum();
                    }
                    catch (Exception ex)
                    {
                        startingSum = value.stroke.sum().checksum;
                    }
                    this.SetTag(sumTag, value.stroke.sum().checksum.ToString());
                    this.SetTag(startingSumTag, startingSum);
                    this.SetTag(pointsTag, strokeToPoints(value.stroke));
                    this.SetTag(colorTag, strokeToColor(value.stroke));
                    this.SetTag(thicknessTag, value.stroke.DrawingAttributes.Width.ToString());
                    this.SetTag(highlighterTag, value.stroke.DrawingAttributes.IsHighlighter.ToString());
                    this.SetTag(authorTag, value.author);
                    this.SetTag(targetTag, value.target);
                    this.SetTag(privacyTag, value.privacy);
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
                    points.Add(new StylusPoint
                    {
                        X = Double.Parse(pointInfo[i++]),
                        Y = Double.Parse(pointInfo[i++]),
                        PressureFactor = (float)((Double.Parse(pointInfo[i++]) / 255.0))
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
                    Width = width
                };

                InkCanvas.SetLeft(textBox, x);
                InkCanvas.SetTop(textBox, y);
                return textBox;
            }
            public TargettedTextBox Box
            {
                get
                {
                    var box = new TargettedTextBox(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), GetTag(privacyTag), this, GetTag(identityTag));
                    return box;
                }
                set
                {
                    var Dispatcher = value.boxProperty.Dispatcher;
                    Dispatcher.adopt(() =>
                    {
                        this.height = value.boxProperty.Height;
                        this.width = value.boxProperty.Width;
                        this.caret = value.boxProperty.CaretIndex;
                        this.x = InkCanvas.GetLeft(value.boxProperty);
                        this.y = InkCanvas.GetTop(value.boxProperty);
                        this.text = value.boxProperty.Text;
                        this.tag = (string)value.boxProperty.Tag;
                        this.style = value.boxProperty.FontStyle;
                        this.family = value.boxProperty.FontFamily;
                        this.weight = value.boxProperty.FontWeight;
                        this.size = value.boxProperty.FontSize;
                        this.decoration = value.boxProperty.TextDecorations;
                        this.SetTag(authorTag, value.author);
                        this.SetTag(identityTag, value.boxProperty.tag().id);
                        this.SetTag(targetTag, value.target);
                        this.SetTag(privacyTag, value.privacy);
                        this.SetTag(slideTag, value.slide);
                        this.color = value.boxProperty.Foreground;
                    });
                }
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
            public TargettedFile fileResource
            {
                get
                {
                    var fileuploadTime = HasTag(TIME) ? GetTag(TIME) : DateTimeFactory.Now().ToString();
                    var filesize = HasTag(SIZE) ? long.Parse(GetTag(SIZE)) : 0;
                    var filename = HasTag(NAME) ? GetTag(NAME) : Path.GetFileNameWithoutExtension(GetTag(URL));
                    var slide = HasTag(slideTag) ? GetTag(slideTag) : "0";
                    var target = HasTag(targetTag) ? GetTag(targetTag) : "";
                    var privacy = HasTag(privacyTag) ? GetTag(privacyTag) : "public";
                    var file = new TargettedFile(Int32.Parse(slide), GetTag(authorTag), target, privacy, GetTag(URL), fileuploadTime, filesize, filename);
                    return file;
                }
                set
                {
                    SetTag(AUTHOR, value.author);
                    SetTag(URL, value.url);
                    SetTag(TIME, value.uploadTime);
                    SetTag(SIZE, value.size);
                    SetTag(NAME, value.name);
                }
            }
        }
        public class LocalFileInformation
        {
            public LocalFileInformation(int Slide, string Author, string Target, string Privacy, string File, string Name, bool Overwrite, long Size, string UploadTime)
            {
                slide = Slide;
                author = Author;
                target = Target;
                privacy = Privacy;
                file = File;
                name = Name;
                overwrite = Overwrite;
                size = Size;
                uploadTime = UploadTime;
            }
            public string author;
            public string file;
            public bool overwrite;
            public string name;
            public string privacy;
            public long size;
            public int slide;
            public string target;
            public string uploadTime;

            public LocalFileInformation()
            {
            }
        }
        public class LocalImageInformation
        {
            public LocalImageInformation(int Slide, string Author, string Target, string Privacy, System.Windows.Controls.Image Image, string File, bool Overwrite)
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
            public string privacy;
            public int slide;
            public string target;

            public LocalImageInformation()
            {
            }
        }
        public class LocalSubmissionInformation
        {
            public LocalSubmissionInformation(int Slide, string Author, string Target, string Privacy, string File)
            {
                slide = Slide;
                author = Author;
                target = Target;
                privacy = Privacy;
                file = File;
            }
            public string author;
            public string file;
            public string privacy;
            public int slide;
            public string target;
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
            public static string SLIDE = "slide";
            public static string TIME = "time";

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
            public TargettedSubmission parameters
            {
                get
                {
                    return new TargettedSubmission(int.Parse(GetTag(SLIDE)), GetTag(AUTHOR), GetTag(targetTag), GetTag(privacyTag), GetTag(URL), long.Parse(GetTag(TIME)));
                }
                set
                {
                    SetTag(AUTHOR, value.author);
                    SetTag(URL, value.url);
                    SetTag(SLIDE, value.slide.ToString());
                    SetTag(TIME, value.time.ToString());
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

            public QuizAnswer parameters
            {
                get
                {
                    return new QuizAnswer(long.Parse(GetTag(ID)), GetTag(ANSWERER), GetTag(ANSWER));
                }
                set
                {
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
            public static readonly string TITLE = "title";
            public static readonly string QUESTION = "question";
            public static readonly string AUTHOR = "author";
            public static readonly string ID = "id";
            public static readonly string URL = "url";

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
            public QuizQuestion parameters
            {
                get
                {
                    var quiz = new QuizQuestion(long.Parse(GetTag(ID)), GetTag(TITLE), GetTag(AUTHOR), GetTag(QUESTION), new List<Option>());
                    quiz.url = HasTag(URL) ? GetTag(URL) : "none";
                    foreach (var node in ChildNodes)
                    {
                        if (node.GetType() == typeof(QuizOption))
                            quiz.options.Add(((QuizOption)node).parameters);
                    }
                    return quiz;
                }
                set
                {

                    SetTag(TITLE, value.title);
                    SetTag(QUESTION, value.question);
                    SetTag(AUTHOR, value.author);
                    SetTag(ID, value.id.ToString());
                    SetTag(URL, value.url);
                    foreach (var option in value.options)
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
        public class Video : Element
        {
            public ResourceCache resourceCache;
            public MeTLServerAddress server;
            static Video()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(Video));
            }
            public static string TAG = "video";
            public Video()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Video(TargettedVideo video)
                : this()
            {
                this.Vid = video;
            }
            public Video adoptCache(ResourceCache cache, MeTLServerAddress server)
            {
                this.resourceCache = cache;
                this.server = server;
                return this;
            }
            public MeTLLib.DataTypes.Video forceEvaluation()
            {
                var video = new MediaElement
                {
                    Tag = this.tag,
                    LoadedBehavior = MediaState.Manual,
                    Source = source
                };
                MeTLLib.DataTypes.Video srVideo = new MeTLLib.DataTypes.Video
                {
                    MediaElement = video,
                    Tag = this.tag,
                    VideoSource = video.Source,
                    VideoHeight = video.NaturalVideoHeight,
                    VideoWidth = video.NaturalVideoWidth
                };
                return srVideo;
            }
            public TargettedVideo Vid
            {
                get
                {
                    var targettedVideo =
                        new TargettedVideo(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), GetTag(privacyTag),
                        this, GetTag(identityTag), Double.Parse(GetTag(xTag)), Double.Parse(GetTag(yTag)), Double.Parse(GetTag(widthTag)), Double.Parse(GetTag(heightTag)));
                    if (resourceCache != null && server != null) targettedVideo.adoptCache(resourceCache, server);
                    return targettedVideo;
                }
                set
                {
                    var Dispatcher = value.videoProperty.Dispatcher;
                    Dispatcher.adopt(() =>
                    {
                        var absolutePath = value.videoProperty.VideoSource != null ? value.videoProperty.VideoSource.ToString() : value.videoProperty.MediaElement.Source.ToString();
                        SetTag(tagTag, value.videoProperty.Tag.ToString());
                        SetTag(sourceTag, absolutePath);
                        SetTag(xTag, value.X.ToString());
                        SetTag(yTag, value.Y.ToString());
                        SetTag(heightTag, (value.videoProperty.Height).ToString());
                        SetTag(widthTag, (value.videoProperty.Width).ToString());
                        SetTag(authorTag, value.author);
                        SetTag(targetTag, value.target);
                        SetTag(privacyTag, value.privacy);
                        SetTag(slideTag, value.slide);
                        SetTag(identityTag, value.id);
                    });
                }
            }
            private static readonly string widthTag = "width";
            private static readonly string heightTag = "height";
            private static readonly string sourceTag = "source";
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
            public string tag
            {
                get { return GetTag(tagTag); }
                set { SetTag(tagTag, value); }
            }
            private Uri getCachedVideo(string url)
            {
                //how do I ensure that there's a resourceCache here?  How did I end up here without one?
                return resourceCache.LocalSource(url);
            }
            public Uri source
            {
                get
                {
                    return getCachedVideo(GetTag(sourceTag));
                }
                set { SetTag(sourceTag, value.ToString()); }
            }
        }
        public class Image : Element
        {
            private MeTLServerAddress server;
            private ResourceCache cache;
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
            public Image adoptCache(ResourceCache cache, MeTLServerAddress server)
            {
                this.cache = cache;
                this.server = server;
                return this;
            }
            public System.Windows.Controls.Image forceEvaluation()
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                    {
                        Tag = this.tag,
                        Source = this.source,
                        Height = this.height,
                        Width = this.width
                    };
                    InkCanvas.SetLeft(image, this.x);
                    InkCanvas.SetTop(image, this.y);
                return image;
            }
            public string GetCachedImage(string url)
            {

                try
                {
                    return cache.LocalSource(url).ToString();
                }
                catch (Exception e)
                {
                    return "resources/slide_not_loaded.png";
                }
            }

            public TargettedImage Img
            {
                get
                {
                    var targettedImage = new TargettedImage(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), GetTag(privacyTag), this, GetTag(identityTag));
                    return targettedImage;
                }
                set
                {
                    var Dispatcher = value.imageProperty.Dispatcher;
                    Dispatcher.adopt(() =>
                        {
                            var absolutePath = value.imageProperty.Source.ToString();
                            var uri = new Uri(absolutePath, UriKind.RelativeOrAbsolute);
                            string relativePath;
                            if (uri.IsAbsoluteUri)
                                relativePath = uri.LocalPath;
                            else
                                relativePath = uri.ToString();
                            SetTag(tagTag, value.imageProperty.Tag.ToString());
                            SetTag(sourceTag, relativePath);
                            SetTag(widthTag, value.imageProperty.Width.ToString());
                            SetTag(heightTag, value.imageProperty.Height.ToString());
                            SetTag(xTag, InkCanvas.GetLeft(value.imageProperty).ToString());
                            SetTag(yTag, InkCanvas.GetTop(value.imageProperty).ToString());
                            SetTag(authorTag, value.author);
                            SetTag(targetTag, value.target);
                            SetTag(privacyTag, value.privacy);
                            SetTag(slideTag, value.slide);
                            SetTag(identityTag, value.id);
                        });
                }
            }
            private static readonly string sourceTag = "source";
            private static readonly string heightTag = "height";
            private static readonly string widthTag = "width";
            public string tag
            {
                get { return GetTag(tagTag); }
                set { SetTag(tagTag, value); }
            }
            public ImageSource source
            {
                get
                {
                    try
                    {
                        var path = string.Format("https://{0}:1188{1}", server.host, GetTag(sourceTag));
                        return (ImageSource)new ImageSourceConverter().ConvertFromString(GetCachedImage(path));
                    }
                    catch (Exception e)
                    {
                        return BitmapSource.Create(1, 1, 96, 96, PixelFormats.BlackWhite, BitmapPalettes.BlackAndWhite, new byte[96 * 96], 1);
                    }
                }
                set { SetTag(sourceTag, new ImageSourceConverter().ConvertToString(value)); }
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
                    return new TargettedDirtyElement(Int32.Parse(GetTag(slideTag)), GetTag(authorTag), GetTag(targetTag), GetTag(privacyTag), GetTag(identityTag));
                }
                set
                {
                    SetTag(authorTag, value.author);
                    SetTag(slideTag, value.slide);
                    SetTag(targetTag, value.target);
                    SetTag(privacyTag, value.privacy);
                    SetTag(identityTag, value.identifier);
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
}
