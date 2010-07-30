using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using Newtonsoft.Json;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;
using System.Windows.Media.Effects;

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
        private static readonly int PADDING = 5;
        public Image()
        {
            EditingMode = InkCanvasEditingMode.Select;
            Background = Brushes.Transparent;
            PreviewKeyDown += keyPressed;
            SelectionMoved += transmitImageAltered;
            SelectionMoving += dirtyImage;
            SelectionChanging += selectingImages;
            SelectionResizing += dirtyImage;
            SelectionResized += transmitImageAltered;
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<IEnumerable<TargettedImage>>(ReceiveImages));
            Commands.ReceiveVideo.RegisterCommand(new DelegateCommand<TargettedVideo>(ReceiveVideo));
            Commands.ReceiveAutoShape.RegisterCommand(new DelegateCommand<TargettedAutoShape>(ReceiveAutoShape));
            Commands.ReceiveDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyImage));
            Commands.ReceiveDirtyVideo.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyVideo));
            Commands.ReceiveDirtyAutoShape.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyAutoShape));
            Commands.AddAutoShape.RegisterCommand(new DelegateCommand<object>(createNewAutoShape));
            Commands.AddImage.RegisterCommand(new DelegateCommand<object>(addImageFromDisk));
            Commands.QuizResultsSnapshotAvailable.RegisterCommand(new DelegateCommand<string>(addImageFromQuizSnapshot));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<string>(changeSelectedItemsPrivacy));
            Commands.ImageDropped.RegisterCommand(new DelegateCommand<ImageDrop>((drop) =>
            {
                try
                {
                    if (drop.target.Equals(target) && me != "projector")
                        handleDrop(drop.filename, drop.point, drop.position);
                }
                catch (NotSetException e)
                {
                    //YAY
                }
            }));
            Commands.ReceiveDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyLiveWindow));
            Commands.DugPublicSpace.RegisterCommand(new DelegateCommand<LiveWindowSetup>(DugPublicSpace));
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
                var numberOfImages = GetSelectedElements().Count;
                for (var i = 0; i < numberOfImages; i++)
                {
                    if ((GetSelectedElements().ElementAt(i)).GetType().ToString() == "System.Windows.Controls.Image")
                    {
                        var image = (System.Windows.Controls.Image)GetSelectedElements().ElementAt(i);
                        if (image.tag().privacy == "private") removePrivateRegion(image);
                        UndoHistory.Queue(
                            () =>
                            {
                                AddImage(image);
                                Commands.SendImage.Execute(new TargettedImage
                                                               {
                                                                   author = image.tag().author,
                                                                   slide = currentSlide,
                                                                   privacy = privacy,
                                                                   target = target,
                                                                   image = image
                                                               });
                            },
                            () =>
                            {
                                Children.Remove(image);
                                Commands.SendDirtyImage.Execute(new TargettedDirtyElement
                                                                    {
                                                                        identifier = image.tag().id,
                                                                        target = target,
                                                                        privacy = image.tag().privacy,
                                                                        author = image.tag().author,
                                                                        slide = currentSlide
                                                                    });
                            });

                        Commands.SendDirtyImage.Execute(new TargettedDirtyElement
                                                        {
                                                            identifier = image.tag().id,
                                                            target = target,
                                                            privacy = image.tag().privacy,
                                                            author = Globals.me,
                                                            slide = currentSlide
                                                        });
                    }
                    if ((GetSelectedElements().ElementAt(i)).GetType().ToString() == "SandRibbonInterop.AutoShape")
                    {
                        var autoshape = (SandRibbonInterop.AutoShape)GetSelectedElements().ElementAt(i);
                        Commands.SendDirtyAutoShape.Execute(new TargettedDirtyElement
                        {
                            identifier = autoshape.Tag.ToString(),
                            target = target,
                            privacy = privacy,
                            author = Globals.me,
                            slide = currentSlide
                        });
                    }
                    if ((GetSelectedElements().ElementAt(i)).GetType().ToString() == "SandRibbonInterop.Video")
                    {
                        var video = (SandRibbonInterop.Video)GetSelectedElements().ElementAt(i);
                        Commands.SendDirtyVideo.Execute(new TargettedDirtyElement
                        {
                            identifier = video.Tag.ToString(),
                            target = target,
                            privacy = privacy,
                            author = Globals.me,
                            slide = currentSlide
                        });
                    }
                    if ((GetSelectedElements().ElementAt(i)).GetType().ToString() == "SandRibbonInterop.RenderedLiveWindow")
                    {
                        var liveWindow = (SandRibbonInterop.RenderedLiveWindow)GetSelectedElements().ElementAt(i);
                        Commands.SendDirtyLiveWindow.Execute(new TargettedDirtyElement
                        {
                            identifier = ((Rectangle)((RenderedLiveWindow)liveWindow).Rectangle).Tag.ToString(),
                            target = target,
                            privacy = privacy,
                            author = Globals.me,
                            slide = currentSlide
                        });
                    }
                }
            }
        }
        protected override void CanEditChanged()
        {
            canEdit = base.canEdit;
            if (privacy == "private") canEdit = true;
        }
        public void ReceiveImages(IEnumerable<TargettedImage> images)
        {
            var safeImages = images.Where(shouldDisplay);
            foreach (var image in safeImages)
                ReceiveImage(image);
            ensureAllImagesHaveCorrectPrivacy();
        }
        private bool shouldDisplay(TargettedImage image)
        {
            return !(image.slide != currentSlide ||
                !(image.target.Equals(target)) ||
                (!(image.privacy == "public" || (image.author == Globals.me && me != "projector"))));
        }
        private void ReceiveImage(TargettedImage image)
        {
            Dispatcher.adoptAsync(delegate
            {
                AddImage(image.image);
            });
        }
        private void ReceiveVideo(TargettedVideo video)
        {
            //videos currently disabled.  Remove the return to re-enable.
            return;
            Dispatcher.adoptAsync(delegate
            {
                video.video.MediaElement.LoadedBehavior = MediaState.Manual;
                video.video.MediaElement.ScrubbingEnabled = true;
                AddVideo(video.video);
            });
        }
        public void AddVideo(SandRibbonInterop.Video element)
        {
            if (!videoExistsOnCanvas(element))
            {
                Children.Add(element);
                element.MediaElement.LoadedBehavior = MediaState.Manual;
                InkCanvas.SetLeft(element, element.X);
                InkCanvas.SetTop(element, element.Y);
            }
        }
        private void ensureAllImagesHaveCorrectPrivacy()
        {
            Dispatcher.adoptAsync(delegate
            {
                var images = new List<System.Windows.Controls.Image>();
                foreach (var child in Children)
                    if (child is System.Windows.Controls.Image)
                        images.Add((System.Windows.Controls.Image)child);
                foreach (System.Windows.Controls.Image image in images)
                    ApplyPrivacyStylingToElement(image, image.tag().privacy);
                /*if (image.tag().privacy == "private")
                        addPrivateRegion(image);*/
            });
        }
        private void addPrivateRegion(System.Windows.Controls.Image image)
        {
            if (image != null && image is System.Windows.Controls.Image)
                ApplyPrivacyStylingToElement(image, image.tag().privacy);
            //addPrivateRegion(getImagePoints(image));
        }
        private void removePrivateRegion(System.Windows.Controls.Image image)
        {
            if (image != null && image is System.Windows.Controls.Image)
                RemovePrivacyStylingFromElement(image);
            //removePrivateRegion(getImagePoints(image));
        }
        public static IEnumerable<Point> getImagePoints(System.Windows.Controls.Image image)
        {
            var x = InkCanvas.GetLeft(image) + PADDING;
            var y = InkCanvas.GetTop(image) + PADDING;
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
            if (!(element.slide == currentSlide)) return;
            doDirtyImage(element.identifier);
        }
        public void ReceiveDirtyVideo(TargettedDirtyElement element)
        {
            if (!(element.target.Equals(target))) return;
            if (!(element.slide == currentSlide)) return;
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
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is SandRibbonInterop.Video)
                {
                    var currentVideo = (SandRibbonInterop.Video)Children[i];
                    if (videoId.Equals(currentVideo.Tag.ToString()))
                    {
                        Children.Remove(currentVideo);
                    }
                }
            }
        }
        public void AddImage(System.Windows.Controls.Image image)
        {
            try
            {
                if (image.tag().isBackground)
                    Background = new VisualBrush(image);
                else if (!imageExistsOnCanvas(image))
                {
                    image.Margin = new Thickness(PADDING, PADDING, PADDING, PADDING);
                    Children.Add(image);
                    //ApplyPrivacyStylingToElement(image, image.tag().privacy);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Sorry, your image could not be imported");
            }
        }
        public void ReceiveAutoShape(TargettedAutoShape autoshape)
        {
            return;
        }
        public void ReceiveDirtyAutoShape(TargettedDirtyElement autoshape)
        {
            return;
        }
        public void AddAutoShape(TargettedAutoShape autoshape)
        {
            if (!autoshapeExistsOnCanvas(autoshape.autoshape))
                Children.Add(autoshape.autoshape);
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
                    var hostedFileName = ResourceUploader.uploadResource(currentSlide.ToString(), tmpFile);
                    var uri = new Uri(hostedFileName, UriKind.RelativeOrAbsolute);
                    var image = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(uri)
                    };
                    image.tag(new ImageTag
                                  {
                                      author = Globals.me,
                                      id = string.Format("{0}:{1}:{2}", Globals.me, SandRibbonObjects.DateTimeFactory.Now(), 1),
                                      privacy = privacy,
                                      zIndex = -1
                                  });
                    InkCanvas.SetLeft(image, 15);
                    InkCanvas.SetTop(image, 15);
                    Commands.SendImage.Execute(new TargettedImage
                    {
                        author = Globals.me,
                        slide = currentSlide,
                        privacy = privacy,
                        target = target,
                        image = image
                    });
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

            foreach (var image in GetSelectedElements().Where(e => e is System.Windows.Controls.Image))
            {
                if (((System.Windows.Controls.Image)image).tag().privacy == "private") removePrivateRegion(((System.Windows.Controls.Image)image));
                Clipboard.SetImage((BitmapSource)((System.Windows.Controls.Image)image).Source);
                listToCut.Add(new TargettedDirtyElement
                    {
                        identifier = ((System.Windows.Controls.Image)image).tag().id,
                        target = target,
                        privacy = ((System.Windows.Controls.Image)image).tag().privacy,
                        author = Globals.me,
                        slide = currentSlide
                    });
            }
            foreach (var element in listToCut)
                Commands.SendDirtyImage.Execute(element);
        }
        #region EventHandlers
        /*Event Handlers*/
        private void selectingImages(object sender, InkCanvasSelectionChangingEventArgs e)
        {
            e.SetSelectedElements(filterMyImages(e.GetSelectedElements()));
        }
        private IEnumerable<UIElement> filterMyImages(IEnumerable<UIElement> elements)
        {
            if (inMeeting()) return elements;
            var myImages = new List<UIElement>();
            foreach (UIElement image in elements)
            {
                if (image.GetType().ToString() == "System.Windows.Controls.Image")
                {
                    var imageInfo = JsonConvert.DeserializeObject<ImageInformation>(((System.Windows.Controls.Image)image).Tag.ToString());
                    if (imageInfo.Author == Globals.me)
                        myImages.Add((System.Windows.Controls.Image)image);
                }
                if (image.GetType().ToString() == "SandRibbonInterop.AutoShape")
                    myImages.Add((SandRibbonInterop.AutoShape)image);
                if (image.GetType().ToString() == "SandRibbonInterop.RenderedLiveWindow")
                    myImages.Add((SandRibbonInterop.RenderedLiveWindow)image);
                if (image.GetType().ToString() == "SandRibbonInterop.Video")
                {
                    ((SandRibbonInterop.Video)image).MediaElement.LoadedBehavior = MediaState.Manual;
                    myImages.Add((SandRibbonInterop.Video)image);
                }
            }
            return myImages;
        }
        private void transmitImageAltered(object sender, EventArgs e)
        {
            foreach (UIElement selectedImage in GetSelectedElements())
            {
                if (selectedImage is System.Windows.Controls.Image)
                {
                    var tag = ((System.Windows.Controls.Image)selectedImage).tag();
                    tag.privacy = privacy;
                    tag.zIndex = -1;
                    ((System.Windows.Controls.Image)selectedImage).tag(tag);
                    Commands.SendImage.Execute(new TargettedImage
                    {
                        author = Globals.me,
                        slide = currentSlide,
                        privacy = privacy,
                        target = target,
                        image = (System.Windows.Controls.Image)selectedImage
                    });
                }
                else if (selectedImage is AutoShape)
                    Commands.SendAutoShape.Execute(new TargettedAutoShape
               {
                   author = Globals.me,
                   slide = currentSlide,
                   privacy = privacy,
                   target = target,
                   autoshape = (SandRibbonInterop.AutoShape)selectedImage
               });
                else if (selectedImage is RenderedLiveWindow)
                {
                    var container = (RenderedLiveWindow)selectedImage;
                    var window = (Rectangle)(container.Rectangle);
                    var box = ((VisualBrush)window.Fill).Viewbox;
                    window.Height = container.Height;
                    window.Width = container.Width;
                    Commands.SendLiveWindow.Execute(new LiveWindowSetup
                    {
                        frame = window,
                        origin = box.TopLeft,
                        snapshotAtTimeOfCreation = window.Tag.ToString(),
                        target = new Point(InkCanvas.GetLeft(container), InkCanvas.GetTop(container)),
                        slide = currentSlide,
                        author = Globals.me
                    });
                }
                else if (selectedImage is Video)
                {
                    ((SandRibbonInterop.Video)selectedImage).Tag = ((SandRibbonInterop.Video)selectedImage).MediaElement.Tag;
                    var tag = ((SandRibbonInterop.Video)selectedImage).tag();
                    tag.privacy = privacy;
                    tag.zIndex = -1;
                    ((SandRibbonInterop.Video)selectedImage).tag(tag);

                    var srVideo = ((SandRibbonInterop.Video)selectedImage);
                    srVideo.X = InkCanvas.GetLeft(srVideo);
                    srVideo.Y = InkCanvas.GetTop(srVideo);
                    srVideo.VideoHeight = srVideo.MediaElement.NaturalVideoHeight;
                    srVideo.VideoWidth = srVideo.MediaElement.NaturalVideoWidth;
                    srVideo.Height = srVideo.ActualHeight;
                    srVideo.Width = srVideo.ActualWidth;
                    srVideo.MediaElement.LoadedBehavior = MediaState.Manual;
                    Commands.SendVideo.Execute(new TargettedVideo
                    {
                        author = Globals.me,
                        slide = currentSlide,
                        privacy = privacy,
                        target = target,
                        video = srVideo,
                        id = srVideo.tag().id
                    });
                }
            }
        }
        private void dirtyImage(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            doDirtySelection();
        }
        private void deleteSelectedImage(object sender, ExecutedRoutedEventArgs e)
        {
            doDirtySelection();
        }
        private void doDirtySelection()
        {
            foreach (UIElement selectedImage in GetSelectedElements())
            {
                var imageTag = ((FrameworkElement)selectedImage).Tag;
                var selectedElementPrivacy = imageTag == null ?
                    "public" :
                    JsonConvert.DeserializeObject<ImageInformation>(imageTag.ToString())
                        .isPrivate ? "private" : "public";
                if (selectedImage is System.Windows.Controls.Image)
                {
                    var image = (System.Windows.Controls.Image)selectedImage;
                    if (image.tag().privacy == "private") removePrivateRegion(image);
                    Commands.SendDirtyImage.Execute(new TargettedDirtyElement
                    {
                        identifier = ((System.Windows.Controls.Image)selectedImage).tag().id,
                        target = target,
                        privacy = ((System.Windows.Controls.Image)selectedImage).tag().privacy,
                        author = Globals.me,
                        slide = currentSlide
                    });
                }
                else if (selectedImage is RenderedLiveWindow)
                {
                    if (((Rectangle)(((RenderedLiveWindow)selectedImage).Rectangle)).Tag != null)
                    {
                        var rect = ((RenderedLiveWindow)selectedImage).Rectangle;
                        Commands.SendDirtyLiveWindow.Execute(
                            new TargettedDirtyElement
                            {
                                author = Globals.me,
                                identifier = (string)((Rectangle)rect).Tag,
                                target = target,
                                privacy = "private",
                                slide = currentSlide
                            });
                    }
                }
                else if (selectedImage is AutoShape)
                    Commands.SendDirtyAutoShape.Execute(new TargettedDirtyElement
                    {
                        author = Globals.me,
                        slide = currentSlide,
                        identifier = ((SandRibbonInterop.AutoShape)selectedImage).Tag.ToString(),
                        privacy = selectedElementPrivacy,
                        target = target
                    });
                else if (selectedImage is Video)
                    Commands.SendDirtyVideo.Execute(
                        new TargettedDirtyElement
                        {
                            author = Globals.me,
                            slide = currentSlide,
                            identifier = ((SandRibbonInterop.Video)selectedImage).MediaElement.Tag.ToString(),
                            privacy = selectedElementPrivacy,
                            target = target
                        });
            }
        }
        private void DugPublicSpace(LiveWindowSetup setup)
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
                var RLW = new RenderedLiveWindow()
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
        /*        #region Video
        private SandRibbonInterop.Video newVideo(System.Uri Source)
        {
            var MeTLVideo = new SandRibbonInterop.Video()
                {
                    VideoSource = Source,
                };
            return MeTLVideo;
        }
        #endregion
  */
        #region AutoShapes
        private void createNewAutoShape(object obj)
        {
            try
            {
                var paramPath = (AutoShape)obj;
                var newAutoShape = new AutoShape();
                newAutoShape.PathData = paramPath.PathData;
                newAutoShape.Foreground = paramPath.Foreground;
                newAutoShape.Background = paramPath.Background;
                newAutoShape.StrokeThickness = paramPath.StrokeThickness;
                newAutoShape.Height = paramPath.Height;
                newAutoShape.Width = paramPath.Width;
                Children.Add(newAutoShape);
                SetLeft(newAutoShape, 0);
                SetTop(newAutoShape, 0);
                tagAutoShape(newAutoShape, 1);
                Commands.SendAutoShape.Execute(new TargettedAutoShape
                   {
                       author = Globals.me,
                       slide = currentSlide,
                       privacy = privacy,
                       target = target,
                       autoshape = newAutoShape
                   });
            }
            catch (Exception ex)
            {//Don't do as I do, do as I say.  DON'T do this.
                MessageBox.Show("Error creating AutoShape: " + ex.Message);
            }
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
        private void addImageFromQuizSnapshot(string filename)
        {
            handleDrop(filename, new Point(10, 10), 1);
        }
        private void addResourceFromDisk(Action<IEnumerable<string>> withResources)
        {
            if (target == "presentationSpace" && canEdit && me != "projector")
            {
                var fileBrowser = new OpenFileDialog
                                                 {
                                                     InitialDirectory = "c:\\",
                                                     Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                                                     FilterIndex = 2,
                                                     RestoreDirectory = true
                                                 };
                var dialogResult = fileBrowser.ShowDialog();
                if (dialogResult == true)
                    withResources(fileBrowser.FileNames);
            }
        }
        public void dropVideoOnCanvas(string fileName, Point pos, int count)
        {
            FileType type = GetFileType(fileName);
            if (type == FileType.Video)
            {
                var placeHolderMe = new MediaElement
                {
                    Source = new Uri(fileName, UriKind.RelativeOrAbsolute),
                    Width = 200,
                    Height = 200,
                    LoadedBehavior = MediaState.Manual
                };
                var placeHolder = new SandRibbonInterop.Video { MediaElement = placeHolderMe, VideoSource = placeHolderMe.Source };
                InkCanvas.SetLeft(placeHolder, pos.X);
                InkCanvas.SetTop(placeHolder, pos.Y);
                Children.Add(placeHolder);
                var animationPulse = new DoubleAnimation
                                         {
                                             From = .3,
                                             To = 1,
                                             Duration = new Duration(TimeSpan.FromSeconds(1)),
                                             AutoReverse = true,
                                             RepeatBehavior = RepeatBehavior.Forever
                                         };
                placeHolder.BeginAnimation(OpacityProperty, animationPulse);

                var hostedFileName = ResourceUploader.uploadResource(currentSlide.ToString(), fileName);
                if (hostedFileName == "failed") return;
                var me = new MediaElement { Source = new Uri(hostedFileName, UriKind.Absolute), LoadedBehavior = MediaState.Manual };
                var video = new SandRibbonInterop.Video { MediaElement = me, VideoSource = me.Source, VideoHeight = me.NaturalVideoHeight, VideoWidth = me.NaturalVideoWidth };
                Children.Remove(placeHolder);
                InkCanvas.SetLeft(video, pos.X);
                InkCanvas.SetTop(video, pos.Y);
                video.tag(new ImageTag
                                  {
                                      author = Globals.me,
                                      id = string.Format("{0}:{1}:{2}", Globals.me, SandRibbonObjects.DateTimeFactory.Now(), count),
                                      privacy = privacy,
                                      zIndex = -1
                                  });
                UndoHistory.Queue(
                    () =>
                    {
                        Children.Remove(video);
                        Commands.SendDirtyImage.Execute(new TargettedDirtyElement
                                                            {
                                                                identifier = video.tag().id,
                                                                target = target,
                                                                privacy = video.tag().privacy,
                                                                author = video.tag().author,
                                                                slide = currentSlide
                                                            });
                    },
                    () =>
                    {
                    });
                video.MediaElement.LoadedBehavior = MediaState.Manual;
                video.VideoHeight = video.MediaElement.NaturalVideoHeight;
                video.VideoWidth = video.MediaElement.NaturalVideoWidth;
                Commands.SendVideo.Execute(new TargettedVideo
                {
                    author = Globals.me,
                    slide = currentSlide,
                    privacy = privacy,
                    target = target,
                    video = video,
                    X = InkCanvas.GetLeft(video),
                    Y = InkCanvas.GetTop(video),
                    Height = video.ActualHeight,
                    Width = video.ActualWidth,
                    id = video.tag().id
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
                    MessageBox.Show("The object you're trying to import is a video.  At present, MeTL does not support videos.");
                    return;
                    dropVideoOnCanvas(fileName, pos, count);
                    break;
            }
        }
        public void dropImageOnCanvas(string fileName, Point pos, int count)
        {
            System.Windows.Controls.Image image;
            try
            {
                image = createImageFromUri(new Uri(fileName, UriKind.RelativeOrAbsolute));
            }
            catch (Exception e)
            {
                MessageBox.Show("Sorry could not create an image from this file :" + fileName + "\n Error: " + e.Message);
                return;
            }

            // Add the image to the Media Panel
            InkCanvas.SetLeft(image, pos.X);
            InkCanvas.SetTop(image, pos.Y);
            Children.Add(image);
            var animationPulse = new DoubleAnimation
                                     {
                                         From = .3,
                                         To = 1,
                                         Duration = new Duration(TimeSpan.FromSeconds(1)),
                                         AutoReverse = true,
                                         RepeatBehavior = RepeatBehavior.Forever
                                     };
            image.BeginAnimation(OpacityProperty, animationPulse);
            string hostedFileName;
            if (!fileName.Contains("http"))
            {
                hostedFileName = ResourceUploader.uploadResource(currentSlide.ToString(), fileName);
                if (hostedFileName == "failed") return;
            }
            else
                hostedFileName = fileName;
            var uri = new Uri(hostedFileName, UriKind.Absolute);
            var hostedImage = new System.Windows.Controls.Image();
            try
            {
                var bitmap = new BitmapImage(uri);
                hostedImage.Source = bitmap;
            }
            catch (Exception e1)
            {
                MessageBox.Show("Cannot create image: " + e1.Message);
            }
            hostedImage.Height = image.Height;
            hostedImage.Width = image.Width;
            Children.Remove(image);
            InkCanvas.SetLeft(hostedImage, pos.X);
            InkCanvas.SetTop(hostedImage, pos.Y);
            hostedImage.tag(new ImageTag
                              {
                                  author = Globals.me,
                                  id = string.Format("{0}:{1}:{2}", Globals.me, SandRibbonObjects.DateTimeFactory.Now(), count),
                                  privacy = privacy,
                                  zIndex = -1
                              });
            UndoHistory.Queue(
                () =>
                {
                    Children.Remove(hostedImage);
                    Commands.SendDirtyImage.Execute(new TargettedDirtyElement
                                                        {
                                                            identifier = hostedImage.tag().id,
                                                            target = target,
                                                            privacy = hostedImage.tag().privacy,
                                                            author = hostedImage.tag().author,
                                                            slide = currentSlide
                                                        });
                },
                () =>
                {
                    AddImage(hostedImage);
                    Commands.SendImage.Execute(new TargettedImage
                                                   {
                                                       author = Globals.me,
                                                       slide = currentSlide,
                                                       privacy = privacy,
                                                       target = target,
                                                       image = hostedImage
                                                   });
                });
            Commands.SendImage.Execute(new TargettedImage
            {
                author = Globals.me,
                slide = currentSlide,
                privacy = privacy,
                target = target,
                image = hostedImage
            });
        }
        public void tagAutoShape(SandRibbonInterop.AutoShape autoshape, int count)
        {
            tagAutoShape(autoshape, Globals.me, count);
        }
        public void tagAutoShape(SandRibbonInterop.AutoShape autoshape, string author, int count)
        {
            var id = string.Format("{0}:{1}:{2}", author, SandRibbonObjects.DateTimeFactory.Now(), count);
            var imageInfo = new ImageInformation
            {
                Author = author,
                isPrivate = privacy.Equals("private"),
                Id = id
            };
            autoshape.Tag = JsonConvert.SerializeObject(imageInfo);
        }
        public static System.Windows.Controls.Image createImageFromUri(Uri uri)
        {
            var image = new System.Windows.Controls.Image();
            var jpgFrame = BitmapFrame.Create(uri);
            image.Source = jpgFrame;
            image.Height = jpgFrame.Height;
            image.Width = jpgFrame.Width;
            image.Stretch = Stretch.Uniform;
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
        private bool autoshapeExistsOnCanvas(SandRibbonInterop.AutoShape autoshape)
        {
            foreach (UIElement shape in Children)
                if (shape is SandRibbonInterop.AutoShape)
                    if (autoshapeCompare((SandRibbonInterop.AutoShape)shape, autoshape))
                        return true;
            return false;
        }
        private bool videoExistsOnCanvas(SandRibbonInterop.Video testVideo)
        {
            foreach (UIElement video in Children)
                if (video is SandRibbonInterop.Video)
                    if (videoCompare((SandRibbonInterop.Video)video, testVideo))
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
            if (!(System.Windows.Controls.Canvas.GetTop(currentImage) != System.Windows.Controls.Canvas.GetTop(image)))
                return false;
            if (!(System.Windows.Controls.Canvas.GetLeft(currentImage) != System.Windows.Controls.Canvas.GetLeft(image)))
                return false;
            if (image.Source.ToString() != currentImage.Source.ToString())
                return false;
            if (image.tag().id != currentImage.tag().id)
                return false;
            return true;
        }
        private static bool videoCompare(SandRibbonInterop.Video video, SandRibbonInterop.Video currentVideo)
        {
            if (!(System.Windows.Controls.Canvas.GetTop(currentVideo) != System.Windows.Controls.Canvas.GetTop(video)))
                return false;
            if (!(System.Windows.Controls.Canvas.GetLeft(currentVideo) != System.Windows.Controls.Canvas.GetLeft(video)))
                return false;
            if (video.VideoSource.ToString() != currentVideo.VideoSource.ToString())
                return false;
            if (video.tag().id != currentVideo.tag().id)
                return false;
            return true;
        }
        private static bool autoshapeCompare(SandRibbonInterop.AutoShape autoshape, SandRibbonInterop.AutoShape currentAutoshape)
        {
            if (!(System.Windows.Controls.Canvas.GetTop(currentAutoshape) != System.Windows.Controls.Canvas.GetTop(autoshape)))
                return false;
            if (!(System.Windows.Controls.Canvas.GetLeft(currentAutoshape) != System.Windows.Controls.Canvas.GetLeft(autoshape)))
                return false;
            //this next bit is ALMOST working.  When it gets converted back off the wire, it has some spaces between some parts,
            //which are not considered to be a perfect match.  As a result, comparing against path data isn't working, but the new
            //shape is working fine in the program.  Strange peculiarity of shapes, I guess.
            //For an example see:
            //currentAutoShape = {M47.7778,48.6667L198,48.6667 198,102C174.889,91.3334 157.111,79.7778 110.889,114.444 64.667,149.111 58.4444,130.444 47.7778,118.889z}
            //autoShape        = {M47.7778,48.6667L198,48.6667L198,102C174.889,91.3334,157.111,79.7778,110.889,114.444C64.667,149.111,58.4444,130.444,47.7778,118.889z}
            //diff             =                              *                       *               *               *              *               *

            if (autoshape.PathData.Figures.ToString() != currentAutoshape.PathData.Figures.ToString())
                return false;
            if (autoshape.Tag.ToString() != currentAutoshape.Tag.ToString())
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
                foreach (System.Windows.Controls.Image image in GetSelectedElements().ToList().Where(i =>
                    i is System.Windows.Controls.Image
                    && ((System.Windows.Controls.Image)i).tag().privacy != newPrivacy))
                {
                    var oldTag = ((System.Windows.Controls.Image)image).tag();
                    Commands.SendDirtyImage.Execute(new TargettedDirtyElement
                    {
                        identifier = ((System.Windows.Controls.Image)image).tag().id,
                        target = target,
                        privacy = ((System.Windows.Controls.Image)image).tag().privacy,
                        author = Globals.me,
                        slide = currentSlide
                    });
                    oldTag.privacy = newPrivacy;
                    ((System.Windows.Controls.Image)image).tag(oldTag);
                    Commands.SendImage.Execute(new TargettedImage
                    {
                        author = Globals.me,
                        slide = currentSlide,
                        privacy = newPrivacy,
                        target = target,
                        image = (System.Windows.Controls.Image)image
                    });
                }
            }
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
            /*
            Image.ParseInjectedStream(value, element => Image.Dispatcher.adopt((Action)delegate
                                        {
                                            foreach (var image in element.SelectElements<MeTLStanzas.Image>(true))
                                            {
                                                //Image.dropImageOnCanvas(image.source.ToString(), new Point { X = image.x, Y = image.y }, 1);
                                            }
                                                                                            }));**/
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
                                         {
                                             author = Globals.me,
                                             privacy = img.privacy,
                                             slide = Globals.slide,
                                             imageProperty = (System.Windows.Controls.Image)image,
                                             target = img.target
                                         }).ToString())
                    sb.Append(toString);
                sb.Append("</image>");
                return sb.ToString();
            }
        }
    }
    class ImageImpl : Image
    {
    }
}
