using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SandRibbonObjects
{
    public class Friend : DependencyObject
    {
        public override string ToString()
        {
            return Name;
        }
        public void Flush()
        {
            Count = 0;
        }
        public void Ping()
        {
            Count++;
        }
        DispatcherTimer timer;
        public void Pulse()
        {
            Background = Brushes.Red;
            if (timer == null)
            {
                timer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(300),
                    DispatcherPriority.Normal,
                    (a, b) =>
                    {
                        Background = Brushes.White;
                        timer.Stop();
                        timer = null;
                    },
                    Application.Current.Dispatcher);
            }
            else
            {
                timer.Stop();
                timer.Start();
            }
        }
        public void MoveTo(int where)
        {
            Location = where;
        }
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(Friend), new UIPropertyMetadata("Someone"));
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(Friend), new UIPropertyMetadata(0));
        public int Location
        {
            get { return (int)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(int), typeof(Friend), new UIPropertyMetadata(0));
        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register("ImagePath", typeof(string), typeof(Friend), new UIPropertyMetadata("../Resources/AnonHead.png"));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(Friend), new UIPropertyMetadata(new SolidColorBrush(Colors.Blue)));
    }
}
