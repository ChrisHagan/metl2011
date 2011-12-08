using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MeTLLib;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using MeTLLib.Utilities;
using System.Collections.Specialized;
using SandRibbon.Components.Utility;

namespace SandRibbon.Components
{
    public class SelectorExtenders : DependencyObject
    {
        public static bool GetIsAutoscroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAutoscrollProperty);
        }

        public static void SetIsAutoscroll(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAutoscrollProperty, value);
        }

        public static readonly DependencyProperty IsAutoscrollProperty =
            DependencyProperty.RegisterAttached("IsAutoscroll", typeof(bool), typeof(SelectorExtenders), new UIPropertyMetadata(default(bool), OnIsAutoscrollChanged));

        public static void OnIsAutoscrollChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var currentIsAutoscroll = (bool)e.NewValue;
            var element = s as FrameworkElement;
            var container = UIHelper.FindVisualParent<ListBoxItem>(element);
            
            var autoscroller = new DragEventHandler(
                (object sender, DragEventArgs args) => 
                {
                    var imageControl = sender as Image;
                    ScrollViewer scrollViewer = UIHelper.FindVisualParent<ScrollViewer>(imageControl);

                    const double tolerance = 10;
                    const double offset = 3;
                    double horizontalPos = args.GetPosition(imageControl).X;

                    if (horizontalPos < tolerance)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - offset);
                    }
                    else if (horizontalPos > imageControl.ActualHeight - tolerance)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offset);
                    }
                });

            if (currentIsAutoscroll)
                container.DragOver += autoscroller;
            else
                container.DragOver -= autoscroller; 
        }
    }
    /// <summary>
    /// Interaction logic for EditConversation.xaml
    /// </summary>
    /// 
    
    public partial class EditConversation : Window
    {
        public ObservableCollection<Slide> activeSlideList = new ObservableCollection<Slide>();
        public static SlideToThumbConverter SlideToThumb;
        public static SlideIndexConverter SlideIndex;
        public EditConversation()
        {
            SlideToThumb = new SlideToThumbConverter();
            SlideIndex = new SlideIndexConverter(activeSlideList);
            InitializeComponent();
            activeSlides.ItemsSource = activeSlideList;
            loadConversation(Globals.conversationDetails.Slides);
        }

        private void PageList_DragOver(object sender, DragEventArgs e)
        {
            var listBox = sender as ListBox;
            var scrollViewer = UIHelper.FindVisualChild<ScrollViewer>(listBox);

            const double tolerance = 10;
            const double offset = 3;
            double horizontalPos = e.GetPosition(listBox).X;

            if (horizontalPos < tolerance) // left of list
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - offset); // scroll left
            }
            else if (horizontalPos > listBox.ActualWidth - tolerance) // right of visible list
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offset);
            }
        }

        private void loadConversation(List<Slide> slides)
        {
            activeSlideList.Clear();
            foreach (var slide in slides.OrderBy(s => s.index))
            {
                var image = new Image();
                activeSlideList.Add(slide);
                ThumbnailProvider.thumbnail(image, slide.id);
            }
        }
        private void cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void save(object sender, RoutedEventArgs e)
        {
            var details = Globals.conversationDetails;
            foreach (var slide in activeSlideList)
                details.Slides.Where(s => s.id == slide.id).First().index = activeSlideList.IndexOf(slide);
            
            ClientFactory.Connection().UpdateConversationDetails(details);
            Commands.SendNewSlideOrder.Execute(Int32.Parse(details.Jid));
            Close();
        }
        private void first(object sender, RoutedEventArgs e)
        {
            if (activeSlides.SelectedItem != null)
            {
                var item = ((Slide)activeSlides.SelectedItem);
                activeSlideList.Remove(item);
                activeSlideList.Insert(0, item);
                activeSlides.ScrollIntoView(item);
            }

        }
        private void last(object sender, RoutedEventArgs e)
        {
            if (activeSlides.SelectedItem != null)
            {
                var item = ((Slide)activeSlides.SelectedItem);
                activeSlideList.Remove(item);
                activeSlideList.Add(item);
                activeSlides.ScrollIntoView(item);
            }
        }

        /*private void deleteConversation(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            if (MeTLMessage.Question("Really delete this conversation?", owner) == MessageBoxResult.Yes)
            {
                MeTLLib.ClientFactory.Connection().DeleteConversation(Globals.conversationDetails);
                Close();
            }
        }*/
    }
}
