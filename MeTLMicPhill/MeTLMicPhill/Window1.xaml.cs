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

namespace MeTLMicPhill
{
    public partial class Window1 : Window
    {
        private static int sIndex = 0;
        private static string[] s = new string[] { 
            "Michael",
            "It's me, Michael.",
            "Michael, I'm very disappointed with you.",
            "I thought you were going to be something, Michael.",
            "You promised me you were going to try.",
            "Michael.",
            "Michael, I'd still be alive if it wasn't for you."
        };
        public Window1()
        {
            InitializeComponent();
            WindowState = WindowState.Normal;
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Width = screen.Width;
            Height = screen.Height - 20;
            Top = 0;
            Left = 0;
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
            Width = Width * 0.95;
            Height = Height * 0.95;
             */
            text.Text = string.Format("{0}\n{1}",text.Text,s[sIndex++]);
        }
    }
}
