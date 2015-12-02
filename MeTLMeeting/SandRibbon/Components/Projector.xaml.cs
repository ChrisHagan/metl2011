using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Media;
using MeTLLib;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using System.Collections.Generic;
using MeTLLib.DataTypes;
using SandRibbon.Pages;

namespace SandRibbon.Components
{
    public partial class Projector : UserControl
    {
        public static WidthCorrector WidthCorrector = new WidthCorrector();
        public static HeightCorrector HeightCorrector = new HeightCorrector();
        public ScrollViewer viewConstraint
        {
            set 
            {
                DataContext = value;
                value.ScrollChanged += new ScrollChangedEventHandler(value_ScrollChanged);
            }
        }
        private static Window windowProperty;
        public static Window Window
        {
            get { return windowProperty; }
            set 
            {
                windowProperty = value;
                value.Closed += (_sender, _args) =>
                {
                    windowProperty = null;
                    Commands.RequerySuggested(Commands.MirrorPresentationSpace);
                };
            }
        }
        private void value_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scroll.ScrollToHorizontalOffset(e.HorizontalOffset);
            scroll.ScrollToVerticalOffset(e.VerticalOffset);
        }
        public SlideAwarePage rootPage { get; protected set; }
        public Projector(SlideAwarePage _rootPage)
        {
            rootPage = _rootPage;
            InitializeComponent();
            var setDrawingAttributesCommand = new DelegateCommand<DrawingAttributes>(SetDrawingAttributes);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdateConversationDetails);
            Loaded += (s, e) =>
            {
                conversationLabel.Text = generateTitle(rootPage.ConversationDetails);
                stack.me = "projector";
                stack.Work.EditingMode = InkCanvasEditingMode.None;
                rootPage.NetworkController.client.historyProvider.Retrieve<PreParser>(null, null, PreParserAvailable, rootPage.Slide.id.ToString());
                Commands.SetDrawingAttributes.RegisterCommand(setDrawingAttributesCommand);
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
            };
            Unloaded += (s, e) =>
            {
                Commands.SetDrawingAttributes.RegisterCommand(setDrawingAttributesCommand);
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
            };
        }
        private string generateTitle(ConversationDetails details)
        {
            return string.Format("{0} Page:{1}", details.Title, rootPage.Slide.index);
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            conversationLabel.Text = generateTitle(details);
        }

        private static DrawingAttributes currentAttributes = new DrawingAttributes();
        private static DrawingAttributes deleteAttributes = new DrawingAttributes();
        private static Color deleteColor = Colors.Red;
        public void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            //if (!isPrivate(parser))
            //{
            BeginInit();
                stack.ReceiveStrokes(parser.ink);
                stack.ReceiveImages(parser.images.Values);
                foreach (var text in parser.text.Values)
                    stack.DoText(text);
                stack.RefreshCanvas();
                /*foreach (var moveDelta in parser.moveDeltas)
                    stack.ReceiveMoveDelta(moveDelta, processHistory: true);*/
                EndInit();
           //}
        }

        private bool isPrivate(MeTLLib.Providers.Connection.PreParser parser)
        {
            if (parser.ink.Where(s => s.privacy == Privacy.Private).Count() > 0)
                return true;
            if (parser.text.Where(s => s.Value.privacy == Privacy.Private).Count() > 0)
                return true;
            if (parser.images.Where(s => s.Value.privacy == Privacy.Private).Count() > 0)
                return true;
            return false;
        }

        private void SetDrawingAttributes(DrawingAttributes attributes)
        {
            currentAttributes = attributes;
            deleteAttributes = currentAttributes.Clone();
            deleteAttributes.Color = deleteColor;
        }
    }
    public class WidthCorrector : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var sourceWidth = (double)values[0];
            var sourceHeight = (double)values[1];
            var targetWidth = (double)values[2];
            var targetHeight = (double)values[3];
            var sourceAspect = sourceWidth / sourceHeight;
            var destinationAspect = targetWidth / targetHeight;
            if(Math.Abs(destinationAspect - sourceAspect) < 0.01) return sourceWidth;
            if (destinationAspect < sourceAspect) return sourceWidth;
            return sourceHeight * destinationAspect;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class HeightCorrector : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var sourceWidth = (double)values[0];
            var sourceHeight = (double)values[1];
            var targetWidth = (double)values[2];
            var targetHeight = (double)values[3];
            var sourceAspect = sourceWidth / sourceHeight;
            var destinationAspect = targetWidth / targetHeight;
            if(Math.Abs(destinationAspect - sourceAspect) < 0.01) return sourceHeight;
            if (destinationAspect > sourceAspect) return sourceHeight;
            return sourceWidth / destinationAspect;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}