using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbonObjects;
using System.Windows.Media.Effects;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;
using MeTLLib.DataTypes;
using System.Diagnostics;

namespace SandRibbon.Components.Canvas
{
    public enum FileType
    {
        Video,
        Image,
        NotSupported
    }
    public class ImageInformation : TagInformation
    {
    }
    public class Image : AbstractCanvas
    {
        public Image()
        {
            EditingMode = InkCanvasEditingMode.Select;
            Background = Brushes.Transparent;
            PreviewKeyDown += keyPressed;
            SelectionMoved += elementsMovedOrResized;
            SelectionMoving += elementsMovingOrResizing;
            SelectionChanging += selectingImages;
            SelectionChanged += selectionChanged;
            SelectionResizing += elementsMovingOrResizing;
            SelectionResized += elementsResized;
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<IEnumerable<TargettedImage>>(ReceiveImages));
            Commands.ReceiveVideo.RegisterCommandToDispatcher<TargettedVideo>(new DelegateCommand<TargettedVideo>(ReceiveVideo));
            Commands.ReceiveDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyImage));
            Commands.ReceiveDirtyVideo.RegisterCommandToDispatcher<TargettedDirtyElement>(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyVideo));
            Commands.AddImage.RegisterCommandToDispatcher(new DelegateCommand<object>(addImageFromDisk));
            Commands.FileUpload.RegisterCommand(new DelegateCommand<object>(uploadFile));
            Commands.PlaceQuizSnapshot.RegisterCommand(new DelegateCommand<ImageDropParameters>(addImageFromQuizSnapshot));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<string>(changeSelectedItemsPrivacy));
            Commands.ImageDropped.RegisterCommandToDispatcher(new DelegateCommand<ImageDrop>(imagedDropped));
            Commands.ReceiveDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyLiveWindow));
            Commands.DugPublicSpace.RegisterCommand(new DelegateCommand<LiveWindowSetup>(DugPublicSpace));
            Commands.DeleteSelectedItems.RegisterCommand(new DelegateCommand<object>(deleteSelectedImages));
            Commands.MirrorVideo.RegisterCommand(new DelegateCommand<VideoMirror.VideoMirrorInformation>(mirrorVideo));
            Commands.VideoMirrorRefreshRectangle.RegisterCommand(new DelegateCommand<string>(mirrorVideoRefresh));
            Commands.HideConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(hideConversationSearchBox));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<object>(updateImagePrivacy));
            var undoHandler = new CommandBinding(ApplicationCommands.Undo, null, justNo);
            this.CommandBindings.Add(undoHandler);
        }
        private void justNo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }
        
        private void updateImagePrivacy(object obj)
        {
            foreach (System.Windows.Controls.Image image in Children)
                ApplyPrivacyStylingToElement(image, image.tag().privacy);
        }
        private void imagedDropped(ImageDrop drop)
        {
            try
            {
                if (drop.target.Equals(target) && me != "projector")
                    handleDrop(drop.filename, drop.point, drop.position);
            }
            catch (NotSetException)
            {
                //YAY
            }
        }

        private void hideConversationSearchBox(object obj)
        {
            addAdorners();
        }
        public void deleteSelectedImages(object obj)
        {
            if (GetSelectedElements().Count == 0) return;
            deleteImages();
            // set keyboard focus to the current canvas so the help button does not grey out
            Keyboard.Focus(this);
            ClearAdorners();
        }
        private void mirrorVideoRefresh(string id)
        {
            try
            {
                if (me == "projector") return;
                foreach (FrameworkElement fe in Children.ToList())
                {
                    if (fe is Video)
                    {
                        var video = (Video)fe;
                        video.UpdateMirror(id);
                    }
                }
            }
            catch (Exception) { }
        }
        private void mirrorVideo(MeTLLib.DataTypes.VideoMirror.VideoMirrorInformation info)
        {
            if (me != "projector") return;
            try
            {
                foreach (FrameworkElement fe in Children.ToList())
                {
                    if (fe is MeTLLib.DataTypes.VideoMirror)
                    {
                        var vm = (MeTLLib.DataTypes.VideoMirror)fe;
                        if (info.rect == null)
                            Children.Remove(vm);
                        else vm.UpdateMirror(info);
                    }
                }
            }
            catch (Exception) { }
        }
        private void ReceiveDirtyLiveWindow(TargettedDirtyElement dirtyElement)
        {
            if (target != dirtyElement.target) return;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child is RenderedLiveWindow && (string)((Rectangle)((RenderedLiveWindow)child).Rectangle).Tag == dirtyElement.identifier)
                    Children.Remove(child);
            }
        }
        private void keyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                deleteImages();
            }
        }
        private void deleteImages()
        {
            var selectedElements = new List<UIElement>();
            Dispatcher.adopt(() =>
                                 {
                                     selectedElements = GetSelectedClonedElements().Where(i=> ((System.Windows.Controls.Image)i).tag().author == Globals.me).ToList();
                                 });
            Trace.TraceInformation("DeletingImages");
            Action undo = () =>
                              {
                                  ClearAdorners();
                                  foreach (var element in selectedElements)
                                  {
                                     if (Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id ==
                                         ((System.Windows.Controls.Image)element).tag().id).Count() == 0)
                                          Children.Add(element);
                                      sendThisElement(element);
                                  }
                                  Select(selectedElements);
                                  addAdorners();
                              };
            Action redo = () =>
                             {
                                 ClearAdorners();
                                 foreach (var element in selectedElements)
                                 {
                                     if (Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id ==
                                         ((System.Windows.Controls.Image)element).tag().id).Count() > 0)
                                         Children.Remove(Children.ToList().Where(i =>((System.Windows.Controls.Image)i).tag().id 
                                             == ((System.Windows.Controls.Image)element).tag().id ).First());
                                     dirtyThisElement(element);
                                 }
                             };
            redo();
            UndoHistory.Queue(undo, redo);
        }
        public List<string> GetSelectedAuthors()
        {
            return GetSelectedElements().Where(s => ((System.Windows.Controls.Image)s).tag().author != me).Select(s => ((System.Windows.Controls.Image)s).tag().author).Distinct().ToList();

        }

        protected override void CanEditChanged()
        {
            canEdit = base.canEdit;
            if (privacy == "private") canEdit = true;
        }
        public void ReceiveImages(IEnumerable<TargettedImage> images)
        {
            var safeImages = images.Where(shouldDisplay).ToList();
            foreach (var image in safeImages)
            {
                TargettedImage image1 = image;
                
                Dispatcher.adoptAsync(() => AddImage(image1.image));
            }
            ensureAllImagesHaveCorrectPrivacy();
        }
        public void ReceiveVideos(IEnumerable<MeTLLib.DataTypes.TargettedVideo> videos)
        {
            var safeVideos = videos.Where(shouldDisplay).ToList();
            foreach (var video in safeVideos)
                ReceiveVideo(video);
            ensureAllImagesHaveCorrectPrivacy();
        }
        private bool shouldDisplay(MeTLLib.DataTypes.TargettedImage image)
        {
            return !(image.slide != currentSlide ||
                !(image.target.Equals(target)) ||
                (!(image.privacy == "public" || (image.author == Globals.me && me != "projector"))));
        }
        private bool shouldDisplay(MeTLLib.DataTypes.TargettedVideo video)
        {
            return !(video.slide != currentSlide ||
                !(video.target.Equals(target)) ||
                (!(video.privacy == "public" || (video.author == Globals.me && me != "projector"))));
        }
        private void ReceiveImage(MeTLLib.DataTypes.TargettedImage image)
        {
        }
        public void ReceiveVideo(MeTLLib.DataTypes.TargettedVideo video)
        {
            Dispatcher.adoptAsync(delegate
            {
                video.video.MediaElement.LoadedBehavior = MediaState.Manual;
                video.video.MediaElement.ScrubbingEnabled = true;
                if (me == "projector")
                    AddVideoClone(video.video);
                else
                    AddVideo(video.video);
            });
        }
        public void AddVideo(MeTLLib.DataTypes.Video element)
        {
            if (!videoExistsOnCanvas(element))
            {
                var height = element.Height;
                var width = element.Width;
                Children.Add(element);
                element.MediaElement.LoadedBehavior = MediaState.Manual;
                InkCanvas.SetLeft(element, element.X);
                InkCanvas.SetTop(element, element.Y);
                element.Height = height;
                element.Width = width;
            }
        }
        public void AddVideoClone(MeTLLib.DataTypes.Video element)
        {
            if (!videoExistsOnCanvas(element) && element.tag().privacy == "public")
            {
                var videoClone = new MeTLLib.DataTypes.VideoMirror();
                videoClone.id = element.tag().id;
                if (videoClone.Rectangle == null)
                    videoClone.RequestNewRectangle();
                Children.Add(videoClone);
                InkCanvas.SetLeft(videoClone, element.X);
                InkCanvas.SetTop(videoClone, element.Y);
                videoClone.Height = element.Height;
                videoClone.Width = element.Width;
            }
        }
        protected void ApplyPrivacyStylingToElement(FrameworkElement element, string privacy)
        {
            if (!Globals.conversationDetails.Permissions.studentCanPublish && !Globals.isAuthor)
            {
                RemovePrivacyStylingFromElement(element);
                return;
            }
            if (privacy != "private")
            {
                RemovePrivacyStylingFromElement(element);
                return;
            }

            applyShadowEffectTo(element, Colors.Black);
        }
        public FrameworkElement applyShadowEffectTo(FrameworkElement element, Color color)
        {
            element.Effect = new DropShadowEffect { BlurRadius = 50, Color = color, ShadowDepth = 0, Opacity = 1 };
            element.Opacity = 0.7;
            return element;
        }
        private void ensureAllImagesHaveCorrectPrivacy()
        {
            Dispatcher.adoptAsync(delegate
            {
                var images = new List<System.Windows.Controls.Image>();
                var videos = new List<MeTLLib.DataTypes.Video>();
                foreach (var child in Children)
                {
                    if (child is System.Windows.Controls.Image)
                        images.Add((System.Windows.Controls.Image)child);
                    if (child is MeTLLib.DataTypes.Video)
                        videos.Add((MeTLLib.DataTypes.Video)child);
                }
                foreach (System.Windows.Controls.Image image in images)
                    ApplyPrivacyStylingToElement(image, image.tag().privacy);
                foreach (MeTLLib.DataTypes.Video video in videos)
                    ApplyPrivacyStylingToElement(video, video.tag().privacy);

            });
        }
        public static IEnumerable<Point> getImagePoints(System.Windows.Controls.Image image)
        {
            var x = InkCanvas.GetLeft(image);
            var y = InkCanvas.GetTop(image);
            var width = image.Width;
            var height = image.Height;

            return new[]
            {
                new Point(x, y),
                new Point(x + width, y),
                new Point(x + width, y + height),
                new Point(x, y + height)
            };
        }
        public void ReceiveDirtyImage(TargettedDirtyElement element)
        {
            if (!(element.target.Equals(target))) return;
            if (element.slide != currentSlide) return;
            doDirtyImage(element.identifier);
        }
        public void ReceiveDirtyVideo(TargettedDirtyElement element)
        {
            if (!(element.target.Equals(target))) return;
            if (element.slide != currentSlide) return;
            doDirtyVideo(element.identifier);
        }
        private void doDirtyImage(string imageId)
        {
            Dispatcher.adoptAsync(delegate
            {
                dirtyImage(imageId);
            });
        }
        private void doDirtyVideo(string imageId)
        {
            Dispatcher.adoptAsync(delegate
            {
                dirtyVideo(imageId);
            });
        }

        private void dirtyImage(string imageId)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is System.Windows.Controls.Image)
                {
                    var currentImage = (System.Windows.Controls.Image)Children[i];
                    if (imageId.Equals(currentImage.tag().id))
                    {
                        Children.Remove(currentImage);
                    }
                }
            }
        }
        private void dirtyVideo(string videoId)
        {
            try
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    if (me == "projector")
                    {
                        if (Children[i] is MeTLLib.DataTypes.VideoMirror)
                        {
                            var currentVideoMirror = (MeTLLib.DataTypes.VideoMirror)Children[i];
                            if (videoId.Equals(currentVideoMirror.id))
                                Children.Remove(currentVideoMirror);
                        }
                    }
                    else
                    {
                        if (Children[i] is MeTLLib.DataTypes.Video)
                        {
                            var currentVideo = (MeTLLib.DataTypes.Video)Children[i];
                            if (videoId.Equals(currentVideo.Tag.ToString()))
                            {
                                Children.Remove(currentVideo);
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public void AddImage(System.Windows.Controls.Image image)
        {
            try
            {
                if (!imageExistsOnCanvas(image))
                {
                    Panel.SetZIndex(image, image.tag().isBackground ? 1 : 2);
                    Children.Add(image);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("I could not add this image because {0}", e));
            }
        }
        public void FlushImages()
        {
            Dispatcher.adoptAsync(delegate
            {
                Background = Brushes.Transparent;
                Children.Clear();
            });
        }
        protected override void HandlePaste()
        {
            if (Clipboard.ContainsImage())
            {
                var tmpFile = "tmpImage";
                using (FileStream fileStream = new FileStream(tmpFile, FileMode.OpenOrCreate))
                {
                    var frame = BitmapFrame.Create(Clipboard.GetImage());
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(frame);
                    encoder.QualityLevel = 100;
                    encoder.Save(fileStream);
                }
                if (File.Exists(tmpFile))
                {
                    var uri = MeTLLib.ClientFactory.Connection().NoAuthUploadResource(new System.Uri(tmpFile, UriKind.RelativeOrAbsolute), currentSlide);
                    var image = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(uri)
                    };
                    image.tag(new ImageTag(Globals.me, privacy, generateId(), false, -1));
                    SetLeft(image, 15);
                    SetTop(image, 15);
                    Commands.SendImage.ExecuteAsync(new TargettedImage
                    (currentSlide, Globals.me, target, privacy, image));
                }

                else MessageBox.Show("Sorry, your file could not be pasted.  Try dragging and dropping, or selecting with the add image button.");
            }
        }

        protected override void HandleCopy()
        {
            foreach (var image in GetSelectedElements().Where(e => e is System.Windows.Controls.Image))
                Clipboard.SetImage((BitmapSource)((System.Windows.Controls.Image)image).Source);
        }
        protected override void HandleCut()
        {
            var listToCut = new List<TargettedDirtyElement>();

            foreach (var element in GetSelectedElements().Where(e => e is System.Windows.Controls.Image))
            {
                var image = (System.Windows.Controls.Image)element;
                ApplyPrivacyStylingToElement(image, image.tag().privacy);
                Clipboard.SetImage((BitmapSource)image.Source);
                listToCut.Add(new TargettedDirtyElement(currentSlide, Globals.me, target, image.tag().privacy, image.tag().id));
            }
            var selectedImages = GetSelectedClonedElements();
            Action redo = () =>
                            {
                                ClearAdorners();
                                foreach (var element in listToCut)
                                    Commands.SendDirtyImage.ExecuteAsync(element);
                            };
            Action undo = () =>
            {
                foreach (var element in listToCut)
                    Clipboard.GetImage(); //remove the images from the undo queue

                var selection = new List<UIElement>();
                foreach (var element in selectedImages)
                {

                    if (!Children.Contains(element))
                        Children.Add(element);
                    sendThisElement(element);
                    selection.Add(element);
                }
                Select(selection);
                addAdorners();

            };
            UndoHistory.Queue(undo, redo);
            redo();
        }
        #region EventHandlers
        /*Event Handlers*/
        private void selectingImages(object sender, InkCanvasSelectionChangingEventArgs e)
        {
            e.SetSelectedElements(filterMyImages(e.GetSelectedElements()));
        }
        private IEnumerable<UIElement> filterMyImages(IEnumerable<UIElement> elements)
        {
           // if (inMeeting() || Globals.isAuthor) return elements;
            if (true || inMeeting()) return elements;
            var myImages = new List<UIElement>();
            foreach (UIElement image in elements)
            {
                if (image.GetType().ToString() == "System.Windows.Controls.Image")
                {
                    if (!((System.Windows.Controls.Image)image).Tag.ToString().StartsWith("NOT_LOADED"))
                    {
                        var newImage = (System.Windows.Controls.Image)image;
                        if (newImage.tag().author == Globals.me)
                            myImages.Add((System.Windows.Controls.Image)image);
                    }
                }
                if (image.GetType() == typeof(MeTLLib.DataTypes.AutoShape))
                    myImages.Add((AutoShape)image);
                if (image.GetType() == typeof(MeTLLib.DataTypes.RenderedLiveWindow))
                    myImages.Add((RenderedLiveWindow)image);
                if (image.GetType() == typeof(MeTLLib.DataTypes.Video))
                {
                    ((Video)image).MediaElement.LoadedBehavior = MediaState.Manual;
                    myImages.Add((Video)image);
                }
            }
            return myImages;
        }
        private void selectionChanged(object sender, EventArgs e)
        {
            ClearAdorners();
            addAdorners();
        }

        internal void addAdorners()
        {
            var selectedElements = GetSelectedElements();
            if (selectedElements.Count == 0) return;
            var publicElements = selectedElements.Where(i => (((i is System.Windows.Controls.Image) && ((System.Windows.Controls.Image)i).tag().privacy.ToLower() == "public")) || ((i is Video) && ((Video)i).tag().privacy.ToLower() == "public")).ToList();
            var myElements = selectedElements.Where(i => ((i is System.Windows.Controls.Image) && ((System.Windows.Controls.Image)i).tag().author == Globals.me)).ToList();
            string privacyChoice;
            if (publicElements.Count == 0)
                privacyChoice = "show";
            else if (publicElements.Count == selectedElements.Count)
                privacyChoice = "hide";
            else
                privacyChoice = "both";
            Commands.AddPrivacyToggleButton.ExecuteAsync(new PrivacyToggleButton.PrivacyToggleButtonInfo(privacyChoice, myElements.Count != 0, GetSelectionBounds()));
        }
        List<UIElement> elementsAtStartOfTheMove = new List<UIElement>();

        private double Clamp(double val, double min, double max)
        {
            if (val < min) 
                return min;
            else if ( val > max)
                return max;
            else 
                return val;
        }

        private void elementsMovingOrResizing(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            elementsAtStartOfTheMove.Clear();
            elementsAtStartOfTheMove = GetSelectedClonedElements();

            if (e.NewRectangle.Width == e.OldRectangle.Width && e.NewRectangle.Height == e.OldRectangle.Height)
                return;

            Rect imageCanvasRect = new Rect(new Size(ActualWidth, ActualHeight));

            double resizeWidth;
            double resizeHeight;
            double imageX;
            double imageY;

            if (e.NewRectangle.Right > imageCanvasRect.Right)
                resizeWidth = Clamp(imageCanvasRect.Width - e.NewRectangle.X, 0, imageCanvasRect.Width);
            else
                resizeWidth = e.NewRectangle.Width;

            if (e.NewRectangle.Height > imageCanvasRect.Height)
                resizeHeight = Clamp(imageCanvasRect.Height - e.NewRectangle.Y, 0, imageCanvasRect.Height);
            else
                resizeHeight = e.NewRectangle.Height;

            imageX = Clamp(e.NewRectangle.X, 0, e.NewRectangle.X);
            imageY = Clamp(e.NewRectangle.Y, 0, e.NewRectangle.Y);
            e.NewRectangle = new Rect(imageX, imageY, resizeWidth, resizeHeight);
        }

        private List<UIElement> GetSelectedClonedElements()
        {
            var selectedElements = new List<UIElement>();
            foreach (var element in GetSelectedElements())
            {
                if (element is System.Windows.Controls.Image)
                    selectedElements.Add(((System.Windows.Controls.Image)element).clone());
                else if (element is Video)
                    selectedElements.Add(((Video)element).clone());
                SetLeft(selectedElements.Last(), GetLeft(element));
                SetTop(selectedElements.Last(), GetTop(element));
            }
            return selectedElements;
        }
        private void elementsResized(object sender, EventArgs e)
        {
            var selectedElements = GetSelectedElements();
            foreach (var element in selectedElements)
            {
               // move the images back on the canvas if they moved out of range while resizing
                var imageX = GetLeft(element);
                var imageY = GetTop(element);
                imageX = imageX < 0 ? 0: imageX;
                imageY = imageY < 0 ? 0: imageY;

                SetLeft(element, imageX);
                SetTop(element, imageY);
            }
            elementsMovedOrResized(sender, e);
        }

        private void elementsMovedOrResized(object sender, EventArgs e)
        {
            ClearAdorners();
            var selectedElements = GetSelectedClonedElements();
            var startingElements = elementsAtStartOfTheMove.Select(i => ((System.Windows.Controls.Image)i).clone()).ToList();
            Trace.TraceInformation("MovingImages {0}", string.Join(",", startingElements.Select(el => el.tag().ToString()).ToArray()));
            Action undo = () =>
              {
                  ClearAdorners();
                  var selection = new List<UIElement>();
                  var mySelectedElements = selectedElements.Select(i => ((System.Windows.Controls.Image)i).clone()).ToList();
                  foreach (var element in mySelectedElements)
                  {
                      if (Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id == element.tag().id).Count() > 0)
                          Children.Remove(Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id == element.tag().id).FirstOrDefault());
                      if (!element.Tag.ToString().StartsWith("NOT_LOADED"))
                          dirtyThisElement(element);
                  }
                  foreach (var element in startingElements)
                  {
                      selection.Add(element);
                      if (Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id == element.tag().id).Count() == 0)
                          Children.Add(element);
                      if (!element.Tag.ToString().StartsWith("NOT_LOADED"))
                          sendThisElement(element);
                  }
                  addAdorners();
              };
            Action redo = () =>
              {
                  ClearAdorners();
                  var selection = new List<UIElement>();
                  var mySelectedImages = selectedElements.Select(i => ((System.Windows.Controls.Image)i).clone()).ToList();
                  foreach (var element in startingElements)
                  {
                      if (Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id == element.tag().id).Count() > 0)
                          Children.Remove(Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id == element.tag().id).FirstOrDefault());
                      dirtyThisElement(element);
                  }
                  foreach (var element in mySelectedImages)
                  {
                      selection.Add(element);
                      if (Children.ToList().Where(i => ((System.Windows.Controls.Image)i).tag().id == element.tag().id).Count() == 0)
                          Children.Add(element);
                      sendThisElement(element);
                  }
                  Select(new List<UIElement>());
                  addAdorners();
              };
            UndoHistory.Queue(undo, redo);
            redo();
        }

        private void sendThisElement(UIElement element)
        {

            switch (element.GetType().ToString())
            {
                case "System.Windows.Controls.Image":
                    var newImage = (System.Windows.Controls.Image)element;
                    newImage.UpdateLayout();

                    Commands.SendImage.Execute(new TargettedImage(currentSlide, Globals.me, target, newImage.tag().privacy, newImage));
                    break;
                case "MeTLLib.DataTypes.Video":
                    var srVideo = (Video)element;
                    srVideo.UpdateLayout();
                    srVideo.X = GetLeft(srVideo);
                    srVideo.Y = GetTop(srVideo);
                    Commands.SendVideo.Execute(new TargettedVideo(currentSlide, Globals.me, target, srVideo.tag().privacy, srVideo));
                    break;
            }
        }
        private void dirtyThisElement(UIElement element)
        {
            var thisImage = (System.Windows.Controls.Image)element;
            var dirtyElement = new TargettedDirtyElement(currentSlide, Globals.me, target,thisImage.tag().privacy, thisImage.tag().id );
            switch (element.GetType().ToString())
            {
                case "System.Windows.Controls.Image":
                    var image = (System.Windows.Controls.Image)element;
                    ApplyPrivacyStylingToElement(image, image.tag().privacy);
                    Commands.SendDirtyImage.Execute(dirtyElement);
                    break;
                case "MeTLLib.DataTypes.Video":
                    Commands.MirrorVideo.ExecuteAsync(new VideoMirror.VideoMirrorInformation(dirtyElement.identifier, null));
                    Commands.SendDirtyVideo.ExecuteAsync(dirtyElement);
                    break;
            }

        }
        private void DugPublicSpace(MeTLLib.DataTypes.LiveWindowSetup setup)
        {
            if (target != "notepad") return;
            Dispatcher.adopt((Action)delegate
            {
                var view = new Rect(setup.origin, new Size(setup.frame.Width, setup.frame.Height));
                var liveWindow = new Rectangle
                {
                    Width = setup.frame.Width,
                    Height = setup.frame.Height,
                    Fill = new VisualBrush
                    {
                        Visual = setup.visualSource,
                        TileMode = TileMode.None,
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top,
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = view
                    },
                    Tag = setup.snapshotAtTimeOfCreation
                };
                var RLW = new MeTLLib.DataTypes.RenderedLiveWindow()
                {
                    Rectangle = liveWindow,
                    Height = liveWindow.Height,
                    Width = liveWindow.Width
                };
                Children.Add(RLW);
                InkCanvas.SetLeft(RLW, setup.target.X);
                InkCanvas.SetTop(RLW, setup.target.Y);
            });
        }
        #endregion

        #region Video
        private MeTLLib.DataTypes.Video newVideo(System.Uri Source)
        {
            var MeTLVideo = new MeTLLib.DataTypes.Video()
                {
                    VideoSource = Source,
                };
            return MeTLVideo;
        }
        #endregion
        #region ImageImport
        private void addImageFromDisk(object obj)
        {
            addResourceFromDisk((files) =>
            {
                int i = 0;
                foreach (var file in files)
                    handleDrop(file, new Point(0, 0), i++);
            });
        }
        private void uploadFile(object _obj)
        {
            addResourceFromDisk("All files (*.*)|*.*", (files) =>
                                    {
                                        foreach (var file in files)
                                        {
                                            uploadFileForUse(file);
                                        }
                                    });
        }
        private void addImageFromQuizSnapshot(ImageDropParameters parameters)
        {
            if (target == "presentationSpace" && me != "projector")
                handleDrop(parameters.file, parameters.location, 1);
        }
        private void addResourceFromDisk(Action<IEnumerable<string>> withResources)
        {
            const string filter = "Image files(*.jpeg;*.gif;*.bmp;*.jpg;*.png)|*.jpeg;*.gif;*.bmp;*.jpg;*.png|All files (*.*)|*.*";
            addResourceFromDisk(filter, withResources);
        }

        private void addResourceFromDisk(string filter, Action<IEnumerable<string>> withResources)
        {
            if (target == "presentationSpace" && canEdit && me != "projector")
            {
                string initialDirectory = "c:\\";
                foreach (var path in new[] { Environment.SpecialFolder.MyPictures, Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolder.MyComputer })
                    try
                    {
                        initialDirectory = Environment.GetFolderPath(path);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                var fileBrowser = new OpenFileDialog
                                             {
                                                 InitialDirectory = initialDirectory,
                                                 Filter = filter,
                                                 FilterIndex = 1,
                                                 RestoreDirectory = true
                                             };
                DisableDragDrop(); 
                var dialogResult = fileBrowser.ShowDialog(Window.GetWindow(this));
                EnableDragDrop();

                if (dialogResult == true)
                    withResources(fileBrowser.FileNames);
            }
        }

        private void DisableDragDrop()
        {
            DragOver -= ImageDragOver;
            Drop -= ImagesDrop;
            DragOver += ImageDragOverCancel;
        }

        private void EnableDragDrop()
        {
            DragOver -= ImageDragOverCancel;
            DragOver += ImageDragOver;
            Drop += ImagesDrop;
        }

        protected void ImageDragOverCancel(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        public void dropVideoOnCanvas(string filename, Point pos, int count)
        {
            FileType type = GetFileType(filename);
            if (type == FileType.Video)
            {
                Dispatcher.adopt(() =>
                {
                    var placeHolderMe = new MediaElement
                    {
                        Source = new Uri(filename, UriKind.Relative),
                        Width = 200,
                        Height = 200,
                        LoadedBehavior = MediaState.Manual
                    };
                    var placeHolder = new Video { MediaElement = placeHolderMe, VideoSource = placeHolderMe.Source };
                    InkCanvas.SetLeft(placeHolder, pos.X);
                    InkCanvas.SetTop(placeHolder, pos.Y);
                    var animationPulse = new DoubleAnimation
                                             {
                                                 From = .3,
                                                 To = 1,
                                                 Duration = new Duration(TimeSpan.FromSeconds(1)),
                                                 AutoReverse = true,
                                                 RepeatBehavior = RepeatBehavior.Forever
                                             };
                    placeHolder.BeginAnimation(OpacityProperty, animationPulse);
                    placeHolder.tag(new MeTLLib.DataTypes.ImageTag
                                      {
                                          author = Globals.me,
                                          id = generateId(),
                                          privacy = privacy,
                                          zIndex = -1
                                      });
                    MeTLLib.ClientFactory.Connection().UploadAndSendVideo(new MeTLStanzas.LocalVideoInformation
                    (currentSlide, Globals.me, target, privacy, placeHolder, filename, false));
                    Children.Remove(placeHolder);
                });
            }
        }
        public void handleDrop(string fileName, Point pos, int count)
        {
            FileType type = GetFileType(fileName);
            switch (type)
            {
                case FileType.Image:
                    dropImageOnCanvas(fileName, pos, count);
                    break;
                case FileType.Video:
                    break;
                default:
                    uploadFileForUse(fileName);
                    break;
            }
        }
        private const int KILOBYTE = 1024;
        private const int MEGABYTE = 1024 * KILOBYTE;
        private bool isFileLessThanXMB(string filename, int size)
        {
            if (filename.StartsWith("http")) return true;
            var info = new FileInfo(filename);
            if (size != null && info.Length > size * MEGABYTE)
            {
                return false;
            }
            return true;
        }
        private int fileSizeLimit = 50;
        private void uploadFileForUse(string unMangledFilename)
        {
            string filename = unMangledFilename + ".MeTLFileUpload";
            if (filename.Length > 260)
            {
                MessageBox.Show("Sorry, your filename is too long, must be less than 260 characters");
                return;
            }
            if (isFileLessThanXMB(unMangledFilename, fileSizeLimit))
            {
                var worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                 {
                     File.Copy(unMangledFilename, filename);
                     MeTLLib.ClientFactory.Connection().UploadAndSendFile(
                         new MeTLStanzas.LocalFileInformation(currentSlide, Globals.me, target, "public", filename, Path.GetFileNameWithoutExtension(filename), false, new FileInfo(filename).Length, DateTimeFactory.Now().Ticks.ToString()));
                     File.Delete(filename);
                 };
                worker.RunWorkerCompleted += (s, a) => Dispatcher.Invoke(DispatcherPriority.Send,
                                                                                   (Action)(() => MessageBox.Show(string.Format("Finished uploading {0}.", unMangledFilename))));
                worker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show(String.Format("Sorry, your file is too large, must be less than {0}mb", fileSizeLimit));
                return;
            }
        }
        public void dropImageOnCanvas(string fileName, Point pos, int count)
        {
            if (!isFileLessThanXMB(fileName, fileSizeLimit))
            {
                MessageBox.Show(String.Format("Sorry, your file is too large, must be less than {0}mb", fileSizeLimit));
                return;
            }
            Dispatcher.adopt(() =>
            {
                System.Windows.Controls.Image image = null;
                try
                {
                    image = createImageFromUri(new Uri(fileName, UriKind.RelativeOrAbsolute));
                }
                catch (Exception e)
                {
                    MessageBox.Show("Sorry could not create an image from this file :" + fileName + "\n Error: " + e.Message);
                    return;
                }
                if (image == null)
                    return;
                SetLeft(image, pos.X);
                SetTop(image, pos.Y);
                image.tag(new ImageTag(Globals.me, privacy, generateId(), false, 0));
                if (!fileName.StartsWith("http"))
                    MeTLLib.ClientFactory.Connection().UploadAndSendImage(new MeTLStanzas.LocalImageInformation(currentSlide, Globals.me, target, privacy, image, fileName, false));
                else
                    MeTLLib.ClientFactory.Connection().SendImage(new TargettedImage(currentSlide, Globals.me, target, privacy, image));
                var myImage = image.clone();
                Action undo = () =>
                                  {
                                      ClearAdorners();
                                      if (Children.Contains(myImage))
                                          Children.Remove(myImage);
                                      dirtyThisElement(myImage);
                                  };
                Action redo = () =>
                {
                    ClearAdorners();
                    SetLeft(myImage, pos.X);
                    SetTop(myImage, pos.Y);
                    if (!Children.Contains(myImage))
                        Children.Add(myImage);
                    sendThisElement(myImage);

                    Select(new[] { myImage });
                    addAdorners();
                };
                UndoHistory.Queue(undo, redo);
                Children.Add(image);
            });
        }
        public static System.Windows.Controls.Image createImageFromUri(Uri uri)
        {
            var image = new System.Windows.Controls.Image();
            var jpgFrame = BitmapFrame.Create(uri);
            image.Source = jpgFrame;
            image.Height = jpgFrame.Height;
            image.Width = jpgFrame.Width;
            image.Stretch = Stretch.Uniform;
            image.StretchDirection = StretchDirection.Both;
            image.Margin = new Thickness(5);
            return image;
        }
        public static FileType GetFileType(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLower();
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".bmp" || extension == ".gif" || extension == ".png" || extension == ".dib")
                return FileType.Image;
            if (extension == ".wmv")
                return FileType.Video;
            return FileType.NotSupported;
        }
        #endregion
        #region UtilityMethods
        /*Utility methods*/
        private bool videoExistsOnCanvas(MeTLLib.DataTypes.Video testVideo)
        {
            foreach (UIElement video in Children)
                if (video is MeTLLib.DataTypes.Video)
                    if (videoCompare((MeTLLib.DataTypes.Video)video, testVideo))
                        return true;
            return false;
        }
        private bool imageExistsOnCanvas(System.Windows.Controls.Image testImage)
        {
            foreach (UIElement image in Children)
                if (image is System.Windows.Controls.Image)
                    if (imageCompare((System.Windows.Controls.Image)image, testImage))
                        return true;
            return false;
        }
        private static bool imageCompare(System.Windows.Controls.Image image, System.Windows.Controls.Image currentImage)
        {
            if (System.Windows.Controls.Canvas.GetTop(currentImage) == System.Windows.Controls.Canvas.GetTop(image))
                return false;
            if (System.Windows.Controls.Canvas.GetLeft(currentImage) == System.Windows.Controls.Canvas.GetLeft(image))
                return false;
            if (image.Source.ToString() != currentImage.Source.ToString())
                return false;
            if (image.tag().id != currentImage.tag().id)
                return false;
            return true;
        }
        private static bool videoCompare(MeTLLib.DataTypes.Video video, MeTLLib.DataTypes.Video currentVideo)
        {
            if (System.Windows.Controls.Canvas.GetTop(currentVideo) == System.Windows.Controls.Canvas.GetTop(video))
                return false;
            if (System.Windows.Controls.Canvas.GetLeft(currentVideo) == System.Windows.Controls.Canvas.GetLeft(video))
                return false;
            if (video.VideoSource.ToString() != currentVideo.VideoSource.ToString())
                return false;
            if (video.tag().id != currentVideo.tag().id)
                return false;
            return true;
        }
        #endregion
        public override void showPrivateContent()
        {
            foreach (UIElement child in Children)
                if (child.GetType() == typeof(System.Windows.Controls.Image) && ((System.Windows.Controls.Image)child).tag().privacy == "private")
                    child.Visibility = Visibility.Visible;
        }
        public override void hidePrivateContent()
        {
            foreach (UIElement child in Children)
                if (child.GetType() == typeof(System.Windows.Controls.Image) && ((System.Windows.Controls.Image)child).tag().privacy == "private")
                    child.Visibility = Visibility.Collapsed;
        }
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new ImageAutomationPeer(this);
        }
        private void changeSelectedItemsPrivacy(string newPrivacy)
        {
            if (me != "projector")
            {
                List<UIElement> selectedElements = new List<UIElement>();
                Dispatcher.adopt(() => selectedElements = GetSelectedElements().ToList());

                foreach (System.Windows.Controls.Image image in selectedElements.Where(i => i is System.Windows.Controls.Image && ((System.Windows.Controls.Image)i).tag().privacy != newPrivacy))
                {
                    var oldTag = image.tag();
                    Commands.SendDirtyImage.ExecuteAsync(new TargettedDirtyElement (currentSlide, image.tag().author, target, image.tag().privacy, image.tag().id));
                    oldTag.privacy = newPrivacy;
                    image.tag(oldTag);
                    var privateRoom = string.Format("{0}{1}", currentSlide, image.tag().author);
                    if(newPrivacy.ToLower() == "private" && Globals.isAuthor && Globals.me != image.tag().author)
                        Commands.SneakInto.Execute(privateRoom);
                    Commands.SendImage.ExecuteAsync(new TargettedImage(currentSlide, image.tag().author, target, newPrivacy, image));
                    if(newPrivacy.ToLower() == "private" && Globals.isAuthor && Globals.me != image.tag().author)
                        Commands.SneakOutOf.Execute(privateRoom);
                        
                }
            }
            Dispatcher.adoptAsync(() => Select(new List<UIElement>()));
        }
    }
    class ImageAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public ImageAutomationPeer(Image owner)
            : base(owner)
        {
        }
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;
            return base.GetPattern(patternInterface);
        }
        private Image Image
        {
            get { return (Image)base.Owner; }
        }
        protected override string GetAutomationIdCore()
        {
            return "images";
        }
        public void SetValue(string value)
        {
            Image.dropImageOnCanvas(value, new Point(0, 0), 1);
        }
        bool IValueProvider.IsReadOnly
        {
            get { return false; }
        }
        string IValueProvider.Value
        {
            get
            {
                var img = Image;
                var sb = new StringBuilder("<image>");
                foreach (var toString in from UIElement image in img.Children
                                         select new MeTLStanzas.Image(new TargettedImage
                                         (Globals.slide, Globals.me, img.target, img.privacy, (System.Windows.Controls.Image)image)).ToString())
                    sb.Append(toString);
                sb.Append("</image>");
                return sb.ToString();
            }
        }
    }
    class ImageImpl : Image
    {
    }
    class ImageDropParameters {
        public string file { get; set; }
        public Point location { get; set; }
    }
}
