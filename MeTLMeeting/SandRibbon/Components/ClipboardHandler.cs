using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using System.Windows.Controls;
using System.Windows.Media;
using SandRibbonObjects;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components
{
    public enum ClipboardAction
    {
        Cut,
        Copy,
        Paste
    }

    public interface IClipboardHandler
    {
        bool CanHandleClipboardPaste();
        bool CanHandleClipboardCopy();
        bool CanHandleClipboardCut();

        void OnClipboardPaste();
        void OnClipboardCopy();
        void OnClipboardCut();
    }
    public class ClipboardManager
    {
        class Handler
        {
            public ClipboardAction ActionType;
            public Action ClipboardAction;
            public Func<bool> CanHandle;
        }

        private List<Handler> registeredHandlers = new List<Handler>();

        public void RegisterHandler(ClipboardAction action, Action onClipboardAction, Func<bool> canHandle)
        {
            registeredHandlers.Add(new Handler() { ActionType = action, ClipboardAction = onClipboardAction, CanHandle = canHandle });
        }

        public void ClearAllHandlers()
        {
            registeredHandlers.Clear();
        }

        public void OnClipboardAction(ClipboardAction action)
        {
            foreach (var handler in registeredHandlers.Where((byAction) => byAction.ActionType == action))
            {
                if (handler.CanHandle())
                    handler.ClipboardAction();
            }
        }
    }
    [Serializable()]
    public class MeTLClipboardData
    {
        private IEnumerable<byte[]> _imagesAsBytes;
        public IEnumerable<String> _InkAsString;
        public static string Type = "MeTLClipboardData";        
        public MeTLClipboardData(DataContextRoot rootPage, IEnumerable<string> text, IEnumerable<BitmapSource> images, List<Stroke> ink)
        {            
            Text = text;
            Images = images;
            Ink = ink;
            this.rootPage = rootPage;
        }

        public IEnumerable<String> Text;
        public IEnumerable<BitmapSource> Images
        {
            get { 
                var images = new List<BitmapSource>();
                foreach(var bytes in _imagesAsBytes)
                {
                    var stream = new MemoryStream(bytes);
                    PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource pngSource = decoder.Frames[0];

                    images.Add(pngSource);
                }
                return images;
            }
            set
            {
                var bytes = new List<byte[]>();
                foreach (var image in value)
                {
                    MemoryStream memStream = new MemoryStream();              
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(memStream);
                    bytes.Add(memStream.GetBuffer());
                    
                }
                _imagesAsBytes =bytes;
            }
        }
        
        private string pointsTag = "points";
        private string colorTag = "color";
        private string thicknessTag = "thickness";
        private string highlighterTag = "highlight";
        private string sumTag = "checksum";
        private string startingSumTag = "startingSum";
        public static readonly string privacyTag = "privacy";
        public static readonly string authorTag = "author";
        public static readonly string identityTag = "identity";
        public static readonly string timestampTag = "timestamp";
        private string metl_ns = "{monash:metl}";
        private DataContextRoot rootPage;

        public List<Stroke> Ink
        {
          get
          {
              const double pasteOffset = 20.0;
              var strokes = new List<Stroke>();
              var offsetMatrix = new Matrix { OffsetX = pasteOffset, OffsetY = pasteOffset };
              foreach (var xmlStrokeString in _InkAsString)
              {
                  var XmlStroke = XElement.Parse(xmlStrokeString);
                  var stroke = new Stroke(MeTLStanzas.Ink.stringToPoints(XmlStroke.Element(metl_ns + pointsTag).Value), new DrawingAttributes {Color = MeTLStanzas.Ink.stringToColor(XmlStroke.Element(metl_ns + colorTag).Value)})
                                   {
                                       DrawingAttributes =
                                           {
                                               IsHighlighter = Boolean.Parse(XmlStroke.Element(metl_ns + highlighterTag).Value),
                                               Width = Double.Parse(XmlStroke.Element(metl_ns + thicknessTag).Value),
                                               Height = Double.Parse(XmlStroke.Element(metl_ns + thicknessTag).Value)
                                           }
                                   };
                  // offset what we copied so we can paste the same ink stroke to the canvas
                  stroke.Transform(offsetMatrix, false);
                  stroke.AddPropertyData(stroke.sumId(), Double.Parse(XmlStroke.Element(metl_ns + sumTag).Value ));
                  stroke.AddPropertyData(stroke.startingId(), Double.Parse(XmlStroke.Element(metl_ns + startingSumTag).Value));
                  stroke.AddPropertyData(stroke.startingId(), Double.Parse(XmlStroke.Element(metl_ns + sumTag).Value));
                  var identity = Globals.generateId(rootPage.NetworkController.credentials.name,Guid.NewGuid().ToString());
                  stroke.tag(new StrokeTag(XmlStroke.Element(metl_ns + authorTag).Value, (Privacy)Enum.Parse(typeof(Privacy), XmlStroke.Element(metl_ns + privacyTag).Value, true), identity, XmlStroke.Element(metl_ns + startingSumTag) == null ? stroke.sum().checksum : Double.Parse(XmlStroke.Element(metl_ns + startingSumTag).Value), Boolean.Parse(XmlStroke.Element(metl_ns + highlighterTag).Value), long.Parse(XmlStroke.Element(metl_ns + timestampTag).Value)));
                  strokes.Add(stroke);
              }
              return strokes;
          }
          set
          {
              var data = new List<string>();
              foreach(var stroke in value)
              {
                  var xShift = stroke.StylusPoints.First().X;
                  var yShift = stroke.StylusPoints.First().Y;
                  var newStroke = stroke.Clone();
                  newStroke.tag(new StrokeTag(newStroke.tag().author, newStroke.tag().privacy, newStroke.tag().id, newStroke.sum().checksum, newStroke.tag().isHighlighter,newStroke.tag().timestamp));
                  //this code will translate all strokes to a 0,0 starting point
                  //newStroke.StylusPoints = new StylusPointCollection(stroke.StylusPoints.Select(p => new StylusPoint((p.X - xShift), (p.Y - yShift), p.PressureFactor)));
                  data.Add(new MeTLStanzas.Ink(new TargettedStroke(rootPage.ConversationState.Slide.id, newStroke.tag().author, "PresentationSpace", newStroke.tag().privacy, newStroke.tag().id, newStroke.tag().timestamp, newStroke, newStroke.tag().startingSum)).ToString()); 
              }
              _InkAsString = data;
          }
       }
    }
}
