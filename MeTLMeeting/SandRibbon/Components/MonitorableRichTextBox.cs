using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SandRibbon.Components
{    
    public class MonitorableRichTextBox : RichTextBox
    {
        public class TextAttributes : DependencyObject
        {
            public static TextAttributes DEFAULT = new TextAttributes { size = 12.0, font = new FontFamily("Arial") };
            public double size { get; set; }
            public FontFamily font { get; set; }
            public FontWeight weight { get; set; }
            public FontStyle style { get; set;}
            public bool underline { get; set; }
            public bool strikethrough { get; set; }
            public SolidColorBrush color { get; set; }
        }
        private int oldCursorPosition;
        public MonitorableRichTextBox()
        {
            KeyUp += new KeyEventHandler(MonitorableRichTextBox_KeyUp);
            SelectionChanged += new RoutedEventHandler(MonitorableRichTextBox_SelectionChanged);
        }
        void MonitorableRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Fire();
        }
        private void MonitorableRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            MaybeFire();
        }
        private void MaybeFire()
        {
            var newCursorPosition = currentCursorPosition();
            if (newCursorPosition != oldCursorPosition)
            {
                Fire();
            }
        }
        private void Fire()
        {
            var attributes = currentAttributes();
            if (attributes != null)
            {
                Commands.NewTextCursorPosition.Execute(attributes);
                oldCursorPosition = currentCursorPosition();
            }
        }
        private int currentCursorPosition()
        {
            return CaretPosition.GetOffsetToPosition(Document.ContentStart);
        }
        private TextAttributes currentAttributes()
        {
            TextRange currentPosition = Selection.IsEmpty ? new TextRange(CaretPosition, CaretPosition) : Selection;
            var fontProperty = currentPosition.GetPropertyValue(Run.FontFamilyProperty);
            var fontWeight = currentPosition.GetPropertyValue(Run.FontWeightProperty);
            var styleProperty = currentPosition.GetPropertyValue(Run.FontStyleProperty);
            var decorationProperty = currentPosition.GetPropertyValue(Inline.TextDecorationsProperty);
            var sizeProperty = currentPosition.GetPropertyValue(Run.FontSizeProperty);
            if (fontProperty == DependencyProperty.UnsetValue || sizeProperty == DependencyProperty.UnsetValue)
                return null;

            return new TextAttributes
            {
                font = (FontFamily)fontProperty,
                weight = (FontWeight)fontWeight,
                style = (FontStyle)styleProperty,
                size = (double)sizeProperty,
                underline = decorationProperty == TextDecorations.Underline,
                strikethrough = decorationProperty == TextDecorations.Strikethrough
            };
        }
    }
}
