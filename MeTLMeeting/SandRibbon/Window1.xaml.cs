using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using sr = Divelements.SandRibbon;
using System.ComponentModel;

namespace SandRibbon
{
    public partial class Window1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private TextAttributes currentAttributesProperty;
        public TextAttributes currentAttributes{
            get{
                return currentAttributesProperty;
            }
            set{
                currentAttributesProperty = value;
                var focussedElement = FocusManager.GetFocusedElement(this);
                if (focussedElement != null && focussedElement is RichTextBox)
                {
                    var rtb = (RichTextBox)focussedElement;
                    var selection = rtb.Selection;
                    if (!selection.IsEmpty)
                    {
                        selection.ApplyPropertyValue(Run.FontSizeProperty, value.size);
                        selection.ApplyPropertyValue(Run.FontFamilyProperty, value.font);
                    }
                    else
                    {
                    }
                }
                if(PropertyChanged != null){
                    PropertyChanged(this, new PropertyChangedEventArgs("currentAttributes"));
                }
            }
        }
        public Window1()
        {
            InitializeComponent();
            loadQuickPens();
            loadText();
            loadSlides();
        }
        private void loadQuickPens()
        {
            foreach (var colorName in new[] { "Red", "Blue", "Green" })
            {
                var button = new sr.Button
                {
                    Text = colorName,
                    Background = new SolidColorBrush(ColorsAvailable.ColorOf(colorName))
                };
                button.Command = Commands.SetPenColor;
                button.CommandParameter = colorName;
                pencilCase.Items.Add(button);
            }
        }
        private void loadText()
        {
            fontSize.ItemsSource = new[] { 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0 };
            fontSize.SelectionChanged += new SelectionChangedEventHandler(fontSize_SelectionChanged);
        }
        void fontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentAttributes.size = (double)e.AddedItems[0];
        }
        private void loadSlides()
        {
            for(int i = 0; i < 100; i++)
                slides.Items.Add(new Divelements.SandRibbon.GalleryButton());
        }
        private void canChangePenColor(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canvas.EditingMode == InkCanvasEditingMode.Ink;
        }
        private void changePenColor(object sender, ExecutedRoutedEventArgs e)
        {
            setPenColor(ColorsAvailable.ColorOf((string)e.Parameter));
        }
        private void RibbonGallery_DrawButton(object sender, sr.DrawGalleryButtonEventArgs e)
        {
            Pen pen = new Pen(Brushes.Black, 1.5);
            e.DrawingContext.DrawEllipse(Brushes.Orange, pen,
                new Point(e.Bounds.X + e.Bounds.Width / 2, e.Bounds.Y +  e.Bounds.Height / 2),
                e.Bounds.Width / 2 - 6, e.Bounds.Height / 2 - 6);
        }
        private void colors_ColorPicked(object sender, Divelements.SandRibbon.ColorEventArgs e)
        {
            setPenColor(e.Color);    
        }
        private void setPenColor(Color color){
            canvas.DefaultDrawingAttributes.Color = color;
        }
        private void alwaysTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void addTextBoxOnNextClick(object sender, ExecutedRoutedEventArgs e)
        {
            MouseButtonEventHandler handler = null;
            handler = new MouseButtonEventHandler((_sender,addArgs)=>{
                canvas.MouseLeftButtonUp -= handler;
                var rtb = new MonitorableRichTextBox();
                rtb.Width = 100;
                rtb.Height = 100;
                InkCanvas.SetLeft(rtb, addArgs.GetPosition(canvas).X);
                InkCanvas.SetTop(rtb, addArgs.GetPosition(canvas).Y);
                canvas.EditingMode = InkCanvasEditingMode.None;
                rtb.LostFocus += new RoutedEventHandler((_s,_a)=>canvas.EditingMode = InkCanvasEditingMode.Ink);
                rtb.CaretMoved += new MonitorableRichTextBox.AttributesAtCurrentCaretPosition((attributes)=> currentAttributes = attributes);
                canvas.Children.Add(rtb);
            });
            canvas.MouseLeftButtonUp += handler;
        }
        private void fontSizeSelected(object sender, SelectionChangedEventArgs e)
        {
            currentAttributes = new TextAttributes{font = currentAttributes.font, size = (double)e.AddedItems[0]};
        }
    }
    public class TextAttributes : DependencyObject
    {
        public double size{get;set;}
        public FontFamily font{get;set;}
    }
    public class MonitorableRichTextBox : RichTextBox
    {
        public delegate void AttributesAtCurrentCaretPosition(TextAttributes attributes);
        public event AttributesAtCurrentCaretPosition CaretMoved;
        private int oldCursorPosition;
        public MonitorableRichTextBox()
        {
            KeyUp += new KeyEventHandler(MonitorableRichTextBox_KeyUp);
            SelectionChanged += new RoutedEventHandler(MonitorableRichTextBox_SelectionChanged);
        }
        void MonitorableRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            CaretMoved(currentAttributes());
        }
        private void MonitorableRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            MaybeFire();
        }
        private void MaybeFire()
        {
            var newCursorPosition = currentCursorPosition();
            if ( newCursorPosition != oldCursorPosition)
            {
                CaretMoved(currentAttributes());
                oldCursorPosition = newCursorPosition;
            }
        }
        private int currentCursorPosition()
        {
            return CaretPosition.GetOffsetToPosition(Document.ContentStart);
        }
        private TextAttributes currentAttributes()
        {
            var caret = new TextRange(CaretPosition, CaretPosition);
            return new TextAttributes {
                font = (FontFamily)caret.GetPropertyValue(Run.FontFamilyProperty),
                size = (double)caret.GetPropertyValue(Run.FontSizeProperty)
            };
        }
    }
}