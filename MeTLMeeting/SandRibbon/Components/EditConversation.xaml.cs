using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
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
    }
}
