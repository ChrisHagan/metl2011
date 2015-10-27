using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MeTLLib;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SandRibbon.Components
{
    public partial class EditConversation : ServerAwareWindow
    {
        public ObservableCollection<Slide> activeSlideList = new ObservableCollection<Slide>();
        //public SlideIndexConverter SlideIndex;
        //public UrlForSlideConverter UrlForSlide;
        public EditConversation(MetlConfiguration _backend) : base(_backend)
        {
            //SlideIndex = new SlideIndexConverter(activeSlideList);
            //UrlForSlide = new UrlForSlideConverter(backend);
            InitializeComponent();
            activeSlides.ItemsSource = activeSlideList;
            loadConversation(Globals.conversationDetails.Slides);
        }

        private void loadConversation(List<Slide> slides)
        {
            activeSlideList.Clear();
            foreach (var slide in slides.OrderBy(s => s.index))
            {
                activeSlideList.Add(slide);

            }
            activeSlides.UpdateLayout();
            var generator = activeSlides.ItemContainerGenerator;
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

            ServerContext.controller.client.UpdateConversationDetails(details);
            ServerContext.controller.commands.SendNewSlideOrder.Execute(Int32.Parse(details.Jid));
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
    public class UrlForSlideConverter : IValueConverter
    {
        /*
        public MetlConfiguration backend;
        public UrlForSlideConverter(MetlConfiguration _backend)
        {
            backend = _backend;
        }
        */
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var id = value.ToString();
            var backend = parameter as MetlConfiguration;
            var server = App.getContextFor(backend).controller.client.server;
            var host = server.name;
            return new BitmapImage(new Uri(string.Format(server.thumbnailUrl + "{0}/{1}", host, id),UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
