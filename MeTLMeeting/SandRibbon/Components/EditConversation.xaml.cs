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
using SimplestDragDrop;

namespace SandRibbon.Components
{
    /// <summary>
    /// Interaction logic for EditConversation.xaml
    /// </summary>
    /// 
    public class ReorderSlideToThumbConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (System.Windows.Controls.Image)values[0];
            var id = ((ReorderSlide)source.DataContext).slide.id;
            ThumbnailProvider.thumbnail(source, id);
            return null;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ReorderSlideIndexConverter : IValueConverter
    {
        private ObservableCollection<ReorderSlide> collection;
        public ReorderSlideIndexConverter(ObservableCollection<ReorderSlide> collection)
        {
            this.collection = collection;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return collection.IndexOf((ReorderSlide)value) + 1;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public partial class EditConversation : Window
    {
        public ObservableCollection<ReorderSlide> activeSlideList = new ObservableCollection<ReorderSlide>();
        public static ReorderSlideToThumbConverter SlideToThumb;
        public static ReorderSlideIndexConverter SlideIndex;
        public EditConversation()
        {
            SlideToThumb = new ReorderSlideToThumbConverter();
            SlideIndex = new ReorderSlideIndexConverter(activeSlideList);
            InitializeComponent();
            activeSlides.ItemsSource = activeSlideList;
            loadConversation(Globals.conversationDetails.Slides);

        }
        private void loadConversation(List<Slide> slides)
        {
            foreach (var slide in slides.OrderBy(s => s.index))
            {
                var image = new Image();
                activeSlideList.Add(new ReorderSlide(image, slide));
                ThumbnailProvider.thumbnail(image, slide.id);

            }
        }
        private void save(object sender, RoutedEventArgs e)
        {
            var details = Globals.conversationDetails;
            foreach (var slide in activeSlideList)
                details.Slides.Where(s => s.id == slide.slide.id).First().index = activeSlideList.IndexOf(slide);
            ClientFactory.Connection().UpdateConversationDetails(details);
            Close();
        }
        private void first(object sender, RoutedEventArgs e)
        {
            if (activeSlides.SelectedItem != null)
            {
                var item = ((ReorderSlide)activeSlides.SelectedItem);
                activeSlideList.Remove(item);
                activeSlideList.Insert(0, item);
            }

        }
        private void last(object sender, RoutedEventArgs e)
        {
            if (activeSlides.SelectedItem != null)
            {
                var item = ((ReorderSlide)activeSlides.SelectedItem);
                activeSlideList.Remove(item);
                activeSlideList.Add(item);
            }
        }
    }

	public class ReorderSlide
	{
		public Image Location { get; set; }
        public Slide slide { get; set; }

		public ReorderSlide(Image location, Slide slide)
		{
			this.Location = location;
            this.slide = slide;
		}
	}
}
