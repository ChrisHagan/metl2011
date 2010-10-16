using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SandRibbon.Providers;
using SandRibbonInterop;
using System.Windows.Shapes;
using MeTLLib.DataTypes;
using Divelements.SandRibbon;

namespace SandRibbon.Components.ResourceDictionaries
{
    partial class TypedControlStylingEventHandlers
    {
        public void OpenDoubleButtonPopup(object sender, RoutedEventArgs e)
        {
            var popup = (Popup)(((FrameworkElement)sender).DataContext);
            popup.IsOpen = true;
        }
        public void Video_Play(object sender, RoutedEventArgs e)
        {
            var srVideo = ((Video)((FrameworkElement)sender).DataContext);
            var newMediaElement = srVideo.MediaElement;
            newMediaElement.LoadedBehavior = MediaState.Manual;
            if (newMediaElement.Clock != null)
            {
                if (newMediaElement.Clock.CurrentTime.HasValue)
                    newMediaElement.Clock.Controller.Seek(newMediaElement.Clock.CurrentTime.Value, System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
                else
                    newMediaElement.Clock.Controller.Seek(new TimeSpan(0, 0, 0), System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
                newMediaElement.Clock.Controller.Resume();
            }
            newMediaElement.UpdateLayout();
            var rectToPush = new Rectangle();
            rectToPush.Fill = new VisualBrush(newMediaElement);
            rectToPush.Stroke = Brushes.Red;
            rectToPush.Height = newMediaElement.ActualHeight;
            rectToPush.Width = newMediaElement.ActualWidth;
            Commands.MirrorVideo.Execute(new MeTLLib.DataTypes.VideoMirror.VideoMirrorInformation(srVideo.tag().id, rectToPush));
            newMediaElement.Play();
        }
        public void Video_Pause(object sender, RoutedEventArgs e)
        {
            var MediaElement = ((MeTLLib.DataTypes.Video)((FrameworkElement)sender).DataContext).MediaElement;
            MediaElement.LoadedBehavior = MediaState.Manual;
            if (MediaElement.Clock != null)
                MediaElement.Clock.Controller.Pause();
            else MediaElement.Pause();
        }
        public void Video_Mute(object sender, RoutedEventArgs e)
        {
            var MediaElement = ((MeTLLib.DataTypes.Video)((FrameworkElement)sender).DataContext).MediaElement;
            MediaElement.LoadedBehavior = MediaState.Manual;
            if (MediaElement.IsMuted)
                MediaElement.IsMuted = false;
            else MediaElement.IsMuted = true;
        }
        public void CreateMediaTimeline(object sender, EventArgs e)
        {
            var srVideo = ((MeTLLib.DataTypes.Video)((FrameworkElement)sender).DataContext);
            var MediaElement = srVideo.MediaElement;
            MediaElement.DataContext = (System.Windows.Controls.Slider)sender;
            MediaElement.MediaOpened += new RoutedEventHandler(MediaElement_MediaOpened);
            MediaElement.LoadedBehavior = MediaState.Manual;
            //MediaElement.Source = SandRibbonInterop.LocalCache.ResourceCache.LocalSource(MediaElement.Source);
            Video_Play(sender, new RoutedEventArgs());
            Video_Pause(sender, new RoutedEventArgs());
        }
        private bool MouseDown = false;
        private bool Updating = true;
        private bool isPaused = false;
        public void sliderMouseDown(object sender, EventArgs e)
        {
            var MediaElement = ((MeTLLib.DataTypes.Video)((FrameworkElement)sender).DataContext).MediaElement;
            Updating = false;
            if (MediaElement.Clock.IsPaused)
                isPaused = true;
        }
        public void sliderMouseUp(object sender, EventArgs e)
        {
            var MediaElement = ((MeTLLib.DataTypes.Video)((FrameworkElement)sender).DataContext).MediaElement;
            isPaused = false;
            var currentPosition = MediaElement.Position;
            Updating = true;
            MediaElement.Clock.Controller.Seek(currentPosition, System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
        }
        public void sliderValueChanged(object sender, RoutedEventArgs e)
        {
            if (!Updating)
            {
                var MediaElement = ((MeTLLib.DataTypes.Video)((FrameworkElement)sender).DataContext).MediaElement;
                var Slider = ((Slider)sender);
                int Seconds = Convert.ToInt32(Slider.Value / 1000);
                int Minutes = Seconds / 60;
                int Hours = Minutes / 60;
                var newPosition = new TimeSpan(Hours, Minutes, Seconds);
                MediaElement.Clock.Controller.Seek(newPosition, System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
            }
        }
        public void MediaElement_MediaOpened(object sender, EventArgs e)
        {
            var MediaElement = ((MediaElement)sender);
            var Slider = ((System.Windows.Controls.Slider)MediaElement.DataContext);
            var srVideo = (MeTLLib.DataTypes.Video)Slider.DataContext;
            MediaTimeline mt = new MediaTimeline((Uri)MediaElement.Source);
            MediaClock mc = mt.CreateClock();
            MediaElement.Clock = mc;
            MediaElement.Clock.CurrentTimeInvalidated += (_sender, _e) =>
                {
                    if (Updating)
                    {
                        if (mc.NaturalDuration.HasTimeSpan)
                            Slider.Maximum = mc.NaturalDuration.TimeSpan.TotalMilliseconds;
                        if (mc.CurrentTime.HasValue)
                            Slider.Value = mc.CurrentTime.Value.TotalMilliseconds;
                    }
                };
            MediaElement.Clock.Controller.Pause();
            MediaElement.UpdateLayout();
            var rectToPush = new Rectangle();
            rectToPush.Fill = new VisualBrush(MediaElement);
            rectToPush.Stroke = Brushes.Red;
            rectToPush.Height = MediaElement.ActualHeight;
            rectToPush.Width = MediaElement.ActualWidth;
            Commands.MirrorVideo.Execute(new MeTLLib.DataTypes.VideoMirror.VideoMirrorInformation(srVideo.tag().id, rectToPush ));
        }
    }
    public class MeTLImageToolTip : System.Windows.Controls.ToolTip
    {
        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("TitleText", typeof(string), typeof(MeTLImageToolTip), new UIPropertyMetadata(""));
        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }
        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register("ContentText", typeof(string), typeof(MeTLImageToolTip), new UIPropertyMetadata(""));
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MeTLImageToolTip), new UIPropertyMetadata(null));
    }
    public class MeTLContentToolTip : System.Windows.Controls.ToolTip
    {
        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("TitleText", typeof(string), typeof(MeTLContentToolTip), new UIPropertyMetadata(""));
        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }
        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register("ContentText", typeof(string), typeof(MeTLContentToolTip), new UIPropertyMetadata(""));
        public UIElement ContentElement
        {
            get { return (UIElement)GetValue(ContentElementProperty); }
            set { SetValue(ContentElementProperty, value); }
        }
        public static readonly DependencyProperty ContentElementProperty =
            DependencyProperty.Register("ContentElement", typeof(UIElement), typeof(MeTLContentToolTip), new UIPropertyMetadata(null));
    }
    public enum ButtonSize
    {
        Never, WhenGroupIsMedium, WhenGroupIsSmall
    }
    public enum InternalButtonSize
    {
        Small, Medium, Large
    }
    public class CheckBox : System.Windows.Controls.CheckBox
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CheckBox), new UIPropertyMetadata("No label"));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CheckBox), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(CheckBox), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(CheckBox), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(CheckBox), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(CheckBox), new UIPropertyMetadata(InternalButtonSize.Large));

        public CheckBox()
            : base()
        {
            this.Loaded += (a, b) =>
            {

            };
        }
    }
    public class RibbonPanel : System.Windows.Controls.WrapPanel
    {
        public RibbonPanel()
            : base()
        {
            this.Orientation = System.Windows.Controls.Orientation.Vertical;
            this.Loaded += (a, b) =>
            {
            };
        }
    }
    public class RadioButton : System.Windows.Controls.RadioButton
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RadioButton), new UIPropertyMetadata("No label"));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(RadioButton), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(RadioButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(RadioButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(RadioButton), new UIPropertyMetadata(InternalButtonSize.Large));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(RadioButton), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public RadioButton()
            : base()
        {
            this.Loaded += (a, b) =>
            {
                var tag = this.Parent != null && this.Parent is FrameworkElement ?
                    ((FrameworkElement)this.Parent).Tag :
                    null;
                if (tag != null && this.GroupName != null)
                    this.GroupName = (string)tag;
            };
        }
    }
    public class Button : System.Windows.Controls.Button
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Button), new UIPropertyMetadata("No label"));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(Button), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(Button), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(Button), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(Button), new UIPropertyMetadata(InternalButtonSize.Large));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(Button), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public Button()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }
    public class DrawerContent
    {
        public System.Windows.Controls.UserControl content { get; set; }
        public string header { get; set; }
        public Visibility visibility { get; set; }
        public int column { get; set; }
    }
    public class NonRibbonButton : System.Windows.Controls.Button
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NonRibbonButton), new UIPropertyMetadata("No label"));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(NonRibbonButton), new UIPropertyMetadata(null));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(NonRibbonButton), new UIPropertyMetadata(InternalButtonSize.Large));

        public NonRibbonButton()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }
    public class DoubleButton : System.Windows.Controls.Button
    {
        public FrameworkElement Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register("Popup", typeof(FrameworkElement), typeof(DoubleButton),
                                        new UIPropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DoubleButton), new UIPropertyMetadata("No label"));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(DoubleButton), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(DoubleButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(DoubleButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(DoubleButton), new UIPropertyMetadata(InternalButtonSize.Large));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(DoubleButton), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public DoubleButton()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }
    public class SlideViewingListBox : System.Windows.Controls.ListBox
    {
        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (Divelements.SandRibbon.RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(Divelements.SandRibbon.RibbonGroupVariant), typeof(SlideViewingListBox), new UIPropertyMetadata(Divelements.SandRibbon.RibbonGroupVariant.Large));

    }
    public class PenColorsListBox : System.Windows.Controls.ListBox
    {
        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (Divelements.SandRibbon.RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(Divelements.SandRibbon.RibbonGroupVariant), typeof(PenColorsListBox), new UIPropertyMetadata(Divelements.SandRibbon.RibbonGroupVariant.Large));

    }
    public class QuizContainer : System.Windows.Controls.ItemsControl
    {
        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (Divelements.SandRibbon.RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(Divelements.SandRibbon.RibbonGroupVariant), typeof(QuizContainer), new UIPropertyMetadata(Divelements.SandRibbon.RibbonGroupVariant.Large));
        public void ScrollToEnd()
        {
            if (Items != null && Items.Count > 0)
            {
                var container = ItemContainerGenerator.ContainerFromItem(Items[Items.Count - 1]);
                if (container != null)
                    ((FrameworkElement)container).BringIntoView();
            }
        }
    }
}
