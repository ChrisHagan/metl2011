using System.Windows;
using System.Windows.Shapes;

namespace MeTLLib.DataTypes
{
    public class LiveWindowSetup
    {
        public LiveWindowSetup(int Slide, string Author, FrameworkElement VisualSource, Rectangle Frame, Point Origin, Point Target, string SnapshotAtTimeOfCreation)
            : this(Slide, Author, Frame, Origin, Target, SnapshotAtTimeOfCreation)
        {
            visualSource = VisualSource;
        }
        public LiveWindowSetup(int Slide, string Author, Rectangle Frame, Point Origin, Point Target, string SnapshotAtTimeOfCreation)
        {
            slide = Slide;
            author = Author;
            frame = Frame;
            Origin = origin;
            Target = target;
            snapshotAtTimeOfCreation = SnapshotAtTimeOfCreation;
        }
        public FrameworkElement visualSource;
        public Rectangle frame;
        public Point origin;
        public Point target;
        public string snapshotAtTimeOfCreation;
        public string author;
        public int slide;
    }
}
