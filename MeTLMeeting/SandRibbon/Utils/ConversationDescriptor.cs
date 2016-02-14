namespace SandRibbon.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MeTLLib.DataTypes;
    using System.Xml.Linq;

    public class CanvasContentDescriptor
    {
        public double x { get; protected set; }
        public double y { get; protected set; }
        public double w { get; protected set; }
        public double h { get; protected set; }
        public Privacy privacy { get; protected set; }
        public string target { get; protected set; }
        public CanvasContentDescriptor(double X, double Y, double W, double H, Privacy P, string T)
        {
            x = X;
            y = Y;
            w = W;
            h = H;
            privacy = P;
            target = T;
        }
    }

    public class ImageDescriptor : CanvasContentDescriptor
    {
        public byte[] imageBytes { get; protected set; }
        public ImageDescriptor(double X, double Y, double W, double H, Privacy P, string T, byte[] B) : base(X, Y, W, H,P,T)
        {
            imageBytes = B;
        }
    }
    public class TextDescriptor : CanvasContentDescriptor
    {
        public string fontFamily { get; protected set; }
        public double fontSize { get; protected set; }
        public string fontWeight { get; protected set; }
        public string fontDecoration { get; protected set; }
        public string content { get; protected set; }
        public System.Windows.Media.Color fontColor { get; protected set; }
        public TextDescriptor(double X, double Y, double W, double H, Privacy P, string T, string Content, string FF, double FS, string FW, string FD, System.Windows.Media.Color FC) : base(X, Y, W, H,P,T)
        {
            fontFamily = FF;
            fontSize = FS;
            fontWeight = FW;
            fontDecoration = FD;
            content = Content;
            fontColor = FC;
        }

    }

    public class SlideDescriptor
    {
        public int index { get; protected set; }
        public List<ImageDescriptor> images { get; protected set; } = new List<ImageDescriptor>();
        public List<TextDescriptor> texts { get; protected set; } = new List<TextDescriptor>();
        public double defaultWidth { get; protected set; }
        public double defaultHeight { get; protected set; }
        public bool hasPrivateContent
        {
            get { return images.Any(i => i.privacy == Privacy.Private) || texts.Any(t => t.privacy == Privacy.Private); }
        }
        public SlideDescriptor(int i, double dw = 720, double dh = 576)
        {
            index = i;
            defaultWidth = dw;
            defaultHeight = dh;
        }
    }

    public class ConversationDescriptor
    {
        public string name { get; protected set; }
        public readonly List<SlideDescriptor> slides = new List<SlideDescriptor>();
        public bool hasPrivateContent
        {
            get { return slides.Any(s => s.hasPrivateContent); }
        }
        public ConversationDescriptor(string N)
        {
            name = N;
        }
    }
}
