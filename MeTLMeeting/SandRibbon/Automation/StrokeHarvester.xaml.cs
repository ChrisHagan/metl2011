using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace SandRibbon.Automation
{
    public partial class StrokeHarvester : Window
    {
        string file = "availableStrokes.txt";
        public StrokeHarvester()
        {
            InitializeComponent();
        }
        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var rounding = 1;
            File.AppendAllText(file, e.Stroke.StylusPoints.Aggregate<StylusPoint, string>("",
                (points,point)=>
                    points+string.Format(" {0} {1} {2}", 
                        Math.Round(point.X,rounding), 
                        Math.Round(point.Y,rounding), 
                        Math.Round(point.PressureFactor,rounding))).Trim()+"\n");
        }
    }
}
