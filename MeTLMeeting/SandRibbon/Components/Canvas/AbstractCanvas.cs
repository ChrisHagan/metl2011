using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using System.Windows.Media;
using Point = System.Windows.Point;
using SandRibbon.Providers;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.IO;
using System.Xml;

namespace SandRibbon.Components.Canvas
{
    public struct ImageDrop
    {
        public string filename;
        public Point point;
        public string target;
        public int position;
    }
    public class VisibilityInformation  : DependencyObject 
    {
        public string user { get; set;}
        public bool visible 
        {
            get { return (bool)this.GetValue(UserVisibilityProperty); }
            set { this.SetValue(UserVisibilityProperty, value); } 
        }
        public static readonly DependencyProperty UserVisibilityProperty = DependencyProperty.Register(
          "visible", typeof(bool), typeof(VisibilityInformation), new UIPropertyMetadata(true));
    

    }
    public class TagInformation
    {
        public string Author;
        public bool isPrivate;
        public string Id;
    }
    public abstract class AbstractCanvas : InkCanvas
    {
        private static readonly string TMP_PATH = "tmp\\";
        /* Awesomium comment out
        public static void deleteTempFiles(){
            deltree(TMP_PATH);
        }
        // End Awesomium comment out
        */
        public string defaultPrivacy;
        public string actualPrivacy;
        public string target;
        public bool canEdit;
        private int setSlide = -1;
        public int currentSlide
        {
            get
            {
                try
                {
                    return setSlide == -1 ? Globals.slide : setSlide;
                }
                catch (NotSetException e)
                {
                    return -1;
                }
            }
            set
            {
                setSlide = value;
            }

        }

        private string author = "none";
        public string me
        {
            get
            {
                return author == "none" ? Globals.me : author;
            }
            set
            {
                author = value;
            }
        }

        private bool affectedByPrivacy { get { return target == "presentationSpace"; } }
        public string privacy { get { return affectedByPrivacy ? actualPrivacy : defaultPrivacy; } }
        public delegate void ChildrenChangedHandler(DependencyObject visualAdded, DependencyObject visualRemoved);
        public event ChildrenChangedHandler ChildrenChanged;
        public AbstractCanvas()
            : base()
        {
            
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            DragOver += ImageDragOver;
            Drop += ImagesDrop;
            PreviewKeyDown += AbstractCanvas_KeyDown;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (sender, args) =>
                HandlePaste()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, args) =>
                HandleCopy()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (sender, args) =>
                                                                            HandleCut()));
            
            SelectionChanged += new EventHandler(AbstractCanvas_SelectionChanged);
            Loaded += (_sender, _args) => this.Dispatcher.adoptAsync(delegate
            {
                if (target == null)
                {
                    target = (string)FindResource("target");
                    defaultPrivacy = (string)FindResource("defaultPrivacy");
                    actualPrivacy = defaultPrivacy;
                    //context = getContext();
                }
            });
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(moveTo));
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(ClearSelectionOnLayerChanged));
            Commands.DoWithCurrentSelection.RegisterCommand(new DelegateCommand<Action<SelectedIdentity>>(DoWithCurrentSelection));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<object>(ItemsPrivacyChange));

            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(hideAdorners));
        }


        protected static void abosoluteizeElements(List<UIElement> selectedElements)
        {
            foreach (FrameworkElement element in selectedElements)
            {
                if(GetLeft(element) < 0)
                    SetLeft(element, (element is TextBox) ? 0 : 0 + ((element.Width != element.ActualWidth) ? ((element.Width - element.ActualWidth)/2) : 0));
                else
                    SetLeft(element, (element is TextBox) ? GetLeft(element) : GetLeft(element) + ((element.Width != element.ActualWidth) ? ((element.Width - element.ActualWidth)/2) : 0));
                if(GetTop(element) < 0)
                    SetTop(element, (element is TextBox) ? 0 : 0 + ((element.Height != element.ActualHeight) ? ((element.Height - element.ActualHeight)/2) : 0));
                else
                    SetTop(element, (element is TextBox) ? GetTop(element) : GetTop(element) + ((element.Height != element.ActualHeight) ? ((element.Height - element.ActualHeight)/2) : 0));

            }
        }
        private void hideAdorners(object obj)
        {
            ClearAdorners();
        }

        private void moveTo(object obj)
        {
            ClearAdorners();
        }

        void AbstractCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                ClearAdorners();
        }

        private void ItemsPrivacyChange(object obj)
        {
            ClearAdorners();
        }

        protected internal void ClearAdorners()
        {
            Commands.RemovePrivacyAdorners.ExecuteAsync(null);
        }
        private void ClearSelectionOnLayerChanged(string layer)
        {
            if (this.GetSelectedElements().Count > 0 || this.GetSelectedStrokes().Count > 0)
            {
                this.Select(null, null);
                try
                {
                    Commands.SetInkCanvasMode.ExecuteAsync(Commands.SetInkCanvasMode.LastValue());
                }
                catch (Exception) { }
            }
        }
        protected void AbstractCanvas_SelectionChanged(object sender, EventArgs e)
        {
        }
        public abstract void showPrivateContent();
        public abstract void hidePrivateContent();

        public void DoWithCurrentSelection(Action<SelectedIdentity> todo)
        {
            foreach (var stroke in GetSelectedStrokes())
                todo(new SelectedIdentity(stroke.startingSum().ToString(),this.target));
            foreach (var element in GetSelectedElements())
                todo(new SelectedIdentity((string)((FrameworkElement)element).Tag,this.target));
        }
        public Rect GetContentBounds()
        {
            return VisualTreeHelper.GetDescendantBounds(this);
        }
        private void SetPrivacy(string p)
        {
            Dispatcher.adoptAsync(delegate
                                         {
                                             actualPrivacy = p;
                                             try
                                             {
                                                 var canEdit = actualPrivacy == "private" ||
                                                               Globals.conversationDetails.Permissions.studentCanPublish ||
                                                               Globals.conversationDetails.Author == Globals.me;
                                                 SetCanEdit(canEdit);
                                             }
                                             catch (NotSetException e)
                                             {
                                                 //YAY
                                             }
                                         });
        }
        protected void ApplyPrivacyStylingToElement(FrameworkElement element, string privacy)
        {
            if (privacy == "private")
                element.Effect = Effects.privacyShadowEffect;
            else
                element.Effect = null;
        }
        protected void RemovePrivacyStylingFromElement(FrameworkElement element)
        {
            element.Effect = null;
            element.Opacity = 1;
        }
        void ImageDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (fileNames == null) return;
            foreach (string fileName in fileNames)
            {
                FileType type = Image.GetFileType(fileName);
                if (new[] { FileType.Image}.Contains(type))
                    e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        public string generateId()
        {
            return string.Format("{0}:{1}", Globals.me, DateTimeFactory.Now().Ticks);
        }
        /*
        private Byte[] bytesFromDragImageBitsStream(MemoryStream data)
        {
            var buffer = new byte[24];
            data.Read(buffer, 0, 24);
            int w = buffer[0] + (buffer[1] << 8) + (buffer[2] << 16) + (buffer[3] << 24);
            int h = buffer[4] + (buffer[5] << 8) + (buffer[6] << 16) + (buffer[7] << 24);
            // Stride accounts for any padding bytes at the end of each row. For 32 bit
            // bitmaps there are none, so stride = width * size of each pixel in bytes.
            int stride = w * 4;
            // x and y is the relative position between the top left corner of the image and
            // the mouse cursor.
            int x = buffer[8] + (buffer[9] << 8) + (buffer[10] << 16) + (buffer[11] << 24);
            int y = buffer[12] + (buffer[13] << 8) + (buffer[14] << 16) + (buffer[15] << 24);
            buffer = new byte[stride * h];
            // The image is stored upside down, so we flip it as we read it.
            for (int i = (h - 1) * stride; i >= 0; i -= stride) data.Read(buffer, i, stride);
            return buffer;
            //System.Windows.Media.Imaging.BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, buffer, stride);
        }*/

        void ImagesDrop(object sender, DragEventArgs e)
        {
            var tempImagePath = "temporaryDragNDropFileData.bmp";
            bool needsToRemoveTempFile = false;
            var validFormats = e.Data.GetFormats();
            var fileNames = new string[0];
            var allTypes = validFormats.Select(vf => {
                var outputData = "";
                try
                {
                    var rawData = e.Data.GetData(vf);
                    if (rawData is MemoryStream)
                    {
                        outputData = System.Text.Encoding.UTF8.GetString(((MemoryStream)e.Data.GetData(vf)).ToArray());
                    }
                    else if (rawData is String)
                    {
                        outputData = (String)rawData;
                    }
                    else if (rawData is Byte[])
                    {
                        outputData = System.Text.Encoding.UTF8.GetString((Byte[])rawData);
                    }
                    else throw new Exception("data was in an unexpected format: (" + outputData.GetType() + ") - "+outputData);
                }
                catch (Exception ex)
                {
                    outputData = "getData failed with exception (" + ex.Message + ")";
                }
                return vf + ":  "+ outputData;
            }).ToList();
            if (validFormats.Contains(DataFormats.FileDrop))
            {
                //local files will deliver filenames.  
                fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }
            else if (validFormats.Contains("text/html"))
            {
            }
            else if (validFormats.Contains("UniformResourceLocator"))
            {
                //dragged pictures from chrome will deliver the urls of the objects.  Firefox and IE don't drag images.
                var url = (MemoryStream)e.Data.GetData("UniformResourceLocator");
                if (url != null)
                {
                    fileNames = new string[] { System.Text.Encoding.Default.GetString(url.ToArray()) };
                }
            }
             
            if (fileNames.Length == 0)
            {
                MessageBox.Show("Cannot drop this onto the canvas");
                return;
            }
            Commands.SetLayer.ExecuteAsync("Insert");
            var pos = e.GetPosition(this);
            var origX = pos.X;
            //lets try for a 4xN grid
            var height = 0.0;
            for (var i = 0; i < fileNames.Count(); i++)
            {
                var filename = fileNames[i];
                var image = Image.createImageFromUri(new Uri(filename, UriKind.RelativeOrAbsolute));
                Commands.ImageDropped.Execute(new ImageDrop { filename = filename, point = pos, target = target, position = i });
                pos.X += image.Width + 30;
                if (image.Height > height) height = image.Height;
                if ((i + 1) % 4 == 0)
                {
                    pos.X = origX;
                    pos.Y += (height + 30);
                    height = 0.0;
                }
            }
            if (needsToRemoveTempFile)
                File.Delete(tempImagePath);
            e.Handled = true;
        }
        /* Awesomium comment out
        private static void deltree(string TMP_PATH)
        {
            var errorMessage = string.Format("Deltree failed to delete the path {0}", TMP_PATH);
            foreach (var f in Directory.EnumerateFiles(TMP_PATH))
            {
                try
                {
                    File.Delete(f);
                }
                catch (Exception) {
                    Console.WriteLine(errorMessage);
                }
            }
            try
            {
                Directory.Delete(TMP_PATH);
            }
            catch (Exception) { 
                Console.WriteLine(errorMessage);
            }
        }
        // End Awesomium comment out
        */
        public void SetCanEdit(bool canEdit)
        {
            this.canEdit = canEdit;
            AllowDrop = canEdit;
            CanEditChanged();
        }
        protected bool inMeeting()
        {
            try
            {
                return MeTLLib.DataTypes.Permissions.InferredTypeOf(Globals.conversationDetails.Permissions) == MeTLLib.DataTypes.Permissions.MEETING_PERMISSIONS;
            }
            catch (NotSetException)
            {
                return false;
            }
        }
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (ChildrenChanged != null)
                ChildrenChanged(visualAdded, visualRemoved);
        }
        protected abstract void HandlePaste();
        protected abstract void HandleCopy();
        protected abstract void HandleCut();
        protected virtual void CanEditChanged() { }
        /// <summary>
        /// This method is intended to be called ONLY by windows level automation emulating Jabber service.  
        /// Don't use it to pass XML back and forth as strings.  I'll find you.
        /// </summary>
        /// <param name="stream"></param>
        public virtual void ParseInjectedStream(string stream, Action<Element> action)
        {
            var parser = new StreamParser();
            parser.OnStreamElement += new StreamHandler((_sender, node) => action((Element)node));
            parser.Push(Encoding.UTF8.GetBytes(stream), 0, stream.Length);
        }
    }
}