using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;
using SandRibbon.Components.Utility;

namespace SandRibbon.Quizzing
{
    public partial class DisplayAQuiz : Window
    {
        public DisplayAQuiz(MeTLLib.DataTypes.QuizQuestion thisQuiz)
        {
            DataContext = thisQuiz;
            InitializeComponent();
        }
    }
}
