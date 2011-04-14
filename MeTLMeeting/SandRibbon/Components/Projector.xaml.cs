using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using MeTLLib;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using System.Collections.Generic;
using SandRibbonInterop;
using SandRibbon.Components.Canvas;
using MeTLLib.DataTypes;

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
        private Rect getContentBounds()
        {
            return new List<AbstractCanvas> { stack.images, stack.text, stack.handwriting }
                .Aggregate(new Rect(), (acc, item) =>
                    {
                        acc.Union(VisualTreeHelper.GetDescendantBounds(item));
                        return acc;
                    });
        }
        
        public Projector()
        {
            InitializeComponent();
            instance = this;
            Loaded += Projector_Loaded;
            stack.SetEditable(false);
            Commands.SetDrawingAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>(SetDrawingAttributes));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<MeTLLib.Providers.Connection.PreParser>(PreParserAvailable));
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<string>(SetInkCanvasMode));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<object>(setLayer));
            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<object>(setPedagogy));
            Commands.LeaveAllRooms.RegisterCommand(new DelegateCommand<object>(shutdown));
            Commands.InternalMoveTo.RegisterCommandToDispatcher(new DelegateCommand<object>(moveTo));
            stack.handwriting.EditingModeChanged += modeChanged;
            stack.images.EditingModeChanged += modeChanged;
            stack.text.EditingModeChanged += modeChanged;
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            conversationLabel.Text = details.Title;
        }

        private void shutdown(object obj)
        {
            if(Window != null)
                Window.Close();
        }

        private void setPedagogy(object obj)
        {
            //when you change pedagogy all the commands are deregistered this will restart the projector
            if(Window != null)
                Window.Close();
        }
        private void moveTo(object obj)
        {
            conversationLabel.Text = Globals.conversationDetails.Title;
            stack.Flush();
        }
        void modeChanged(object sender, RoutedEventArgs e)
        {
            var canvas = (InkCanvas)sender;
            if (canvas.EditingMode != InkCanvasEditingMode.None)
                canvas.EditingMode = InkCanvasEditingMode.None;
        }
        private void Projector_Loaded(object sender, RoutedEventArgs e)
        {
            startProjector(null);
        }
        private void setLayer(object obj)
        {
            Dispatcher.adoptAsync(() =>
                                      {
                                          foreach (var layer in stack.stack.Children)
                                              ((UIElement) layer).Opacity = 1;
                                      });
        }
        private void startProjector(object obj)
        {
            setProjectionLayers();
            try
            {
                ClientFactory.Connection().getHistoryProvider().Retrieve<PreParser>(null, null, PreParserAvailable, Globals.location.currentSlide.ToString());
            }
            catch (Exception e)
            {
            }
            stack.handwriting.me = "projector";
            stack.images.me = "projector";
            stack.text.me = "projector";
            conversationLabel.Text = Globals.conversationDetails.Title;
        }
        private void setProjectionLayers()
        {
            Dispatcher.adoptAsync(delegate
                                 {
                                     var projectorImages = stack.images;
                                     var projectorText = stack.text;
                                     var projectorHandwriting = stack.handwriting;
                                     stack.canvasStack.Children.Clear();
                                     stack.canvasStack.Children.Add(projectorImages);
                                     stack.canvasStack.Children.Add(projectorText);
                                     stack.canvasStack.Children.Add(projectorHandwriting);
                                     foreach (
                                         var canvas in
                                             new List<InkCanvas>
                                                 {stack.handwriting, stack.images, stack.text})
                                     {
                                         canvas.EditingMode = InkCanvasEditingMode.None;
                                         canvas.Opacity = 1;
                                     }
                                 });
        }
        private static Stroke strokeInProgress;
        private static Projector instance;
        private static DrawingAttributes currentAttributes = new DrawingAttributes();
        private static DrawingAttributes deleteAttributes = new DrawingAttributes();
        private static Color deleteColor = Colors.Red;
        private static string currentMode;
        private static string privacy;
        public void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            if (!isPrivate(parser))
            {
                stack.handwriting.ReceiveStrokes(parser.ink);
                stack.images.ReceiveImages(parser.images.Values);
                stack.images.ReceiveVideos(parser.videos.Values);
                foreach (var text in parser.text.Values)
                    stack.text.doText(text);
            }
        }


        private bool isPrivate(MeTLLib.Providers.Connection.PreParser parser)
        {
            if (parser.ink.Where(s => s.privacy == "private").Count() > 0)
                return true;
            if (parser.text.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            if (parser.images.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            if (parser.videos.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            if (parser.autoshapes.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            return false;
        }

        private void SetInkCanvasMode(string mode)
        {
            currentMode = mode;
        }
        private void SetDrawingAttributes(DrawingAttributes attributes)
        {
            currentAttributes = attributes;
            deleteAttributes = currentAttributes.Clone();
            deleteAttributes.Color = deleteColor;
        }
        private void SetPrivacy(string privacy)
        {
            Projector.privacy = privacy;
            //setProjectionLayers();
        }
        /*
        public static void PenMoving(StylusPointCollection points)
        {
            GlobalTimers.resetSyncTimer();
            if (instance == null) return;
            if(privacy == "public")
                if (strokeInProgress == null)
                {
                    bool erasing = currentMode == "EraseByStroke";
                    strokeInProgress = new Stroke(points, erasing? deleteAttributes : currentAttributes);
                    instance.liveInk.Strokes.Add(strokeInProgress);
                }
                else
                    strokeInProgress.StylusPoints.Add(points);
        }
        public static void PenUp()
        {
            if (instance == null) return;
            if(instance.liveInk.Strokes.Contains(strokeInProgress))
                instance.liveInk.Strokes.Remove(strokeInProgress);
            strokeInProgress = null;
        }
         * */
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