using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using SandRibbon.Connection;
using SandRibbon.Utils.Connection;
using UserControl=System.Windows.Controls.UserControl;
using SandRibbon.Providers;
using SandRibbonObjects;
using SandRibbon.Providers.Structure;
using Microsoft.Practices.Composite.Presentation.Commands;
using TextBox=System.Windows.Controls.TextBox;

namespace SandRibbon.Components
{
    public partial class PowerpointLoader  
    {

        static MsoTriState FALSE = MsoTriState.msoFalse;
        static MsoTriState TRUE = MsoTriState.msoTrue;
        static int resource = 1;
        private static string me;
        public static FrameworkElement DEFAULT_COMMAND_TARGET;
        public JabberWire Wire;
        public DelegateCommand<string> setAuthor;
        private string parsedTitle;
        public PowerpointLoader()
        {
            InitializeComponent();
            setAuthor = new DelegateCommand<string>((author) => me = author);
            Commands.SetIdentity.RegisterCommand(setAuthor);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProviderMonitor.HealthCheck(() => Commands.StartPowerPointLoad.Execute(null, (IInputElement)Commands.ROOT));
        }
        private void startPowerpointLoad(object sender, ExecutedRoutedEventArgs e)
        {
            startPowerpointImport();
        }
        private void startPowerpointImport()
        {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.InitialDirectory = "c:\\";
            fileBrowser.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            fileBrowser.FilterIndex = 2;
            fileBrowser.RestoreDirectory = true;
            var dialogResult = fileBrowser.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                foreach (var file in fileBrowser.FileNames)
                {
                    if (file.Contains(".ppt"))
                        LoadPowerpoint(file);
                }
            }
            else
            {
                Commands.PowerPointLoadFinished.Execute(null);
            }
        }

        public void LoadPowerpoint(string file)
        {
            var powerpointThread = new Thread(() =>
            {
                var ppt = new ApplicationClass().Presentations.Open(file, TRUE, FALSE, FALSE);
                try
                {
                    var provider = (CouchConversationDetailsProvider)ProviderMonitor.GetProvider(typeof(IConversationDetailsProvider));
                    var xml = new XElement("presentation");
                    xml.Add(new XAttribute("name", ppt.Name));
                    if (provider.DetailsOf(ppt.Name).Author != "Happenstance")
                    {
                        System.Windows.MessageBox.Show("This conversation already exists, and might have private content in it.  Please rename your powerpoint file to retry the import.");
                        return;
                    }

                    parsedTitle = ppt.Name.Replace(".pptx", "");
                    Commands.PowerPointProgress.Execute("Starting to parse powerpoint file");
                    foreach (var slide in ppt.Slides)
                    {
                        importSlide(xml,
                                    (Microsoft.Office.Interop.PowerPoint.Slide)
                                    slide);
                    }

                    Commands.PowerPointProgress.Execute( "Finished parsing powerpoint, Beginning data upload");
                    var uploadedXML = uploadXmlUrls(xml);
                    Commands.PowerPointProgress.Execute( "Powerpoint data uploaded to server, Beginning distribution");
                    sendDocument(uploadedXML);
                    Commands.PowerPointProgress.Equals( "Powerpoint distribution finished");
                  
                }
                finally
                {
                    ppt.Close();
                    Commands.PowerPointLoadFinished.Execute(null);
                    
                }
            });
            powerpointThread.SetApartmentState(ApartmentState.STA);
            powerpointThread.Start();
        }

        private XElement uploadXmlUrls(XElement doc)
        {
            var shapeCount = doc.Descendants("shape").Count();
            for (var i = 0; i < shapeCount; i++ )
            {
                var shape = doc.Descendants("shape").ElementAt(i);
                var file = shape.Attribute("snapshot").Value;
                var hostedFileName = ResourceUploader.sendUriToResourceServer(file);
                var uri = new Uri(hostedFileName, UriKind.RelativeOrAbsolute);
                if(shape.Attribute("uri") == null)
                    shape.Add(new XAttribute("uri", uri));
                else
                    shape.Attribute("uri").Value = uri.ToString();
            }
            return doc;
        }
        private static bool check(MsoTriState cond)
        {
            return cond == MsoTriState.msoTrue;
        }
        private static void importSlide(XElement xml, Microsoft.Office.Interop.PowerPoint.Slide slide)
        {
            var xSlide = new XElement("slide");
            xSlide.Add(new XAttribute("index", slide.SlideIndex));

            var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
            if (!Directory.Exists(currentWorkingDirectory))
                Directory.CreateDirectory(currentWorkingDirectory);
            foreach (var shapeObj in slide.Shapes)
            {/*This wacky customer has been hand tuned to get the height and width that PPT renders faithfully at.  
                I have no idea why it has to be this way, it just looks right when it is.*/
                var shape = (Microsoft.Office.Interop.PowerPoint.Shape)shapeObj;
                var file = currentWorkingDirectory + "\\background" + (++resource).ToString() + ".jpg";
                shape.Export(file, PpShapeFormat.ppShapeFormatPNG, 724, 543, PpExportMode.ppRelativeToSlide);
                var x = shape.Left;
                var y = shape.Top;
                xSlide.Add(new XElement("shape",
                    new XAttribute("x", x),
                    new XAttribute("y", y),
                    new XAttribute("snapshot", file)));
            }
            foreach(var notes in slide.NotesPage.Shapes)
            {
                var shape = (Microsoft.Office.Interop.PowerPoint.Shape) notes;
                addText(xSlide, shape); 
            }
            xml.Add(xSlide);
        }
        private ConversationDetails createConversation(string title)
        {
            var provider = (CouchConversationDetailsProvider)ProviderMonitor.GetProvider(typeof(IConversationDetailsProvider));
            var conversation = provider.DetailsOf(parsedTitle);
            if (conversation.Author == "Happenstance")
            {
                var applicationInfo = provider.GetApplicationLevelInformation();
                conversation.Author = me;
                conversation.Tag = "powerpoint";
                conversation.Slides.First().author = me;
                conversation.Slides.First().id = applicationInfo.currentId;
                applicationInfo.currentId += 200;
                provider.UpdateApplicationLevelInformation(applicationInfo);
                provider.Update(conversation);
            }
            return conversation;
        }
        private void sendDocument(XElement doc)
        {
            var conversation = createConversation(doc.Attribute("name").Value);
            if (conversation.Author != me)
            {
                System.Windows.MessageBox.Show("This conversation already exists, and might have private content in it.  Please rename your powerpoint file to retry the import.");
                return;
            }
            var startingId = conversation.Slides.First().id;
            var index = 0;
            conversation.Slides = doc.Descendants("slide").Select(d=>new SandRibbonObjects.Slide { author = me, id = startingId++, index = index++ }).ToList();
            var provider = (CouchConversationDetailsProvider)ProviderMonitor.GetProvider(typeof(IConversationDetailsProvider));
            provider.Update(conversation);
            foreach (var slide in conversation.Slides)
            {
                Wire.SneakInto(slide.id.ToString());
                var conversationName = parsedTitle.Replace(" ", "_"); 
                var privateRoom = conversationName + slide.id.ToString() + me;
                Wire.SneakInto(privateRoom);
            }
            var currentTargetSlide = conversation.Slides.First().id;
            foreach (var slide in doc.Descendants("slide"))
            {
                var shapes = slide.Descendants("shape");
                for (var shapeCount = 0; shapeCount < slide.Descendants("shape").Count(); shapeCount++ )
                {
                    var shape = shapes.ElementAt(shapeCount);
                    var uri = new Uri(shape.Attribute("uri").Value);
                    var hostedImage = new Image();
                    var bitmap = new BitmapImage(uri);
                    hostedImage.Source = bitmap;
                    InkCanvas.SetLeft(hostedImage, Double.Parse(shape.Attribute("x").Value));
                    InkCanvas.SetTop(hostedImage, Double.Parse(shape.Attribute("y").Value));
                    var id = string.Format("{0}:{1}@{2},{3},{4}", "background", currentTargetSlide, shapeCount, me, DateTime.Now);
                    hostedImage.Tag = id;
                    Wire.SendBackground(new Interpreter.ImageDetails
                                                         {
                                                             author = me,
                                                             uri = XamlWriter.Save(hostedImage),
                                                             slide = currentTargetSlide
                                                         }, currentTargetSlide);
                }
                shapes = slide.Descendants("privateText");
                double height = 0;
                for (var shapeCount = 0; shapeCount < slide.Descendants("privateText").Count(); shapeCount++ )
                {
                    Double X = 0;
                    Double Y = height;
                    var text = shapes.ElementAt(shapeCount);
                    var newText = new TextBox();
                    string content = text.Attribute("content").Value;
                    int count = 0;
                    for (int i = 50; i < content.Count(); i += 50)
                    {
                        while (i < content.Count() && content[i] != ' ')
                        {
                            i++;
                        }
                        if (i < content.Count())
                        {
                            count++;
                            content = content.Insert(i, "\\n");
                        }
                    }
                    var font = text.Descendants("font").ElementAt(0);
                    var fontSize = Double.Parse(font.Attribute("size").Value);
                    newText.Text = content;
                    height += (count * fontSize) + fontSize + 10;
                    InkCanvas.SetLeft(newText, X);
                    InkCanvas.SetTop(newText, Y);
                    newText.Tag = DateTime.Now.ToString() + X + Y + me;
                    newText.FontFamily = new FontFamily(font.Attribute("family").Value);
                    //newText.Foreground = new SolidColorBrush(ColorLookup.ColorOf(font.Attribute("color").Value));
                    newText.FontSize = fontSize;
                    Wire.SendPrivateText( XamlWriter.Save(newText) ,me ,parsedTitle, currentTargetSlide.ToString());
                }
                currentTargetSlide++;
            }
            foreach (var room in conversation.Slides.Select(s => s.id))
            {
                Wire.SneakOutOf(room.ToString());
                var conversationName = parsedTitle.Replace(" ", "_"); 
                var privateRoom = conversationName + room.ToString() + me;
                Wire.SneakInto(privateRoom);
            }
            Dispatcher.Invoke((Action)(()=>Commands.JoinConversation.Execute(conversation.Title)));
        }
        private static void addText(XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape)
        {
            var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
            if (check(textFrame.HasText))
            {
                xSlide.Add(new XElement("privateText",
                        new XAttribute("content", textFrame.TextRange.Text.Replace('\v', '\n')),
                        new XAttribute("x", shape.Left),
                        new XAttribute("y", shape.Top),
                        new XElement("font",
                            new XAttribute("family", textFrame.TextRange.Font.Name),
                            new XAttribute("size", textFrame.TextRange.Font.Size),
                            new XAttribute("color", textFrame.TextRange.Font.Color.ToString()))));
            }
        }
    }
}
