using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SandRibbon;

namespace ThoughtIdeas
{
    public partial class Thread : UserControl
    {
        public InkCanvas previewCanvas { get; set; }
        public string title { get; set; }

        public Thread(SandRibbon.Utils.Connection.Interpreter.ConversationThread backingObject)
        {
            InitializeComponent();
            DataContext = backingObject;
        }
    }
    public class ThreadAdorner : Adorner
    {
        private Thread adorner;
        private UIElement adornee;
        public double x;
        public double y;
        public string id;
        public bool thread;
        public ThreadAdorner(double x, double y, UIElement adornee, ThoughtIdeas.ThoughtWire.ConversationThread context, InkCanvas threadCanvas, bool isPublic)
            : base(adornee)
        {
            thread = isPublic;
            id = context.slideId;
            this.adorner = new Thread(context);
            if (!isPublic)
            {
                var directory = Directory.GetCurrentDirectory();
                adorner.threadImage.Source = new BitmapImage(new Uri(directory + "\\..\\..\\pushPin.jpg"));
            }
            this.adorner.previewCanvas = threadCanvas;
            this.x = x + 20;
            this.y = y - 20;
            AddVisualChild(adorner);
        }
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            adorner.Arrange(new Rect(new Point(x,y), adorner.DesiredSize));
            return finalSize;
        }
        protected override Visual GetVisualChild(int index)
        {
            return adorner;
        }
    }
}
