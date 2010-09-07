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
            var newMediaElement = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext).MediaElement;
            newMediaElement.LoadedBehavior = MediaState.Manual;
            if (newMediaElement.Clock != null)
            {
                if (newMediaElement.Clock.CurrentTime.HasValue)
                    newMediaElement.Clock.Controller.Seek(newMediaElement.Clock.CurrentTime.Value, System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
                else
                    newMediaElement.Clock.Controller.Seek(new TimeSpan(0, 0, 0), System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
                newMediaElement.Clock.Controller.Resume();
            }
            newMediaElement.Play();
        }
        public void Video_Pause(object sender, RoutedEventArgs e)
        {
            var MediaElement = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext).MediaElement;
            MediaElement.LoadedBehavior = MediaState.Manual;
            if (MediaElement.Clock != null)
                MediaElement.Clock.Controller.Pause();
            else MediaElement.Pause();
        }
        public void Video_Mute(object sender, RoutedEventArgs e)
        {
            var MediaElement = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext).MediaElement;
            MediaElement.LoadedBehavior = MediaState.Manual;
            if (MediaElement.IsMuted)
                MediaElement.IsMuted = false;
            else MediaElement.IsMuted = true;
        }
        public void CreateMediaTimeline(object sender, EventArgs e)
        {
            var srVideo = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext);
            var MediaElement = srVideo.MediaElement;
            MediaElement.DataContext = (System.Windows.Controls.Slider)sender;
            MediaElement.MediaOpened += new RoutedEventHandler(MediaElement_MediaOpened);
            MediaElement.LoadedBehavior = MediaState.Manual;
            MediaElement.Source = SandRibbonInterop.LocalCache.MediaElementCache.LocalSource(MediaElement.Source);
            Video_Play(sender, new RoutedEventArgs());
            Video_Pause(sender, new RoutedEventArgs());
        }
        private bool MouseDown = false;
        private bool Updating = true;
        private bool isPaused = false;
        public void sliderMouseDown(object sender, EventArgs e)
        {
            var MediaElement = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext).MediaElement;
            Updating = false;
            if (MediaElement.Clock.IsPaused)
                isPaused = true;
        }
        public void sliderMouseUp(object sender, EventArgs e)
        {
            var MediaElement = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext).MediaElement;
            isPaused = false;
            var currentPosition = MediaElement.Position;
            Updating = true;
            MediaElement.Clock.Controller.Seek(currentPosition, System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
        }
        public void sliderValueChanged(object sender, RoutedEventArgs e)
        {
            if (!Updating)
            {
                var MediaElement = ((SandRibbonInterop.Video)((FrameworkElement)sender).DataContext).MediaElement;
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
            var srVideo = (SandRibbonInterop.Video)Slider.DataContext;
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
            Commands.MirrorVideo.Execute(new SandRibbonInterop.VideoMirror.VideoMirrorInformation { id = srVideo.tag().id, rect = rectToPush });
        }
    }
}
