using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Windows;

namespace SandRibbonInterop
{
    public struct TextTag
    {
        public string author;
        public string privacy;
        public string id;
    }
    public struct ImageTag
    {
        public string author;
        public string privacy;
        public string id;
        public bool isBackground;
        public int zIndex;
    }
    public struct StrokeTag
    {
        public string author;
        public string privacy;
        public double startingSum;
    }
    public struct StrokeChecksum
    {
        public double checksum;
    }
    public struct UserHighlight
    {
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
        public static ImageTag tag(this MediaElement image )
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
        public static ImageTag tag(this MediaElement image, ImageTag tag)
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
        public static StrokeTag tag(this Stroke stroke)
        {
            return new StrokeTag
                       {
                           author = (string) stroke.GetPropertyData(STROKE_TAG_GUID),
                           privacy = (string) stroke.GetPropertyData(STROKE_PRIVACY_GUID),
            };
        }
        public static StrokeTag tag(this Stroke stroke, StrokeTag tag)
        {
            stroke.AddPropertyData(STROKE_TAG_GUID, tag.author);
            stroke.AddPropertyData(STROKE_PRIVACY_GUID, tag.privacy);
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
    class Extensions
    {
    }
}
