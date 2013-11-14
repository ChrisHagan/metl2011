
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Windows.Automation.Peers;

namespace MeTLLib.DataTypes
{
    public class MeTLImage : Image
    {
        public Image internalImage = null;
        public double offsetX = 0;
        public double offsetY = 0;
        public MeTLImage Clone()
        {
            var img = new MeTLImage();
            img.Height = Height;
            img.Width = Width;
            img.Source = Source;
            InkCanvas.SetLeft(img, InkCanvas.GetLeft(this));
            InkCanvas.SetTop(img, InkCanvas.GetTop(this));
            Canvas.SetZIndex(img, Canvas.GetZIndex(this));
            img.Stretch = Stretch.Fill;
            img.tag(this.tag());
            img.offsetX = offsetX;
            img.offsetY = offsetY;
            return img;
        }
    }
}