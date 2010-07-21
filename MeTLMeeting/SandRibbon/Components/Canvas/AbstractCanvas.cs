using System;
using System.Collections.Generic;
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
using SandRibbonInterop.MeTLStanzas;
using SandRibbon.Components.Utility;
using System.Windows.Ink;
using Point=System.Windows.Point;
using SandRibbon.Providers;

namespace SandRibbon.Components.Canvas
{
    public struct ImageDrop
    {
        public string filename;
        public Point point;
        public string target;
        public int position;
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
            get {
                return setSlide == -1 ? Globals.slide : setSlide;
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
        public string privacy{get{return affectedByPrivacy?actualPrivacy:defaultPrivacy;}}
        public delegate void ChildrenChangedHandler(DependencyObject visualAdded, DependencyObject visualRemoved);
        public event ChildrenChangedHandler ChildrenChanged;
        public AbstractCanvas():base()
        {
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            DragOver+=ImageDragOver;
            Drop+=ImagesDrop;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (sender,args)=> 
                HandlePaste()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender,args)=> 
                HandleCopy()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (sender, args) =>
                                                                            HandleCut()));
            Loaded += (_sender, _args) => this.Dispatcher.adoptAsync(delegate{
              if(target == null)
                {
                    target = (string) FindResource("target");
                    defaultPrivacy = (string) FindResource("defaultPrivacy");
                    actualPrivacy = defaultPrivacy;
                    context = getContext();
                }
            });
            Commands.DoWithCurrentSelection.RegisterCommand(new DelegateCommand<Action<SelectedIdentity>>(DoWithCurrentSelection));
        }
        public abstract void showPrivateContent();
        public abstract void hidePrivateContent();

        public void DoWithCurrentSelection(Action<SelectedIdentity> todo)
        {
            foreach (var stroke in GetSelectedStrokes())
                todo(new SelectedIdentity{
                    id=stroke.startingSum().ToString(),
                    target = this.target
                });
            foreach (var element in GetSelectedElements())
                todo(new SelectedIdentity {
                    id=(string)((FrameworkElement)element).Tag,
                    target = this.target
                });
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
                                             catch(NotSetException e)
                                             {
                                                 //YAY
                                             }
                                         });
        }
        private PresentationSpace context;
        protected void addPrivateRegion(IEnumerable<Point> figure)
        {
            if (context == null) return;
            context.AddPrivateRegion(figure);
        }
        protected void removePrivateRegion(IEnumerable<Point> figure)
        {
            if (context == null) return;
            context.RemovePrivateRegion(figure);
        }
        private PresentationSpace getContext()
        {
            DependencyObject parent = this;
            while(!(parent is PresentationSpace) && parent != null)
                parent = LogicalTreeHelper.GetParent(parent);
            return (PresentationSpace)parent;
        }
        protected void ClearAdorners()
        {
            Dispatcher.adoptAsync(delegate
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer == null) return;
                var adorners = adornerLayer.GetAdorners(this);
                if (adorners != null)
                    foreach (var adorner in adorners)
                        adornerLayer.Remove(adorner);
            });
        }
        void ImageDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if(fileNames == null) return;
            foreach (string fileName in fileNames)
            {
                FileType type = Image.GetFileType(fileName);
                if(new[]{FileType.Image,FileType.Video}.Contains(type))
                    e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }
        void ImagesDrop(object sender, DragEventArgs e)
        {
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if(fileNames == null)
            {
                MessageBox.Show("Cannot Drop this onto the canvas");
                return;
            }
            Commands.SetLayer.Execute("Insert");
            var pos = e.GetPosition(this);
            //lets try for a 4xN grid
            for (var i = 0; i < fileNames.Count(); i++)
            {
                var filename = fileNames[i];
                var image = Image.createImageFromUri(new Uri(filename, UriKind.RelativeOrAbsolute));
                Commands.ImageDropped.Execute(new ImageDrop {filename = filename, point = pos, target = target, position = i});
                pos.X += image.Width;
                if ((i+1) % 4 == 0)
                {
                    pos.X = 0;
                    pos.Y += (image.Height + 30);
                }
            }
            e.Handled = true;
        }
        public void SetCanEdit(bool canEdit)
        {
            this.canEdit = canEdit;
            AllowDrop = canEdit;
            CanEditChanged();
        }
        protected bool inMeeting() {
            try
            {
                return Permissions.InferredTypeOf(Globals.conversationDetails.Permissions) == Permissions.MEETING_PERMISSIONS;
            }
            catch (NotSetException) {
                return false;
            }
        }
        protected override void  OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
         	 base.OnVisualChildrenChanged(visualAdded, visualRemoved);
             if(ChildrenChanged != null)
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