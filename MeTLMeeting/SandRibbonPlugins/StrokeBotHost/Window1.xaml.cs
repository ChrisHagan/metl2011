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
using SandRibbon;
using System.Windows.Ink;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace StrokeBotHost
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Commands.AddMyStroke.RegisterCommand(new DelegateCommand<Stroke>((stroke)=>ink.Strokes.Add(stroke)));
            Commands.LoggedIn.Execute("strokeBotHost");
        }
    }
}
