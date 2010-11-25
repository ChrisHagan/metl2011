using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using SandRibbonInterop;
using System.Windows.Media;
using SandRibbon.Components.Sandpit;
//using SandRibbonInterop.MeTLStanzas;
using SandRibbon.Components.Utility;
using System.Windows.Ink;
using Point = System.Windows.Point;
using SandRibbon.Providers;
using SandRibbon.Utils;
using MeTLLib.DataTypes;

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
                    Commands.SetInkCanvasMode.ExecuteAsync(Commands.SetInkCanvasMode.lastValue());
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
            return string.Format("{0}:{1}", Globals.me, DateTimeFactory.Now());
        }
        void ImagesDrop(object sender, DragEventArgs e)
        {
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (fileNames == null)
            {
                MessageBox.Show("Cannot Drop this onto the canvas");
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
                pos.X += image.Width + 30;
                if (image.Height > height) height = image.Height;
                if ((i + 1) % 4 == 0)
                {
                    pos.X = origX;
                    pos.Y += (height + 30);
                    height = 0.0;
                }
                Commands.ImageDropped.ExecuteAsync(new ImageDrop { filename = filename, point = pos, target = target, position = i });
            }
            e.Handled = true;
        }
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