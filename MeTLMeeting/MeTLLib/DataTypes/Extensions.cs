using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows;
using System.Diagnostics;

namespace MeTLLib.DataTypes
{
    public struct TextTag
    {
        public TextTag(string Author, Privacy Privacy, string Id)
        {
            author = Author;
            privacy = Privacy; 
            id = Id;
        }
        public TextTag(TextTag copyTag, Privacy newPrivacy)
        {
            // copy everything except for privacy
            author = copyTag.author;
            id = copyTag.id;
            privacy = newPrivacy;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is TextTag)) return false;
            var foreignTextTag = ((TextTag)obj);
            return ((foreignTextTag.author == author)
                && (foreignTextTag.id == id)
                && (foreignTextTag.privacy == privacy));
        }
        public string author;
        public Privacy privacy;
        public string id;
    }

    public struct ImageTag
    {
        public ImageTag(string Author, Privacy Privacy, string Id, bool IsBackground, int ZIndex = 0)
        {
            author = Author;
            privacy = Privacy; 
            id = Id;
            isBackground = IsBackground;
            zIndex = ZIndex;
        }
        public ImageTag(ImageTag copyTag, Privacy newPrivacy)
        {
            // copy everything except privacy
            author = copyTag.author;
            id = copyTag.id;
            isBackground = copyTag.isBackground;
            zIndex = copyTag.zIndex;

            privacy = newPrivacy;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is ImageTag)) return false;
            var foreignImageTag = ((ImageTag)obj);
            return ((foreignImageTag.author == author)
                && (foreignImageTag.id == id)
                && (foreignImageTag.privacy == privacy)
                && (foreignImageTag.isBackground == isBackground)
                && (foreignImageTag.zIndex == zIndex));
        }
        public string author;
        public Privacy privacy;
        public string id;
        public bool isBackground;
        public int zIndex;
    }
    public struct StrokeTag
    {
        public StrokeTag(string Author, Privacy Privacy, string strokeId, double StartingSum, bool IsHighlighter)
        {
            id = strokeId;
            author = Author;
            privacy = Privacy; 
            startingSum = StartingSum;
            isHighlighter = IsHighlighter;
        }
        public StrokeTag(StrokeTag copyTag, Privacy newPrivacy)
        {
            id = copyTag.id; 
            author = copyTag.author;
            startingSum = copyTag.startingSum;
            isHighlighter = copyTag.isHighlighter;

            privacy = newPrivacy;
        }

        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is StrokeTag)) return false;
            var foreignStrokeTag = ((StrokeTag)obj);
            return ((foreignStrokeTag.author == author)
                && (foreignStrokeTag.id == id)
                && (foreignStrokeTag.isHighlighter == isHighlighter));
        }
        public string id; 
        public string author;
        public Privacy privacy;
        public double startingSum;
        public bool isHighlighter;
    }
    public struct StrokeChecksum
    {
        public StrokeChecksum(double Checksum)
        {
            checksum = Checksum;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is StrokeChecksum)) return false;
            return (((StrokeChecksum)obj).checksum == checksum);
        }
        public double checksum;
    }
    public struct UserHighlight
    {
        public UserHighlight(string User, Color Color)
        {
            user = User;
            color = Color;
        }
        public Color color;
        public string user;
    }
    public static class TextExtensions
    {
        //Image and TextTags are identical, reusing imagetag
        public static TextTag tag(this TextBox box )
        {
            var texttag = new TextTag();
            box.Dispatcher.adopt(delegate
            {
                var textInfo = JsonConvert.DeserializeObject<ImageTag>(box.Tag.ToString(), new StringEnumConverter());
                texttag = new TextTag
                {
                    author = textInfo.author,
                    id = textInfo.id,
                    privacy = textInfo.privacy
                };
            });
            return texttag;
        }
        public static TextTag tag(this TextBox box, TextTag tag)
        {
            box.Dispatcher.adopt(delegate
            {
                box.Tag = JsonConvert.SerializeObject(tag, new StringEnumConverter());
            });
            return tag;
        }

        public static bool HasValidTag(this TextBox box)
        {
            return box.Tag != null && box.Tag is string;
        }
    }
    public static partial class VideoExtensions 
    { 
        public static ImageTag tag(this MeTLLib.DataTypes.Video image )
        {
            var imagetag = new ImageTag();
            image.Dispatcher.adopt(delegate
            {
                var imageInfo = JsonConvert.DeserializeObject<ImageTag>(image.Tag.ToString(), new StringEnumConverter());
                imagetag = new ImageTag
                {
                    author = imageInfo.author,
                    id = imageInfo.id,
                    privacy = imageInfo.privacy,
                    isBackground = imageInfo.isBackground,
                    zIndex = imageInfo.zIndex
                };
            });
            return imagetag;
        }
        public static ImageTag tag(this MeTLLib.DataTypes.Video image, ImageTag tag)
        {
            image.Dispatcher.adopt(delegate
            {
                image.Tag = JsonConvert.SerializeObject(tag, new StringEnumConverter());
            });
            return tag;
        }
    }
    public static class FrameworkElementExtensions {
        public static Privacy privacy(this FrameworkElement element) {
            if (element is Video) return (element as Video).tag().privacy;
            if (element is Image) return (element as Image).tag().privacy;
            if (element is TextBox) return (element as TextBox).tag().privacy;
            throw new Exception(string.Format("Target type {0} does not support MeTL tagging", element.GetType()));
        }
    }
    public static class ImageExtensions
    {
        public static Image clone(this Image image)
        {
            var newImage = new Image {Height = image.Height, Width = image.Width};
            newImage.tag(image.tag());
            newImage.Source = image.Source;
            newImage.Margin = image.Margin;
            InkCanvas.SetLeft(newImage, InkCanvas.GetLeft(image));
            InkCanvas.SetTop(newImage, InkCanvas.GetTop(image));

            return newImage;
        }
        public static ImageTag tag(this Image image)
        {
            ImageTag imagetag = new ImageTag();
            image.Dispatcher.adopt(delegate
            {
                try
                {
                    ImageTag imageInfo = new ImageTag(); 
                    if (image.Tag.ToString().StartsWith("NOT_LOADED"))
                        imageInfo = JsonConvert.DeserializeObject<ImageTag>(image.Tag.ToString().Split(new[] { "::::" }, StringSplitOptions.RemoveEmptyEntries)[2], new StringEnumConverter());
                    else
                        imageInfo = JsonConvert.DeserializeObject<ImageTag>(image.Tag.ToString(), new StringEnumConverter());

                    imagetag = new ImageTag
                    {
                        author = imageInfo.author,
                        id = imageInfo.id,
                        privacy = imageInfo.privacy,
                        isBackground = imageInfo.isBackground,
                        zIndex = imageInfo.zIndex
                    };
                }
                catch (Exception e)
                {
                    Trace.TraceError("Error making ImageTag =>" + e);
                    imagetag = new ImageTag
                    {
                        author = "unknown",
                        id = "unknown",
                        privacy = Privacy.Private,
                        isBackground = false,
                        zIndex = 1
                    };
                }
            });
            return imagetag;
        }
        public static ImageTag tag(this Image image, ImageTag tag)
        {
            image.Dispatcher.adopt(delegate
            {
                image.Tag = JsonConvert.SerializeObject(tag, new StringEnumConverter());
            });
            return tag;
        }
    }
    public static class StrokeExtensions
    {
        private static Guid STROKE_TAG_GUID = Guid.NewGuid();
        private static Guid STROKE_PRIVACY_GUID = Guid.NewGuid();
        private static Guid STARTINGCHECKSUM = Guid.NewGuid();
        private static Guid STARTING_COLOR = Guid.NewGuid();
        private static Guid IS_HIGHLIGHTER = Guid.NewGuid();
        private static Guid STROKE_IDENTITY_GUID = Guid.NewGuid();
        private static readonly string NONPERSISTENT_STROKE = "nonPersistent";
        public static Privacy privacy(this Stroke stroke){
            return stroke.tag().privacy;
        }
        public static StrokeTag tag(this Stroke stroke)
        {
            var stroketag = new StrokeTag();
            stroketag = new StrokeTag
                       {
                           author = (string) stroke.GetPropertyData(STROKE_TAG_GUID),
                           privacy = (Privacy)Enum.Parse(typeof(Privacy), (string)stroke.GetPropertyData(STROKE_PRIVACY_GUID), true),
                           id = (string) stroke.GetPropertyData(STROKE_IDENTITY_GUID),
                           isHighlighter = (bool) stroke.GetPropertyData(IS_HIGHLIGHTER)
                       };
            return stroketag;
        }
        public static StrokeTag tag(this Stroke stroke, StrokeTag tag)
        {
            stroke.AddPropertyData(STROKE_TAG_GUID, tag.author);
            var privacy = Privacy.Private;
            if (tag.privacy != Privacy.NotSet)
                privacy = tag.privacy;
            stroke.AddPropertyData(STROKE_IDENTITY_GUID, tag.id);
            stroke.AddPropertyData(STROKE_PRIVACY_GUID, privacy.ToString());
            stroke.AddPropertyData(IS_HIGHLIGHTER, tag.isHighlighter);
            return tag;
        }
        private static Guid CHECKSUM = Guid.NewGuid();
        public static void doNotPersist(this Stroke stroke)
        {
            var oldTag = stroke.tag();
            var newTag = new StrokeTag(
                NONPERSISTENT_STROKE,
                oldTag.privacy,
                oldTag.id,
                oldTag.startingSum,
                oldTag.isHighlighter
                );
            stroke.tag(newTag);
        }
        public static bool shouldPersist(this Stroke stroke)
        {
            return stroke.tag().author != NONPERSISTENT_STROKE;
        }
        public static Guid sumId(this Stroke stroke)
        {
            return StrokeExtensions.CHECKSUM;
        }
        public static Guid startingId(this Stroke stroke)
        {
            return STARTINGCHECKSUM;
        }
        public static double startingSum(this Stroke stroke)
        {
            try
            {
                return (double)stroke.GetPropertyData(STARTINGCHECKSUM);
            }
            catch (Exception)
            {
                return stroke.sum().checksum;
            }
        }
        public static double startingSum(this Stroke stroke, double startingSum)
        {
            stroke.AddPropertyData(STARTINGCHECKSUM, startingSum);
            return startingSum;
        }
        public static StrokeChecksum sum(this Stroke stroke)
        {
            var checksum = new StrokeChecksum
            {
                checksum = stroke.StylusPoints.Aggregate(0.0, (acc, item) =>
                    acc + Math.Round(item.X, POINT_DECIMAL_PLACE_ROUNDING) + Math.Round(item.Y, POINT_DECIMAL_PLACE_ROUNDING))
            };
            return checksum;
        }
        public static int POINT_DECIMAL_PLACE_ROUNDING = 1;
    }
}
