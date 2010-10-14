using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Windows;

namespace MeTLLib.DataTypes
{
    public struct TextTag
    {
        public TextTag(string Author, string Privacy, string Id)
        {
            author = Author;
            privacy = Privacy;
            id = Id;
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
        public string privacy;
        public string id;
    }
    public struct ImageTag
    {
        public ImageTag(string Author, string Privacy, string Id, bool IsBackground, int ZIndex)
        {
            author = Author;
            privacy = Privacy;
            id = Id;
            isBackground = IsBackground;
            zIndex = ZIndex;
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
        public string privacy;
        public string id;
        public bool isBackground;
        public int zIndex;
    }
    public struct StrokeTag
    {
        public StrokeTag(string Author, string Privacy, double StartingSum, string StartingColor, bool IsHighlighter)
        {
            author = Author;
            privacy = Privacy;
            startingSum = StartingSum;
            startingColor = StartingColor;
            isHighlighter = IsHighlighter;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is StrokeTag)) return false;
            var foreignStrokeTag = ((StrokeTag)obj);
            return ((foreignStrokeTag.author == author)
                && (foreignStrokeTag.isHighlighter == isHighlighter)
                && (foreignStrokeTag.privacy == privacy)
                && (foreignStrokeTag.startingSum == startingSum)
                && (foreignStrokeTag.startingColor == startingColor));
        }
        public string author;
        public string privacy;
        public double startingSum;
        public string startingColor;
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
            var textInfo = JsonConvert.DeserializeObject<ImageTag>(box.Tag.ToString());
            return new TextTag 
            {
                author = textInfo.author,
                id = textInfo.id,
                privacy = textInfo.privacy
            };
        }
        public static TextTag tag(this TextBox box, TextTag tag)
        {
            box.Tag = JsonConvert.SerializeObject(tag);  
            return tag;
        }
    }
    public static class VideoExtensions 
    { 
        public static ImageTag tag(this MeTLLib.DataTypes.Video image )
        {
            var imageInfo = JsonConvert.DeserializeObject<ImageTag>(image.Tag.ToString());
            return new ImageTag 
            {
                author = imageInfo.author,
                id = imageInfo.id,
                privacy = imageInfo.privacy,
                isBackground = imageInfo.isBackground,
                zIndex = imageInfo.zIndex
            };
        }
        public static ImageTag tag(this MeTLLib.DataTypes.Video image, ImageTag tag)
        {
            image.Tag = JsonConvert.SerializeObject(tag);  
            return tag;
        }
    }
    public static class ImageExtensions
    {
        public static ImageTag tag(this Image image )
        {
            var imageInfo = JsonConvert.DeserializeObject<ImageTag>(image.Tag.ToString());
            return new ImageTag 
            {
                author = imageInfo.author,
                id = imageInfo.id,
                privacy = imageInfo.privacy,
                isBackground = imageInfo.isBackground,
                zIndex = imageInfo.zIndex
            };
        }
        public static ImageTag tag(this Image image, ImageTag tag)
        {
            image.Tag = JsonConvert.SerializeObject(tag);  
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

        public static StrokeTag tag(this Stroke stroke)
        {
            return new StrokeTag
                       {
                           author = (string) stroke.GetPropertyData(STROKE_TAG_GUID),
                           privacy = (string) stroke.GetPropertyData(STROKE_PRIVACY_GUID),
                           startingColor = (string) stroke.GetPropertyData(STARTING_COLOR),
                           isHighlighter = (bool) stroke.GetPropertyData(IS_HIGHLIGHTER)
                       };
        }
        public static StrokeTag tag(this Stroke stroke, StrokeTag tag)
        {
            stroke.AddPropertyData(STROKE_TAG_GUID, tag.author);
            stroke.AddPropertyData(STROKE_PRIVACY_GUID, tag.privacy);
            stroke.AddPropertyData(STARTING_COLOR, tag.startingColor.ToString());
            stroke.AddPropertyData(IS_HIGHLIGHTER, tag.isHighlighter);
            return tag;
        }
        private static Guid CHECKSUM = Guid.NewGuid();
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
            return (double) stroke.GetPropertyData(STARTINGCHECKSUM);
        }
        public static double startingSum(this Stroke stroke, double startingSum)
        {
            stroke.AddPropertyData(STARTINGCHECKSUM, startingSum);
            return startingSum;
        }
        public static StrokeChecksum sum(this Stroke stroke)
        {//Removed memoization because it makes moved strokes still think they are in the same place.
            var checksum = new StrokeChecksum
            {
                checksum = stroke.StylusPoints.Aggregate(0.0, (acc, item) =>
                    acc + Math.Round(item.X, POINT_DECIMAL_PLACE_ROUNDING) + Math.Round(item.Y, POINT_DECIMAL_PLACE_ROUNDING))
            };
            return checksum;
        }
        public static int POINT_DECIMAL_PLACE_ROUNDING = 1;
    }
    public class Extensions
    {
    }
}
