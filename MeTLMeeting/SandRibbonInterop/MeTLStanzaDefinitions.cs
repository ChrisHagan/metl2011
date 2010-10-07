using System;
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
using SandRibbonObjects;
using Constants;
using System.Collections.Generic;
using Path=System.IO.Path;

namespace SandRibbonInterop.MeTLStanzas
{
    public class TargettedElement
    {
        public string author { get; set;}
        public string target;
        public string privacy;
        public int slide;
    }
    public class TargettedAutoShape : TargettedElement
    {
        public AutoShape autoshape;
    }
    public class TargettedSubmission : TargettedElement
    {
        public string url { get; set; }
        public long time{ get; set;}

    }
    public class TargettedStroke : TargettedElement
    {
        public Stroke stroke;
        public double startingChecksum;
    }
    public class TargettedBubbleContext : TargettedElement {
        public IEnumerable<SelectedIdentity> context;
        public int thoughtSlide;
    }
    public class TargettedFile : TargettedElement
    {
        public string url { get; set;}
        public string uploadTime { get; set; }
        public long size { get; set; }
        public string name { get; set; }
    }
    public class TargettedImage : TargettedElement
    {
        public System.Windows.Controls.Image imageProperty;
        public MeTLStanzas.Image imageSpecification;
        public string id;
        public System.Windows.Controls.Image image
        {
            get
            {
                var reified = imageSpecification.forceEvaluation();
                id = reified.tag().id;
                return reified;
            }
            set
            {
                id = value.tag().id;
                imageProperty = value;
            }
        }
    }
    public class TargettedVideo : TargettedElement 
    {
        public SandRibbonInterop.Video videoProperty;
        public MeTLStanzas.Video videoSpecification;
        public string id;
        public SandRibbonInterop.Video video { 
            get{
                var reified = videoSpecification.forceEvaluation();
                id = reified.tag().id;
                reified.Height = Height;
                reified.Width = Width;
                reified.X = X;
                reified.Y = Y;
                return reified;
            }
            set {
                id = value.tag().id;
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
    public class TargettedPowerpointBackgroundVideo : TargettedElement
    {
        public MediaElement videoProperty;
        public MeTLStanzas.PowerpointVideo videoSpecification;
        public string identity;
        public MediaState playMode;
        public string animationTimeLine;
        public double height;
        public double width;
        public string tag;
        public MeTLStanzas.PowerpointVideo video
        {
            get
            {
                var reified = videoSpecification.forceEvaluation();
                return reified;
            }
            set
            {
                videoProperty = value.video;
            }
        }
    }
    public class TargettedTextBox : TargettedElement
    {
        public TextBox boxProperty;
        public MeTLStanzas.TextBox boxSpecification;
        public string identity;
        public System.Windows.Controls.TextBox box
        {
            get
            {
                var reified = boxSpecification.forceEvaluation();
                identity = reified.tag().id;
                return reified;
            }
            set
            {
                identity = value.tag().id;
                boxProperty = value;
            }
        }
    }
    public class TargettedDirtyElement : TargettedElement
    {
        public string identifier;
    }
    public class SelectedIdentity { 
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

        public class PowerpointVideo : Element
        {
            static PowerpointVideo()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(PowerpointVideo.TAG, METL_NS, typeof(PowerpointVideo));
            }
            public PowerpointVideo forceEvaluation()
            {
                return this;
            }
            public static readonly string TAG = "powerpointvideo";
            public PowerpointVideo()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public PowerpointVideo(TargettedPowerpointBackgroundVideo video)
                : this()
            {
                this.video = video.video.video;
            }

            private string AnimationTimeLineTag = "animationTimeLine";
            private string PlayModeTag = "playmode";
            private string HeightTag = "height";
            private string WidthTag = "width";
            private string XTag = "x";
            private string YTag = "y";
            public MediaElement video
            {
                get
                {
                    var newVideo = new MediaElement();
                    newVideo.Height = this.height;
                    newVideo.Width = this.width;
                    newVideo.LoadedBehavior = this.PlayMode;
                    InkCanvas.SetLeft(this.video, this.x);
                    InkCanvas.SetTop(this.video, this.y);
                    return newVideo;
                }
                set
                {
                }
            }
            public TargettedPowerpointBackgroundVideo targettedVideo
            {
                get
                {
                    var targettedVideo = new TargettedPowerpointBackgroundVideo
                    {
                        video = new MeTLStanzas.PowerpointVideo
                        {
                            PlayMode = this.PlayMode,
                        },
                        slide = Int32.Parse(GetTag(slideTag)),
                        animationTimeLine = this.AnimationTimeLine,
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        author = GetTag("author")
                    };
                    return targettedVideo;
                }
                set
                {
                    SetTag(tagTag, value.videoProperty.Tag.ToString());
                    if (value.video.AnimationTimeLine != null)
                        SetTag(AnimationTimeLineTag, value.video.AnimationTimeLine.ToString());
                    else throw new InvalidDataException("Trying to create a PowerpointVideo around invalid AnimationTimeLine Data: " + value.video.AnimationTimeLine.ToString());
                    SetTag(PlayModeTag, value.video.PlayMode.ToString());
                    SetTag(widthTag, value.videoProperty.Width.ToString());
                    SetTag(heightTag, value.videoProperty.Height.ToString());
                    SetTag(xTag, InkCanvas.GetLeft(value.videoProperty).ToString());
                    SetTag(yTag, InkCanvas.GetTop(value.videoProperty).ToString());
                    SetTag(authorTag, value.author);
                    SetTag(targetTag, value.target);
                    SetTag(privacyTag, value.privacy);
                    SetTag(slideTag, value.slide);
                }
            }
            private static readonly string heightTag = "height";
            private static readonly string widthTag = "width";
            public MediaState PlayMode
            {
                //I'm not sure exactly how to parse into an enum - I would've thought it'd be this.
                get { return (MediaState)Enum.Parse(typeof(MediaState), GetTag(PlayModeTag).ToString()); }
                set { SetTag(PlayModeTag, value.ToString()); }
            }
            public string AnimationTimeLine
            {
                get { return GetTag(AnimationTimeLineTag); }
                set { SetTag(tagTag, value); }
            }
            public string tag
            {
                get { return GetTag(tagTag); }
                set { SetTag(tagTag, value); }
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
                    var targettedAutoShape = new TargettedAutoShape
                    {
                        autoshape = new SandRibbonInterop.AutoShape
                        {
                            Tag = this.tag,
                            PathData = this.pathData,
                            Foreground = this.Foreground,
                            Background = this.Background,
                            StrokeThickness = this.StrokeThickness,
                            Height = this.height,
                            Width = this.width,
                        },
                        slide = Int32.Parse(GetTag(slideTag)),
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        author = GetTag("author")
                    };
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
                    //var newPathFigureCollection = (PathFigureCollection)(new PathFigureCollectionConverter().ConvertFromString("F1M47.7778,48.6667L198,48.6667L198,102C174.889,91.3334,157.111,79.7778,110.889,114.444C64.667,149.111,58.4444,130.444,47.7778,118.889z"));
                    //var newPathFigureCollection = (PathFigureCollection)(new PathFigureCollectionConverter().ConvertFromString(GetTag(PathDataTag.ToString()).ToString()));
                    newPathData.AddGeometry((Geometry)new GeometryConverter().ConvertFromString(GetTag(PathDataTag)));
                    //newPathData.Figures = newPathFigureCollection;
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
            static Bubble() { 
                agsXMPP.Factory.ElementFactory.AddElementType(Bubble.TAG, METL_NS, typeof(Bubble));
            }
            public readonly static string TAG = "bubble";
            public Bubble() {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Bubble(TargettedBubbleContext context) : this() {
                this.context = context;
            }
            private string idsTag = "IDS";
            private string thoughtTag = "THOUGHTSLIDE";
            private string entityIdTag = "ENTITY";
            private string idAttribute = "ID";
            public TargettedBubbleContext context {
                get 
                {
                    var target = GetTag(targetTag);
                    var context = new TargettedBubbleContext
                    {
                        slide = Int32.Parse(GetTag(slideTag)),
                        thoughtSlide = Int32.Parse(GetTag(thoughtTag)),
                        target = target,
                        privacy = GetTag(privacyTag),
                        author = GetTag("author")
                    };
                    var ids = SelectSingleElement(idsTag).SelectElements(entityIdTag);
                    var identityList = new List<SelectedIdentity>();
                    foreach(var element in ids){
                        identityList.Add(new SelectedIdentity
                        {
                            target = target,
                            id = ((Element)element).GetAttribute(idAttribute)
                        });
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
                    foreach(var selectedIdentity in value.context){
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

            public TargettedStroke Stroke
            {
                get
                {
                    var stroke = new Stroke(stringToPoints(GetTag(pointsTag)), new DrawingAttributes { Color = stringToColor(GetTag(colorTag)) });
                    stroke.tag(new StrokeTag { author = GetTag("author"), privacy = GetTag(privacyTag), startingColor = stroke.DrawingAttributes.Color.ToString()});
                    stroke.DrawingAttributes.IsHighlighter = Boolean.Parse(GetTag(highlighterTag));
                    stroke.DrawingAttributes.Width = Double.Parse(GetTag(thicknessTag));
                    stroke.DrawingAttributes.Height = Double.Parse(GetTag(thicknessTag));
                    if (HasTag(sumTag))
                        stroke.AddPropertyData(stroke.sumId(), Double.Parse(GetTag(sumTag)));
                    if (HasTag(startingSumTag))
                        stroke.AddPropertyData(stroke.startingId(), Double.Parse(GetTag(startingSumTag)));
                    else
                        if(HasTag(sumTag)) 
                            stroke.AddPropertyData(stroke.startingId(), Double.Parse(GetTag(sumTag)));
                    var targettedStroke = new TargettedStroke
                    {
                        slide = Int32.Parse(GetTag(slideTag)),
                        stroke = stroke,
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        author = GetTag("author")
                    };
                    return targettedStroke;
                }
                set
                {
                    this.SetTag(sumTag, value.stroke.sum().checksum.ToString());
                    this.SetTag(startingSumTag, value.stroke.startingSum());
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
                var textBox = new System.Windows.Controls.TextBox
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
                    var box = new TargettedTextBox
                    {
                        slide = Int32.Parse(GetTag(slideTag)),
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        author = GetTag("author"),
                        boxSpecification = this,
                        identity = GetTag(identityTag)
                    };
                    return box;
                }
                set
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
        
        public class FileResource: Element
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
            public FileResource(TargettedFile file) : this()
            {
                fileResource = file;
            }
            public TargettedFile fileResource
            {
                get
                {
                    var file =  new TargettedFile
                               {
                                   author = GetTag(AUTHOR),
                                   url = GetTag(URL)
                               };
                    file.uploadTime = HasTag(TIME) ? GetTag(TIME) : SandRibbonObjects.DateTimeFactory.Now().ToString();
                    file.size = HasTag(SIZE) ? long.Parse(GetTag(SIZE)) : 0;
                    file.name = HasTag(NAME) ? GetTag(NAME) : Path.GetFileNameWithoutExtension(file.url);
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
        public class ScreenshotSubmission: Element
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
            public ScreenshotSubmission(TargettedSubmission submission):this()
            {
                this.parameters = submission;
            }
            public TargettedSubmission parameters
            {
                get
                {
                    return new TargettedSubmission
                               {
                                   author = GetTag(AUTHOR),
                                   url = GetTag(URL),
                                   slide = int.Parse(GetTag(SLIDE)),
                                   time = long.Parse(GetTag(TIME))
                               };
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

        public class QuizOption: Element
        {
            static QuizOption()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(QuizOption));
            }
            public static string TAG = "quizOption";
            public static readonly string NAME = "name";
            public static readonly string TEXT = "text";
            public static readonly string CORRECT = "correct";
            public static readonly string COLOR= "color";

            public QuizOption()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public QuizOption(Option parameters):this()
            {
                this.parameters = parameters;
            }
            public Option parameters
            {
                get
                {
                    return new Option
                               {
                                   name = GetTag(NAME),
                                   correct = GetTag(CORRECT).ToString().ToLower() == "true",
                                   optionText = GetTag(TEXT),
                                   color = Ink.stringToColor(GetTag(COLOR))

                               };

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
        public class QuizResponse: Element
        {
            static QuizResponse()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof (QuizResponse));
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
            public QuizResponse(QuizAnswer parameters):this()
            {
                this.parameters = parameters;
            }

            public QuizAnswer parameters
            {
                get
                {
                    return new QuizAnswer
                               {
                                   answer = GetTag(ANSWER),
                                   answerer = GetTag(ANSWERER),
                                   id = long.Parse(GetTag(ID))
                               };
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
                    var quiz = new QuizQuestion
                               {
                                   title = GetTag(TITLE),
                                   question = GetTag(QUESTION),
                                   author = GetTag(AUTHOR),
                                   id = long.Parse(GetTag(ID))
                               };
                    quiz.url = HasTag(URL) ? GetTag(URL) : "none";
                    foreach(var node in ChildNodes)
                    {
                        if(node.GetType() == typeof(QuizOption))
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
                    foreach(var option in value.options)
                    {
                        var optionElement= new QuizOption(option);
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
                    return new LiveWindowSetup
                    {
                        frame = new Rectangle
                        {
                            Width = Double.Parse(GetTag(widthTag)),
                            Height = Double.Parse(GetTag(heightTag))
                        },
                        origin = new Point(
                            Double.Parse(GetTag(xTag)),
                            Double.Parse(GetTag(yTag))),
                        target = new Point(
                            Double.Parse(GetTag(destXTag)),
                            Double.Parse(GetTag(destYTag))),
                        snapshotAtTimeOfCreation = GetTag(snapshotTag),
                        author = GetTag(authorTag),
                        slide = Int32.Parse(GetTag(slideTag))
                    };
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
        public class Video : Element {
            static Video() { 
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(Video));
            }
            public static string TAG = "video";
            public Video()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Video(TargettedVideo video) : this() {
                this.Vid = video;
            }
            public SandRibbonInterop.Video forceEvaluation() {
                var video = new MediaElement { 
                    Tag = this.tag,
                    LoadedBehavior = MediaState.Manual,
                    Source = source
                };
                var srVideo = new SandRibbonInterop.Video { 
                    MediaElement = video, 
                    Tag=this.tag, 
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
                    var targettedVideo = new TargettedVideo
                    {
                        videoSpecification = this,
                        slide = Int32.Parse(GetTag(slideTag)),
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        author = GetTag(authorTag),
                        id = GetTag(identityTag),
                        Height = Double.Parse(GetTag(heightTag)),
                        Width = Double.Parse(GetTag(widthTag)),
                        X = Double.Parse(GetTag(xTag)),
                        Y = Double.Parse(GetTag(yTag))
                    };
                    return targettedVideo;
                }
                set
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
            private static Uri getCachedVideo(string url)
            {
                return LocalCache.ResourceCache.LocalSource(url);
            }
            public Uri source
            {
                get {
                    return getCachedVideo(GetTag(sourceTag));
                }
                set { SetTag(sourceTag, value.ToString()); }
            }
        }
        public class Image : Element
        {
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
            public System.Windows.Controls.Image forceEvaluation()
            {
                var image = new System.Windows.Controls.Image
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
            public static string GetCachedImage(string url)
            {

                try
                {
                    return LocalCache.ResourceCache.LocalSource(url).ToString();
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
                    var targettedImage = new TargettedImage
                    {
                        imageSpecification = this,
                        slide = Int32.Parse(GetTag(slideTag)),
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        author = GetTag("author"),
                        id = GetTag(identityTag)
                    };
                    return targettedImage;
                }
                set
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
                        var path = string.Format("https://{0}:1188//{1}", JabberWire.SERVER, GetTag(sourceTag));
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
                    return new TargettedDirtyElement
                    {
                        author = GetTag(authorTag),
                        slide = Int32.Parse(GetTag(slideTag)),
                        target = GetTag(targetTag),
                        privacy = GetTag(privacyTag),
                        identifier = GetTag(identityTag)
                    };
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