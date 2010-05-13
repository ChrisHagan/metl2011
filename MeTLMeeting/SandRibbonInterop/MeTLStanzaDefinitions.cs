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

namespace SandRibbonInterop.MeTLStanzas
{
    public class TargettedElement
    {
        public string author;
        public string target;
        public string privacy;
        public int slide;
    }
    public class TargettedAutoShape : TargettedElement
    {
        public AutoShape autoshape;
    }
    public class TargettedStroke : TargettedElement
    {
        public Stroke stroke;
    }
    public class TargettedBubbleContext : TargettedElement {
        public IEnumerable<SelectedIdentity> context;
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
            private string entityIdTag = "ENTITY";
            private string idAttribute = "ID";
            public TargettedBubbleContext context {
                get 
                {
                    var target = GetTag(targetTag);
                    var context = new TargettedBubbleContext
                    {
                        slide = Int32.Parse(GetTag(slideTag)),
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
            public TargettedStroke Stroke
            {
                get
                {
                    var stroke = new Stroke(stringToPoints(GetTag(pointsTag)), new DrawingAttributes { Color = stringToColor(GetTag(colorTag)) });
                    stroke.tag(new StrokeTag { author = GetTag("author"), privacy = GetTag(privacyTag) });
                    stroke.DrawingAttributes.IsHighlighter = Boolean.Parse(GetTag(highlighterTag));
                    stroke.DrawingAttributes.Width = Double.Parse(GetTag(thicknessTag));
                    stroke.DrawingAttributes.Height = Double.Parse(GetTag(thicknessTag));
                    if (HasTag(sumTag))
                        stroke.AddPropertyData(stroke.sumId(), Double.Parse(GetTag(sumTag)));
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
        public class QuizStatus : Element
        {
            static QuizStatus()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(QuizStatus));
            }
            public static string TAG = "quizStatus";
            public static readonly string statusTag = "currentStatus";
            public static readonly string parentTag = "parentTag";
            public QuizStatus()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public QuizStatus(QuizStatusDetails status)
                : this()
            {
                this.status = status;
                SetTag(privacyTag, "public");
            }
            public QuizStatusDetails status
            {
                get
                {
                    return new QuizStatusDetails
                    {
                        answerer = GetTag(answererTag),
                        status = GetTag(statusTag),
                        targetQuiz = Int32.Parse(GetTag(targetTag)),
                        quizParent = Int32.Parse(GetTag(parentTag))

                    };
                }
                set
                {
                    SetTag(statusTag, value.status);
                    SetTag(answererTag, value.answerer);
                    SetTag(targetTag, value.targetQuiz);
                    SetTag(parentTag, value.quizParent);
                }
            }
        }
        public class Answer : Element
        {
            static Answer()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(Answer));
            }
            public static string TAG = "answer";
            public static readonly string URL = "URL";
            public static readonly string TARGETQUIZ = "targetQuiz";
            public static readonly string ANSWER = "targetAnswer";
            public static readonly string ANSWERER_TAG = "targetAnswerer";
            public Answer()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Answer(QuizAnswer answer)
                : this()
            {
                this.quizAnswer = answer;
            }
            public QuizAnswer quizAnswer
            {
                get
                {
                    return new QuizAnswer
                               {
                                   answer = GetTag(ANSWER),
                                   answerURL = GetTag(URL),
                                   targetQuiz = Int32.Parse(GetTag(TARGETQUIZ)),
                                   answerer = GetTag(ANSWERER_TAG)
                               };
                }
                set
                {
                    SetTag(URL, value.answerURL);
                    SetTag(ANSWER, value.answer);
                    SetTag(ANSWERER_TAG, value.answerer);
                    SetTag(TARGETQUIZ, value.targetQuiz.ToString());
                }
            }
        }
        public class Quiz : Element
        {
            static Quiz()
            {
                agsXMPP.Factory.ElementFactory.AddElementType(TAG, METL_NS, typeof(Quiz));
            }
            public static string TAG = "poll";
            public static readonly string DESTINATION = "destination";
            public static readonly string OPTIONCOUNT = "options";
            public static readonly string TARGET = "target";
            public static readonly string ORIGIN = "origin";
            public static readonly string AUTHOR = "author";
            public static readonly string URL = "URL";
            public Quiz()
            {
                this.Namespace = METL_NS;
                this.TagName = TAG;
            }
            public Quiz(QuizDetails parameters)
                : this()
            {
                this.parameters = parameters;
            }
            public QuizDetails parameters
            {
                get
                {
                    return new QuizDetails
                               {
                                   returnSlide = Int32.Parse(GetTag(ORIGIN)),
                                   targetSlide = Int32.Parse(GetTag(DESTINATION)),
                                   target = GetTag(targetTag),
                                   optionCount = Int32.Parse(GetTag(OPTIONCOUNT)),
                                   author = GetTag(AUTHOR),
                                   quizPath = GetTag(URL)

                               };
                }
                set
                {
                    SetTag(DESTINATION, value.targetSlide.ToString());
                    SetTag(ORIGIN, value.returnSlide.ToString());
                    SetTag(OPTIONCOUNT, value.optionCount.ToString());
                    SetTag(targetTag, value.target.ToString());
                    SetTag(AUTHOR, value.author.ToString());
                    SetTag(URL, value.quizPath);
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
                Console.WriteLine(string.Format("Reified and laid out image {0}", this.tag));
                return image;
            }
            public static ImageSource GetCachedImage(string url)
            {
                try
                {
                    var regex = new Regex(@".*?/Resource/(.*?)/(.*)");
                    var match = regex.Matches(url)[0];
                    var room = match.Groups[1].Value;
                    var file = match.Groups[2].Value;
                    var path = string.Format(@"Resource\{0}\{1}", room, file);
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    ensureImageCacheDirectory(room);
                    if (File.Exists(path))
                    {
                        bitmapImage.StreamSource = new MemoryStream(File.ReadAllBytes(path));
                    }
                    else
                    {
                        var sourceBytes = new WebClient { Credentials = new NetworkCredential("exampleUsername", "examplePassword") }.DownloadData(url);
                        bitmapImage.StreamSource = new MemoryStream(sourceBytes);
                        File.WriteAllBytes(path, sourceBytes);
                    }
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
                catch (Exception e)
                {
                    return BitmapSource.Create(1, 1, 96, 96, PixelFormats.BlackWhite, BitmapPalettes.BlackAndWhite, new byte[96 * 96], 1);
                }
            }
            private static void ensureImageCacheDirectory(string room)
            {
                if (!Directory.Exists("Resource"))
                    Directory.CreateDirectory("Resource");
                var roomPath = System.IO.Path.Combine("Resource", room);
                if (!Directory.Exists(roomPath))
                    Directory.CreateDirectory(roomPath);
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
                        var path = string.Format("http://{0}:1188{1}", JabberWire.SERVER, GetTag(sourceTag));
                        return (ImageSource)new ImageSourceConverter().ConvertFromString(path);
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