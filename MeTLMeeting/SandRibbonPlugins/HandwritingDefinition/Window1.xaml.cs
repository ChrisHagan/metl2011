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
using System.Windows.Ink;
using System.IO;
using System.Xml.Linq;
using System.Windows.Markup;

namespace HandwritingDefinition
{
    public partial class Window1 : Window
    {
        private string desiredChars = "abcdefghijklmnopqrstuvwxyz";
        private string STROKE_FILE = "strokes.xml";
        private int currentChar = 0;
        public Window1()
        {
            InitializeComponent();
            new XDocument(new XElement("strokes")).Save(STROKE_FILE);
        }
        private void InkCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            var canvas = (InkCanvas) sender;
            if (currentChar >= desiredChars.Length - 1)
            {
                MessageBox.Show("Thanks!");
                Close();
                return;
            }
            saveStrokeAs(desiredChars.Substring(currentChar, 1), canvas);
            currentChar++;
            ((InkCanvas)sender).Strokes.Clear();
            currentCharacter.Text = desiredChars.Substring(currentChar, 1);
        }
        private void saveStrokeAs(string character, InkCanvas canvas)
        {
            var doc = XDocument.Load(STROKE_FILE);
            doc.Root.Add(new XElement("stroke", 
                new XAttribute("meaning", character),
                new XAttribute("content", XamlWriter.Save(canvas))));
            doc.Save(STROKE_FILE);
        }
    }
}
