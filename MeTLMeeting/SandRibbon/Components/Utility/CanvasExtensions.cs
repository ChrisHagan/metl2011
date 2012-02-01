
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using SandRibbonObjects;

namespace SandRibbon.Components.Utility
{
    public class MeTLTextBox : TextBox
    {
        CommandBinding undoBinding;
        CommandBinding redoBinding;

        public MeTLTextBox()
        {
            UndoLimit = 1;

            undoBinding = new CommandBinding( ApplicationCommands.Undo, UndoExecuted, null);
            redoBinding = new CommandBinding( ApplicationCommands.Redo, RedoExecuted, null);
            CommandBindings.Add(undoBinding);
            CommandBindings.Add(redoBinding);

            CommandManager.AddPreviewCanExecuteHandler(this, canExecute);
        }

        private void canExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Paste)
            {
                e.ContinueRouting = true;
                e.Handled = true;
                e.CanExecute = false;
            }
        }
        private void UndoExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            ApplicationCommands.Undo.Execute(null, Application.Current.MainWindow);
            Commands.Undo.Execute(null);
        }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            ApplicationCommands.Redo.Execute(null, Application.Current.MainWindow);
            Commands.Redo.Execute(null);
        }
    }
    public static class InkCanvasExtensions
    {
        public static IEnumerable<UIElement> TextChildren(this InkCanvas canvas)
        {
            return canvas.Children.ToList().Where(t => t is MeTLTextBox);
        }
        public static IEnumerable<UIElement> ImageChildren(this InkCanvas canvas)
        {
            return canvas.Children.ToList().Where(t => t is Image);
        }
        public static IEnumerable<UIElement> GetSelectedImages(this InkCanvas canvas)
        {
            return canvas.GetSelectedElements().Where(i => i is Image);
        }
        public static IEnumerable<UIElement> GetSelectedTextBoxes(this InkCanvas canvas)
        {
            return canvas.GetSelectedElements().Where(i => i is MeTLTextBox);
        }
    }
    public static class TextBoxExtensions
    {
        public static bool IsUnder(this TextBox box, Point point)
        {
            var boxOrigin = new Point(InkCanvas.GetLeft(box), InkCanvas.GetTop(box));
            var boxSize = new Size(box.ActualWidth, box.ActualHeight);
            var result = new Rect(boxOrigin, boxSize).Contains(point);
            return result;
        }
        public static MeTLTextBox clone(this MeTLTextBox box)
        {
            if (box == null) return null;
            var newBox = new MeTLTextBox();
            newBox.Text = box.Text;
            newBox.TextAlignment = box.TextAlignment;
            newBox.TextDecorations = box.TextDecorations;
            newBox.FontFamily = box.FontFamily;
            newBox.FontSize = box.FontSize;
            newBox.Foreground = box.Foreground;
            newBox.Background = box.Background;
            newBox.tag(box.tag());
            newBox.CaretIndex = box.CaretIndex;
            newBox.Width = box.Width;
            newBox.Height = box.Height;
            newBox.MaxHeight = box.MaxHeight;
            //newBox.SelectedText = box.SelectedText;
            newBox.SelectionLength = box.SelectionLength;
            newBox.SelectionStart = box.SelectionStart;
            InkCanvas.SetLeft(newBox, InkCanvas.GetLeft(box));
            InkCanvas.SetTop(newBox, InkCanvas.GetTop(box));

            return newBox;
        }
        public static MeTLTextBox toMeTLTextBox(this TextBox OldBox)
        {
            var box = new MeTLTextBox(); 
            box.AcceptsReturn = true;
            box.TextWrapping = TextWrapping.WrapWithOverflow;
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.tag(OldBox.tag());
            box.FontFamily = OldBox.FontFamily;
            box.FontStyle = OldBox.FontStyle;
            box.FontWeight = OldBox.FontWeight;
            box.TextDecorations = OldBox.TextDecorations;
            box.FontSize = OldBox.FontSize;
            box.Foreground = OldBox.Foreground;
            box.Text = OldBox.Text;
            box.Width = OldBox.Width;
            //box.Height = OldBox.Height;
            InkCanvas.SetLeft(box, InkCanvas.GetLeft(OldBox));
            InkCanvas.SetTop(box, InkCanvas.GetTop(OldBox));
            return box;
        }

    }
    public class PrivateAwareStroke: Stroke
    {
        private Stroke stroke;
        private Pen pen = new Pen();
        private bool isPrivate;
        private bool shouldShowPrivacy;
        private StreamGeometry geometry;
        private string target;
        private Stroke whiteStroke;
        private StrokeTag tag
        {
            get { return stroke.tag(); }
        }
        private StrokeChecksum sum
        {
            get { return stroke.sum(); }

        }
        public PrivateAwareStroke(Stroke stroke, string target) : base(stroke.StylusPoints, stroke.DrawingAttributes)
        {
            var cs = new[] {55, 0, 0, 0}.Select(i => (byte) i).ToList();
            pen = new Pen(new SolidColorBrush(
                new Color{
                       A=cs[0],
                       R=stroke.DrawingAttributes.Color.R,
                       G=stroke.DrawingAttributes.Color.G,
                       B=stroke.DrawingAttributes.Color.B
                    }), stroke.DrawingAttributes.Width * 4);
            this.stroke = stroke;
            this.target = target; 
            this.tag(stroke.tag());
            isPrivate = this.tag().privacy == "private";
            shouldShowPrivacy = (this.tag().author == Globals.conversationDetails.Author || Globals.conversationDetails.Permissions.studentCanPublish);
            
            if (!isPrivate) return;

            pen.Freeze();
        }
        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (isPrivate && shouldShowPrivacy && target != "notepad")
            {
            
                if (!stroke.DrawingAttributes.IsHighlighter)
                {

                    whiteStroke = stroke.Clone();
                    whiteStroke.DrawingAttributes.Color = Colors.White;
                }
                var wideStroke = this.stroke.GetGeometry().GetWidenedPathGeometry(pen).GetFlattenedPathGeometry()
                    .Figures
                    .SelectMany(f => f.Segments.Where(s => s is PolyLineSegment).SelectMany(s=> ((PolyLineSegment)s).Points));
                geometry = new StreamGeometry();
                using(var context = geometry.Open())
                {
                    context.BeginFigure(wideStroke.First(),false , false);
                    for (var i = 0; i < stroke.StylusPoints.Count; i++)
                        context.LineTo(stroke.StylusPoints.ElementAt(i).ToPoint(), true, true);
                    context.LineTo(wideStroke.Reverse().First(), false, false);
                }
                    drawingContext.DrawGeometry(null, pen, geometry);
            }
            if(whiteStroke != null && isPrivate)
               whiteStroke.Draw(drawingContext);
            else
               base.DrawCore(drawingContext, drawingAttributes);
        }
        
    }
    public class MeTLInkCanvas:InkCanvas
    {
       public MeTLInkCanvas():base()
       {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (sender, args) => HandlePaste(args)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, args) => HandleCopy(args)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (sender, args) => HandleCut(args)));          
       }

        private void HandleCut(ExecutedRoutedEventArgs args)
        {
            Commands.ClipboardManager.Execute(ClipboardAction.Cut);
        }

        private void HandleCopy(ExecutedRoutedEventArgs args)
        {
            Commands.ClipboardManager.Execute(ClipboardAction.Copy);
        }

        private void HandlePaste(ExecutedRoutedEventArgs args)
        {
            Commands.ClipboardManager.Execute(ClipboardAction.Paste);
        }
    }

}
