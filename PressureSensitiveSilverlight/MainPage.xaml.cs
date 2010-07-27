using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Ink;
using System.Windows.Browser;
using System.Collections.ObjectModel;
using System.Threading;
using System.Xml;
using System.IO;

namespace SilverlightApplication1
{
    [ScriptableType]
    public partial class MainPage : UserControl
    {
        private Color[] colors = new Color[] { Colors.Black, 
                Colors.White, Colors.Red, Colors.Blue, Colors.Green,
                Colors.Yellow, Colors.Gray};
        public string me;
        public MainPage()
        {
            InitializeComponent();
            setupColourPicker();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        [ScriptableMember]
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (WaveManager.Wave.IsInWaveContainer())
            {
                me = WaveManager.Wave.GetViewer().Id;
                WaveManager.Wave.StateUpdated = StateUpdated;
                WaveManager.Wave.ParticipantsUpdated = ParticipantsUpdated;
                HtmlPage.RegisterScriptableObject("SilverWave", WaveManager.Wave);
                WaveControls.Visibility = Visibility.Visible;
            }
        }
        private string StrokeToString(Stroke stroke)
        {
            var strokestring = "<stroke>";
            strokestring += "<color>" + stroke.DrawingAttributes.Color.ToString() + "</color>";
            strokestring += "<size>" + stroke.DrawingAttributes.Height.ToString() + "</size>";
            strokestring += "<creator>" + me + "</creator>";
            strokestring += "<points>";
            foreach (StylusPoint sp in stroke.StylusPoints)
            {
                strokestring += sp.X + "," + sp.Y + "," + sp.PressureFactor + " ";
            }
            strokestring.TrimEnd(new char[] { ' ' });
            strokestring += "</points>";
            strokestring += "</stroke>";
            return strokestring;
        }
        private Stroke StringToStroke(string strokestring)
        {
            var PointCollection = new StylusPointCollection();
            var strokeDrawingAttributes = new DrawingAttributes();
            var reader = XmlReader.Create(new StringReader(strokestring));
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "points":
                            reader.Read();
                            if (reader.NodeType == XmlNodeType.Text)
                            {
                                if (reader.HasValue)
                                {
                                    var pointCollectionString = reader.Value.Trim();
                                    string[] points = pointCollectionString.Split(' ');
                                    foreach (string point in points)
                                    {
                                        string[] members = point.Split(',');
                                        var sp = new StylusPoint { X = double.Parse(members[0]), Y = double.Parse(members[1]), PressureFactor = float.Parse(members[2]) };
                                        PointCollection.Add(sp);
                                    }
                                }
                            }
                            break;
                        case "creator":
                            break;
                        case "size":
                            reader.Read();
                            if (reader.NodeType == XmlNodeType.Text)
                            {
                                if (reader.HasValue)
                                {
                                    var sizeValue = reader.Value;
                                    var size = double.Parse(sizeValue);
                                    strokeDrawingAttributes.Height = size;
                                    strokeDrawingAttributes.Width = size;
                                }
                            } break;
                        case "color":
                            reader.Read();
                            if (reader.NodeType == XmlNodeType.Text)
                            {
                                if (reader.HasValue)
                                {
                                    var colorValue = reader.Value;
                                    var color = new Color
                                    {
                                        A = (byte)(Convert.ToUInt32(colorValue.Substring(1, 2), 16)),
                                        R = (byte)(Convert.ToUInt32(colorValue.Substring(3, 2), 16)),
                                        G = (byte)(Convert.ToUInt32(colorValue.Substring(5, 2), 16)),
                                        B = (byte)(Convert.ToUInt32(colorValue.Substring(7, 2), 16))
                                    };
                                    strokeDrawingAttributes.Color = color;
                                }
                            }
                            break;
                        case "stroke":
                            break;
                    }
                }
            }
            var stroke = new Stroke(PointCollection);
            stroke.DrawingAttributes = strokeDrawingAttributes;
            return stroke;
        }
        private int stateUpdateCount = 0;
        private void StateUpdated(object sender, EventArgs e)
        {
            Dictionary<string, string> dict = WaveManager.Wave.State.Get();
            StrokeCollection strokesToAdd = new StrokeCollection();
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                stateUpdateCount++;
                if (kvp.Value.StartsWith("<stroke>"))
                {
                    string[] strokestrings = kvp.Value.Replace("</stroke><stroke>", "</stroke>|<stroke>").Split('|');
                    int strokeCount = 0;
                    foreach (string strokestring in strokestrings)
                    {
                        strokeCount++;
                        strokesToAdd.Add(StringToStroke(strokestring));
                    }
                }
            }
            if (strokesToAdd.Count > 0)
            {
                inkcanvas.Strokes.Clear();
                foreach (Stroke stroke in strokesToAdd)
                    inkcanvas.Strokes.Add(stroke);
            }
            stateIndicator.Text = "s-upd: " + stateUpdateCount;
        }
        private void showState(object sender, RoutedEventArgs e)
        {
            if (WaveManager.Wave.IsInWaveContainer())
            {
                Dictionary<string, string> dict = WaveManager.Wave.State.Get();
                string stringprep = "State Represented by: ";
                int index = 0;
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    stringprep += "item[" + (index++) + "](K:" + kvp.Key + ",V:" + kvp.Value + ") ";
                }
                MessageBox.Show(stringprep);
            }
        }
        private int participantsUpdateCount = 0;
        private void ParticipantsUpdated(object sender, EventArgs e)
        {
            participantsUpdateCount++;
            ObservableCollection<Participant> participants = WaveManager.Wave.GetParticipants();
            string stringprep = "Participants in current wave: ";
            int index = 0;
            foreach (Participant participant in participants)
            {
                stringprep += "item[" + (index++) + "](Id:" + participant.Id +
                    "Name:" + participant.DisplayName +
                    "Thumb:" + participant.ThumbnailUrl + ") ";
            }
            participantsIndicator.Text = "p-upd: " + participantsUpdateCount + ", value: " + stringprep;
        }
        private void setupColourPicker()
        {
            foreach (Color color in colors)
            {
                ColourPicker.Items.Add(new SolidColorBrush(color));
            }
        }
        private void Erase(object sender, RoutedEventArgs e)
        {
            inkcanvas.activeEditingMode = InkCanvas.inkCanvasModes.Erase;
        }
        private void Select(object sender, RoutedEventArgs e)
        {
            inkcanvas.activeEditingMode = InkCanvas.inkCanvasModes.Select;
        }
        private void ChangeColour(object sender, RoutedEventArgs e)
        {
            if (inkcanvas != null)
            {
                var colour = ((Button)sender).Background;
                var itemNumber = ColourPicker.Items.IndexOf(colour);
                var currentColor = colors[itemNumber];
                currentColor.A = inkcanvas.defaultDrawingAttributes.Color.A;
                inkcanvas.defaultDrawingAttributes.Color = currentColor;
                inkcanvas.activeEditingMode = InkCanvas.inkCanvasModes.Draw;
            }
        }
        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkcanvas != null)
            {
                var currentAlpha = Convert.ToByte(e.NewValue);
                var newColor = inkcanvas.defaultDrawingAttributes.Color;
                newColor.A = currentAlpha;
                inkcanvas.defaultDrawingAttributes.Color = newColor;
            }
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkcanvas != null)
            {
                inkcanvas.defaultDrawingAttributes.Height = e.NewValue;
                inkcanvas.defaultDrawingAttributes.Width = e.NewValue;
            }
        }
        private void inkcanvas_strokesReplaced(object sender, StrokesChangedEventArgs e)
        {
            if (WaveManager.Wave.IsInWaveContainer())
            {
                string stringStroke = "";
                List<string> removedStrokes = new List<string> { };
                foreach (Stroke removedStroke in e.removedStrokes)
                {
                    var strokeToRemove = StrokeToString(removedStroke);
                    removedStrokes.Add(strokeToRemove);
                }
                Dictionary<string, string> dict = WaveManager.Wave.State.Get();
                foreach (KeyValuePair<string, string> kvp in dict.Where(pair => pair.Key.ToString().Equals("SLid")))
                {
                    if (!removedStrokes.Contains(kvp.Value))
                        stringStroke += kvp.Value;
                }
                foreach (Stroke addedStroke in e.addedStrokes)
                    stringStroke += StrokeToString(addedStroke);
                WaveManager.Wave.State.SubmitDelta("count", stringStroke);
            }
        }
        private void inkcanvas_strokeCollected(object sender, StrokeAddedEventArgs e)
        {
            if (WaveManager.Wave.IsInWaveContainer())
            {
                var stringStroke = StrokeToString(e.stroke);
                Dictionary<string, string> dict = WaveManager.Wave.State.Get();
                foreach (KeyValuePair<string, string> kvp in dict.Where(pair => pair.Key.ToString().Equals("SLid")))
                {
                    stringStroke += kvp.Value;
                }
                WaveManager.Wave.State.SubmitDelta("stroke", stringStroke);
            }
        }
        private void inkcanvas_selectedStrokesChanged(object sender, StrokesChangedEventArgs e)
        {
            /*   if (e.addedStrokes == null) return;
               string stringMessage = "strokes collected:";
               foreach (Stroke stroke in e.addedStrokes)
                   {
                   stringMessage += " stroke:
                   foreach (StylusPoint sp in stroke.StylusPoints)
                       stringMessage += "(" + sp.X + "," + sp.Y + "," + sp.PressureFactor + "),";
               }
               MessageBox.Show(stringMessage);
           */
        }
    }
}
