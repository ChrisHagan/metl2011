
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;
using System.Windows.Automation.Peers;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using SandRibbonObjects;
using SandRibbon.Pages.Collaboration.Layout;
using System.Windows.Controls;

namespace SandRibbon.Components.Utility
{
    public class MeTLTextBox : Xceed.Wpf.Toolkit.RichTextBox
    {
        public double offsetX = 0;
        public double offsetY = 0;
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
        public static IEnumerable<MeTLTextBox> TextChildren(this InkCanvas canvas)
        {
            return canvas.Children.OfType<MeTLTextBox>();
        }
        public static IEnumerable<MeTLImage> ImageChildren(this InkCanvas canvas)
        {
            return canvas.Children.OfType<MeTLImage>();
        }
        public static IEnumerable<MeTLImage> GetSelectedImages(this InkCanvas canvas)
        {
            return canvas.GetSelectedElements().OfType<MeTLImage>();
        }
        public static IEnumerable<MeTLTextBox> GetSelectedTextBoxes(this InkCanvas canvas)
        {
            return canvas.GetSelectedElements().OfType<MeTLTextBox>();
        }
    }
    public static class TextBoxExtensions
    {
        public static bool IsUnder(this Xceed.Wpf.Toolkit.RichTextBox box, Point point)
        {
            var boxOrigin = new Point(System.Windows.Controls.InkCanvas.GetLeft(box), System.Windows.Controls.InkCanvas.GetTop(box));
            var boxSize = new Size(box.ActualWidth, box.ActualHeight);
            var result = new Rect(boxOrigin, boxSize).Contains(point);
            return result;
        }
        public static MeTLTextBox clone(this MeTLTextBox box)
        {
            if (box == null) return null;
            var newBox = new MeTLTextBox();
            newBox.AcceptsReturn = box.AcceptsReturn;            
            newBox.BorderThickness = box.BorderThickness;
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            newBox.Text = box.Text;            
            newBox.FontFamily = box.FontFamily;
            newBox.FontSize = box.FontSize;
            newBox.FontWeight = box.FontWeight;
            newBox.FontStyle = box.FontStyle;
            newBox.Foreground = box.Foreground;
            newBox.Background = box.Background;
            newBox.tag(box.tag());
            newBox.CaretIndex = box.CaretIndex;
            newBox.Width = Double.IsNaN(box.Width) || box.Width <= 0 ? box.ActualWidth : box.Width;
            newBox.Height = Double.IsNaN(box.Height) || box.Height <= 0 ? box.ActualHeight : box.Height;
            newBox.MaxHeight = box.MaxHeight;
            //newBox.SelectedText = box.SelectedText;
            newBox.SelectionLength = box.SelectionLength;
            newBox.SelectionStart = box.SelectionStart;
            InkCanvas.SetLeft(newBox, InkCanvas.GetLeft(box));
            InkCanvas.SetTop(newBox, InkCanvas.GetTop(box));
            newBox.offsetX = box.offsetX;
            newBox.offsetY = box.offsetY;
            return newBox;
        }
        public static MeTLTextBox toMeTLTextBox(this Xceed.Wpf.Toolkit.RichTextBox OldBox)
        {
            var box = new MeTLTextBox(); 
            box.AcceptsReturn = true;
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.tag(OldBox.tag());            
            box.Text = OldBox.Text;
            box.Width = OldBox.Width;
            box.Height = OldBox.Height;
            InkCanvas.SetLeft(box, InkCanvas.GetLeft(OldBox));
            InkCanvas.SetTop(box, InkCanvas.GetTop(OldBox));
            return box;
        }

    }
    public class PrivateAwareStroke: Stroke
    {
        //public Stroke stroke;
        private Pen pen = new Pen();
        private bool shouldShowPrivacy;
        private StreamGeometry geometry;
        private string target;
        private Stroke whiteStroke;
        private bool isPrivate{
            get {return this.tag().privacy == Privacy.Private; }
        }
        public double offsetX = 0;
        public double offsetY = 0;
        public PrivateAwareStroke Clone()
        {
            var pas = new PrivateAwareStroke(base.Clone(), target);
            pas.offsetX = offsetX;
            pas.offsetY = offsetY;
            return pas;
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
            this.target = target; 
            this.tag(stroke.tag());
            shouldShowPrivacy = (this.tag().author == Globals.conversationDetails.Author || Globals.conversationDetails.Permissions.studentCanPublish);
            
            if (!isPrivate) return;

            pen.Freeze();
        }

        public SignedBounds signedBounds() {
            return new SignedBounds {
                username = this.tag().author,
                bounds = this.GetBounds(),
                privacy = this.tag().privacy.ToString()
            };
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (isPrivate && shouldShowPrivacy && target != "notepad")
            {
                if (!base.DrawingAttributes.IsHighlighter)
                {
                    whiteStroke = base.Clone();
                    whiteStroke.DrawingAttributes.Color = Colors.White;
                }
                var wideStroke = this.GetGeometry().GetWidenedPathGeometry(pen).GetFlattenedPathGeometry()
                    .Figures
                    .SelectMany(f => f.Segments.Where(s => s is PolyLineSegment).SelectMany(s=> ((PolyLineSegment)s).Points));
                geometry = new StreamGeometry();
                using(var context = geometry.Open())
                {
                    context.BeginFigure(wideStroke.First(),false , false);
                    for (var i = 0; i < base.StylusPoints.Count; i++)
                        context.LineTo(base.StylusPoints.ElementAt(i).ToPoint(), true, true);
                    context.LineTo(wideStroke.Reverse().First(), false, false);                    
                }
               if (geometry != null && pen != null)
                   drawingContext.DrawGeometry(null, pen, geometry);
               if (whiteStroke != null)
                   base.DrawCore(drawingContext, whiteStroke.DrawingAttributes);
            }
            else
               base.DrawCore(drawingContext, drawingAttributes);
        }
        
    }
    public class MeTLInkCanvas : InkCanvas
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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MeTLInkCanvasAutomationPeer(this);
        }
    }
    
    public class MeTLInkCanvasAutomationPeer : InkCanvasAutomationPeer
    {
        public MeTLInkCanvasAutomationPeer(MeTLInkCanvas owner) : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return "MeTLInkCanvas";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return base.GetAutomationControlTypeCore();
        }
    }
}
