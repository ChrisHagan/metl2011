using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using Newtonsoft.Json;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonObjects;
using System.Windows.Media.Effects;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;
using MeTLLib.DataTypes;

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
        private Dictionary<string, List<MeTLLib.DataTypes.TargettedImage>> userImages;
        private Dictionary<string, List<MeTLLib.DataTypes.TargettedVideo>> userVideo;
        private Dictionary<string, bool> userVisibility;
        public Image()
        {
            userImages = new Dictionary<string, List<MeTLLib.DataTypes.TargettedImage>>();
            userVideo = new Dictionary<string, List<MeTLLib.DataTypes.TargettedVideo>>();
            userVisibility = new Dictionary<string, bool>();
            EditingMode = InkCanvasEditingMode.Select;
            Background = Brushes.Transparent;
            PreviewKeyDown += keyPressed;
            SelectionMoved += transmitImageAltered;
            SelectionMoving += dirtyImage;
            SelectionChanging += selectingImages;
            SelectionChanged += selectionChanged;
            SelectionResizing += dirtyImage;
            SelectionResized += transmitImageAltered;
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedImage>>(ReceiveImages));
            Commands.ReceiveVideo.RegisterCommandToDispatcher<TargettedVideo>(new DelegateCommand<MeTLLib.DataTypes.TargettedVideo>(ReceiveVideo));
            Commands.ReceiveAutoShape.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedAutoShape>(ReceiveAutoShape));
            Commands.ReceiveDirtyImage.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedDirtyElement>(ReceiveDirtyImage));
            Commands.ReceiveDirtyVideo.RegisterCommandToDispatcher<TargettedDirtyElement>(new DelegateCommand<MeTLLib.DataTypes.TargettedDirtyElement>(ReceiveDirtyVideo));
            Commands.ReceiveDirtyAutoShape.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedDirtyElement>(ReceiveDirtyAutoShape));
            Commands.AddAutoShape.RegisterCommand(new DelegateCommand<object>(createNewAutoShape));
            Commands.AddImage.RegisterCommand(new DelegateCommand<object>(addImageFromDisk));
            Commands.FileUpload.RegisterCommand(new DelegateCommand<object>(uploadFile));
            Commands.PlaceQuizSnapshot.RegisterCommand(new DelegateCommand<string>(addImageFromQuizSnapshot));
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
            Commands.ReceiveDirtyLiveWindow.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedDirtyElement>(ReceiveDirtyLiveWindow));
            Commands.DugPublicSpace.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.LiveWindowSetup>(DugPublicSpace));
            Commands.DeleteSelectedItems.RegisterCommand(new DelegateCommand<object>(deleteSelectedImages));
            Commands.MirrorVideo.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.VideoMirror.VideoMirrorInformation>(mirrorVideo));
            Commands.VideoMirrorRefreshRectangle.RegisterCommand(new DelegateCommand<string>(mirrorVideoRefresh));
            Commands.UserVisibility.RegisterCommand(new DelegateCommand<VisibilityInformation>(setUserVisibility));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(clearVisibilityDictionary));
        }

        private void clearVisibilityDictionary(object obj)
        {
            userImages.Clear();
            userVideo.Clear();
        }
        private void updateVisibility(VisibilityInformation info)
        {
            switch (info.user)
            {
                case "toggleTeacher":
                    {
                        userVisibility["Teacher"] = info.visible;
                        break;
                    }
                case "toggleMe":
                    {
                        userVisibility[Globals.me] = info.visible;
                        break;
                    }
                case "toggleStudents":
                    {
                        var keys = userVisibility.Keys.Where(k => k != "Teacher" && k != Globals.me).ToList();
                        foreach (var key in keys)
                            userVisibility[key] = info.visible;
                        break;
                    }
                default:
                    {
                        userVisibility[info.user] = info.visible;
                        break;
                    }
            }

        }
        private void setUserVisibility(VisibilityInformation info)
        {
            Dispatcher.adoptAsync(() =>
                          {

                              Children.Clear();
                              updateVisibility(info);
                              var visibleUsers =
                                  userVisibility.Keys.Where(u => userVisibility[u] == true).ToList();
                              var allVisibleImages = new List<MeTLLib.DataTypes.TargettedImage>();
                              var allVisibleVideos = new List<MeTLLib.DataTypes.TargettedVideo>();
                              foreach (var user in visibleUsers)
                              {
                                  if (userImages.ContainsKey(user))
                                      allVisibleImages.AddRange(userImages[user]);
                                  if (userVideo.ContainsKey(user))
                                      allVisibleVideos.AddRange(userVideo[user]);
                              }
                              ReceiveImages(allVisibleImages);
                              ReceiveVideos(allVisibleVideos);
                          });
        }

        private void deleteSelectedImages(object obj)
        {
            deleteImages();
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
            var numberOfImages = GetSelectedElements().Count;
            for (var i = 0; i < numberOfImages; i++)
            {
                if ((GetSelectedElements().ElementAt(i)).GetType().ToString() == "System.Windows.Controls.Image")
                {
                    var image = (System.Windows.Controls.Image)GetSelectedElements().ElementAt(i);
                    ApplyPrivacyStylingToElement(image, image.tag().privacy);
                    UndoHistory.Queue(
                        () =>
                        {
                            AddImage(image);
                            Commands.SendImage.ExecuteAsync(new TargettedImage
                            (currentSlide, image.tag().author, target, privacy, image));
                        },
                        () =>
                        {
                            Children.Remove(image);
                            Commands.SendDirtyImage.ExecuteAsync(new TargettedDirtyElement
                            (currentSlide, image.tag().author, target, image.tag().privacy, image.tag().id));
                        });

                    Commands.SendDirtyImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, image.tag().privacy, image.tag().id));
                }
                if ((GetSelectedElements().ElementAt(i)) is MeTLLib.DataTypes.AutoShape)
                {
                    var autoshape = (MeTLLib.DataTypes.AutoShape)GetSelectedElements().ElementAt(i);
                    Commands.SendDirtyAutoShape.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, privacy, autoshape.Tag.ToString()));
                }
                if ((GetSelectedElements().ElementAt(i)) is MeTLLib.DataTypes.Video)
                {
                    var video = (MeTLLib.DataTypes.Video)GetSelectedElements().ElementAt(i);
                    Commands.SendDirtyVideo.ExecuteAsync(new TargettedDirtyElement(currentSlide, Globals.me, target, privacy, video.Tag.ToString()));
                    Commands.MirrorVideo.ExecuteAsync(new MeTLLib.DataTypes.VideoMirror.VideoMirrorInformation(video.tag().id, null));
                }
                if ((GetSelectedElements().ElementAt(i)) is MeTLLib.DataTypes.RenderedLiveWindow)
                {
                    var liveWindow = (MeTLLib.DataTypes.RenderedLiveWindow)GetSelectedElements().ElementAt(i);
                    Commands.SendDirtyLiveWindow.ExecuteAsync(new TargettedDirtyElement
                    (currentSlide, Globals.me, target, privacy, ((Rectangle)((MeTLLib.DataTypes.RenderedLiveWindow)liveWindow).Rectangle).Tag.ToString()));
                }
            }
        }

        protected override void CanEditChanged()
        {
            canEdit = base.canEdit;
            if (privacy == "private") canEdit = true;
        }
        public void ReceiveImages(IEnumerable<MeTLLib.DataTypes.TargettedImage> images)
        {
            var safeImages = images.Where(shouldDisplay).ToList();
            foreach (var image in safeImages)
                ReceiveImage(image);
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
            Dispatcher.adoptAsync(delegate
            {
                if (image.target == target)
                {
                    var author = image.author == Globals.conversationDetails.Author ? "Teacher" : image.author;
                    Commands.ReceiveAuthor.ExecuteAsync(author);
                    if (!userVisibility.ContainsKey(author))
                        userVisibility.Add(author, true);
                    if (!userImages.ContainsKey(author))
                        userImages.Add(author, new List<MeTLLib.DataTypes.TargettedImage>());
                    if (!userImages[author].Contains(image))
                        userImages[author].Add(image);
                }
                AddImage(image.image);
            });
        }
        public void ReceiveVideo(MeTLLib.DataTypes.TargettedVideo video)
        {
            Dispatcher.adoptAsync(delegate
            {
                video.video.MediaElement.LoadedBehavior = MediaState.Manual;
                video.video.MediaElement.ScrubbingEnabled = true;
                if (video.target == target)
                {
                    var author = video.author == Globals.conversationDetails.Author ? "Teacher" : video.author;
                    Commands.ReceiveAuthor.ExecuteAsync(author);
                    if (!userVisibility.ContainsKey(author))
                        userVisibility.Add(author, true);
                    if (!userVideo.ContainsKey(author))
                        userVideo.Add(author, new List<MeTLLib.DataTypes.TargettedVideo>());
                    if (!userVideo[author].Contains(video))
                        userVideo[author].Add(video);
                }
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
                var videoClone = new SandRibbonInterop.VideoMirror();
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
            if (!Globals.isAuthor || Globals.conversationDetails.Permissions == MeTLLib.DataTypes.Permissions.LECTURE_PERMISSIONS) return;
            if (privacy != "private")
            {
                RemovePrivacyStylingFromElement(element);
                return;
            }
            element.Effect = new DropShadowEffect { BlurRadius = 50, Color = Colors.Black, ShadowDepth = 0, Opacity = 1 };
            element.Opacity = 0.7;
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
        public void ReceiveDirtyImage(MeTLLib.DataTypes.TargettedDirtyElement element)
        {
            if (!(element.target.Equals(target))) return;
            if (!(element.slide == currentSlide)) return;
            var author = element.author == Globals.conversationDetails.Author ? "Teacher" : element.author;
            if (userImages.ContainsKey(author) && element.target == target)
            {
                var dirtyImage = userImages[author].Where(i => i.id == element.identifier).FirstOrDefault();
                if (dirtyImage != null)
                    userImages[author].Remove(dirtyImage);
            }
            doDirtyImage(element.identifier);
        }
        public void ReceiveDirtyVideo(MeTLLib.DataTypes.TargettedDirtyElement element)
        {
            if (!(element.target.Equals(target))) return;
            if (!(element.slide == currentSlide)) return;
            var author = element.author == Globals.conversationDetails.Author ? "Teacher" : element.author;
            if (userVideo.ContainsKey(author) && element.target == target)
            {
                var dirtyImage = userVideo[author].Where(i => i.id == element.identifier).FirstOrDefault();
                if (dirtyImage != null)
                    userVideo[author].Remove(dirtyImage);
            }
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
                if (image.tag().isBackground)
                    Background = new VisualBrush(image);
                else if (!imageExistsOnCanvas(image))
                {
                    image.Margin = new Thickness(PADDING, PADDING, PADDING, PADDING);
                    Children.Add(image);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Sorry, your image could not be imported");
            }
        }
        public void ReceiveAutoShape(MeTLLib.DataTypes.TargettedAutoShape autoshape)
        {
            return;
        }
        public void ReceiveDirtyAutoShape(MeTLLib.DataTypes.TargettedDirtyElement autoshape)
        {
            return;
        }
        public void AddAutoShape(MeTLLib.DataTypes.TargettedAutoShape autoshape)
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
                    image.tag(new MeTLLib.DataTypes.ImageTag(Globals.me, privacy, string.Format("{0}:{1}:{2}", Globals.me, SandRibbonObjects.DateTimeFactory.Now(), 1), false, -1));
                    InkCanvas.SetLeft(image, 15);
                    InkCanvas.SetTop(image, 15);
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
            var listToCut = new List<MeTLLib.DataTypes.TargettedDirtyElement>();

            foreach (var element in GetSelectedElements().Where(e => e is System.Windows.Controls.Image))
            {
                var image = (System.Windows.Controls.Image)element;
                ApplyPrivacyStylingToElement(image, image.tag().privacy);
                Clipboard.SetImage((BitmapSource)image.Source);
                listToCut.Add(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, image.tag().privacy, image.tag().id));
            }
            foreach (var element in listToCut)
                Commands.SendDirtyImage.ExecuteAsync(element);
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
            addAdorner();
        }

        private void addAdorner()
        {
            var selectedElements = GetSelectedElements();
            if (selectedElements.Count == 0)
            {
                ClearAdorners();
                return;
            }
            var publicElements = selectedElements.Where(i => (((i is System.Windows.Controls.Image) && ((System.Windows.Controls.Image)i).tag().privacy.ToLower() == "public")) || ((i is Video) && ((Video)i).tag().privacy.ToLower() == "public")).ToList();
            string privacyChoice;
            if (publicElements.Count == 0)
                privacyChoice = "show";
            else if (publicElements.Count == selectedElements.Count)
                privacyChoice = "hide";
            else
                privacyChoice = "both";
            Commands.AddPrivacyToggleButton.ExecuteAsync(new PrivacyToggleButton.PrivacyToggleButtonInfo(privacyChoice, GetSelectionBounds()));
            /*var adorner = GetAdorner();
            AdornerLayer.GetAdornerLayer(adorner).Add(new UIAdorner(adorner, new PrivacyToggleButton(privacyChoice, GetSelectionBounds())));
        */
        }
        private void transmitImageAltered(object sender, EventArgs e)
        {
            foreach (UIElement selectedImage in GetSelectedElements())
            {
                if (selectedImage is System.Windows.Controls.Image)
                {
                    var newImage = (System.Windows.Controls.Image)selectedImage;
                    newImage.UpdateLayout();
                    Commands.SendImage.ExecuteAsync(new TargettedImage(currentSlide, Globals.me, target, newImage.tag().privacy, newImage));
                    /*selectedImage.UpdateLayout();
                    var selectedImageLeft = InkCanvas.GetLeft((System.Windows.Controls.Image)selectedImage);
                    var selectedImageTop = InkCanvas.GetTop((System.Windows.Controls.Image)selectedImage);
                    var newImage = new System.Windows.Controls.Image
                                       {
                                           Height = ((System.Windows.Controls.Image) selectedImage).ActualHeight,
                                           Width = ((System.Windows.Controls.Image) selectedImage).Width,
                                           Source = (ImageSource) new ImageSourceConverter().ConvertFrom(SandRibbonInterop.LocalCache.ResourceCache.RemoteSource( new Uri( ((System.Windows.Controls.Image) selectedImage).Source. ToString(), UriKind.Relative)))
                                       };
                    InkCanvas.SetLeft(newImage, selectedImageLeft);
                    InkCanvas.SetTop(newImage, selectedImageTop);
                    var tag = ((System.Windows.Controls.Image)selectedImage).tag();
                    tag.zIndex = -1;
                    newImage.tag(tag);
                    Commands.SendImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedImage(currentSlide,Globals.me,target,((System.Windows.Controls.Image)selectedImage).tag().privacy,(System.Windows.Controls.Image)selectedImage));
                */
                }
                else if (selectedImage is MeTLLib.DataTypes.AutoShape)
                    Commands.SendAutoShape.ExecuteAsync(new MeTLLib.DataTypes.TargettedAutoShape(currentSlide, Globals.me, target, privacy, (MeTLLib.DataTypes.AutoShape)selectedImage));
                else if (selectedImage is MeTLLib.DataTypes.RenderedLiveWindow)
                {
                    var container = (MeTLLib.DataTypes.RenderedLiveWindow)selectedImage;
                    var window = (Rectangle)(container.Rectangle);
                    var box = ((VisualBrush)window.Fill).Viewbox;
                    window.Height = container.Height;
                    window.Width = container.Width;
                    Commands.SendLiveWindow.ExecuteAsync(new MeTLLib.DataTypes.LiveWindowSetup(currentSlide, Globals.me, window, box.TopLeft, new Point(InkCanvas.GetLeft(container), InkCanvas.GetTop(container)), window.Tag.ToString()));
                }
                else if (selectedImage is MeTLLib.DataTypes.Video)
                {
                    var srVideo = (MeTLLib.DataTypes.Video)selectedImage;
                    srVideo.UpdateLayout();
                    srVideo.X = InkCanvas.GetLeft(srVideo);
                    srVideo.Y = InkCanvas.GetTop(srVideo);
                    Commands.SendVideo.ExecuteAsync(new TargettedVideo(currentSlide, Globals.me, target, srVideo.tag().privacy, srVideo));
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
            ClearAdorners();
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

                    ApplyPrivacyStylingToElement(image, image.tag().privacy);
                    Commands.SendDirtyImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, ((System.Windows.Controls.Image)selectedImage).tag().privacy, ((System.Windows.Controls.Image)selectedImage).tag().id));
                }
                else if (selectedImage is MeTLLib.DataTypes.RenderedLiveWindow)
                {
                    if (((Rectangle)(((MeTLLib.DataTypes.RenderedLiveWindow)selectedImage).Rectangle)).Tag != null)
                    {
                        var rect = ((MeTLLib.DataTypes.RenderedLiveWindow)selectedImage).Rectangle;
                        Commands.SendDirtyLiveWindow.ExecuteAsync(
                            new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, "private", (string)((Rectangle)rect).Tag));
                    }
                }
                else if (selectedImage is MeTLLib.DataTypes.AutoShape)
                    Commands.SendDirtyAutoShape.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, selectedElementPrivacy, ((MeTLLib.DataTypes.AutoShape)selectedImage).Tag.ToString()));
                else if (selectedImage is MeTLLib.DataTypes.Video)
                {
                    Commands.MirrorVideo.ExecuteAsync(new MeTLLib.DataTypes.VideoMirror.VideoMirrorInformation(((MeTLLib.DataTypes.Video)selectedImage).tag().id, null));
                    Commands.SendDirtyVideo.ExecuteAsync(
                        new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, Globals.me, target, selectedElementPrivacy, ((MeTLLib.DataTypes.Video)selectedImage).MediaElement.Tag.ToString()));
                }
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

        #region AutoShapes
        private void createNewAutoShape(object obj)
        {
            try
            {
                var paramPath = (MeTLLib.DataTypes.AutoShape)obj;
                var newAutoShape = new MeTLLib.DataTypes.AutoShape();
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
                Commands.SendAutoShape.ExecuteAsync(new MeTLLib.DataTypes.TargettedAutoShape(currentSlide, Globals.me, target, privacy, newAutoShape));
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
        private void uploadFile(object _obj)
        {
            addResourceFromDisk((files) =>
                                    {
                                        foreach (var file in files)
                                        {
                                            uploadFileForUse(file);
                                        }
                                    });
        }
        private void addImageFromQuizSnapshot(string filename)
        {
            if (target == "presentationSpace" && me != "projector")
                handleDrop(filename, new Point(200, 100), 1);
        }
        private void addResourceFromDisk(Action<IEnumerable<string>> withResources)
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
                                                 Filter = "Image files(*.jpeg;*.gif;*.bmp;*.jpg;*.png)|*.jpeg;*.gif;*.bmp;*.jpg;*.png|All files (*.*)|*.*",
                                                 FilterIndex = 1,
                                                 RestoreDirectory = true
                                             };
                var dialogResult = fileBrowser.ShowDialog(Window.GetWindow(this));
                if (dialogResult == true)
                    withResources(fileBrowser.FileNames);
            }
        }
        public void dropVideoOnCanvas(string fileName, Point pos, int count)
        {
            FileType type = GetFileType(fileName);
            if (type == FileType.Video)
            {
                Dispatcher.adopt(() =>
                {
                    var placeHolderMe = new MediaElement
                    {
                        Source = new Uri(fileName, UriKind.Relative),
                        Width = 200,
                        Height = 200,
                        LoadedBehavior = MediaState.Manual
                    };
                    var placeHolder = new Video { MediaElement = placeHolderMe, VideoSource = placeHolderMe.Source };
                    InkCanvas.SetLeft(placeHolder, pos.X);
                    InkCanvas.SetTop(placeHolder, pos.Y);
                    //Children.Add(placeHolder);
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
                                          id = string.Format("{0}:{1}:{2}", Globals.me, SandRibbonObjects.DateTimeFactory.Now(), count),
                                          privacy = privacy,
                                          zIndex = -1
                                      });
                    MeTLLib.ClientFactory.Connection().UploadAndSendVideo(new MeTLStanzas.LocalVideoInformation
                    (currentSlide, Globals.me, target, privacy, placeHolder, fileName, false));
                    //Children.Remove(placeHolder);
                });
/*
                var hostedFileName = ResourceUploader.uploadResource(currentSlide.ToString(), fileName);
                if (hostedFileName == "failed") return;
                Children.Remove(placeHolder);
                InkCanvas.SetLeft(video, pos.X);
                InkCanvas.SetTop(video, pos.Y);
                UndoHistory.Queue(
                    () =>
                    {
                        Children.Remove(video);
                        Commands.SendDirtyImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement
                        (currentSlide, video.tag().author, target, video.tag().privacy, video.tag().id));
                    },
                    () =>
                    {
                    });
                video.MediaElement.LoadedBehavior = MediaState.Manual;
                video.VideoHeight = video.MediaElement.NaturalVideoHeight;
                video.VideoWidth = video.MediaElement.NaturalVideoWidth;
                var tv = new MeTLLib.DataTypes.TargettedVideo
                (currentSlide, Globals.me, target, privacy, video);
                tv.X = InkCanvas.GetLeft(video);
                tv.Y = InkCanvas.GetTop(video);
                tv.Height = video.ActualHeight;
                tv.Width = video.ActualWidth;
                tv.id = video.tag().id;
                Commands.SendVideo.ExecuteAsync(tv);
                */
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
                    dropVideoOnCanvas(fileName, pos, count);
                    //MessageBox.Show("The object you're trying to import is a video.  At present, MeTL does not support videos.");
                    //return;
                    break;
                default:
                    uploadFileForUse(fileName);
                    break;
            }
        }

        private void uploadFileForUse(string unMangledFilename)
        {
            string filename = unMangledFilename + ".MeTLFileUpload";
            File.Copy(unMangledFilename, filename);
            MeTLLib.ClientFactory.Connection().UploadAndSendFile(new MeTLStanzas.LocalFileInformation(currentSlide,Globals.me,target,"public",filename,Path.GetFileNameWithoutExtension(filename),false,new System.IO.FileInfo(filename).Length,DateTimeFactory.Now().Ticks.ToString()));
            File.Delete(filename);
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
    
                MeTLLib.ClientFactory.Connection().UploadAndSendImage(new MeTLStanzas.LocalImageInformation(currentSlide,Globals.me,target,privacy,image,fileName,false));
                //hostedFileName = ResourceUploader.uploadResource(currentSlide.ToString(), fileName);
                //if (hostedFileName == "failed") return;
                Children.Remove(image);
      /*        UndoHistory.Queue(
                () =>
                {
                    Children.Remove(hostedImage);
                    Commands.SendDirtyImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement
                    (currentSlide,hostedImage.tag().author,target,hostedImage.tag().privacy,hostedImage.tag().id));
                },
                () =>
                {
                    AddImage(hostedImage);
                    Commands.SendImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedImage
                    (currentSlide,Globals.me,target,privacy,hostedImage));
                });*/
            }
            else {

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
            hostedImage.tag(new MeTLLib.DataTypes.ImageTag
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
                    Commands.SendDirtyImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement
                    (currentSlide,hostedImage.tag().author,target,hostedImage.tag().privacy,hostedImage.tag().id));
                },
                () =>
                {
                    AddImage(hostedImage);
                    Commands.SendImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedImage
                    (currentSlide,Globals.me,target,privacy,hostedImage));
                });
            Commands.SendImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedImage
            (currentSlide,Globals.me,target,privacy,hostedImage));
        }
        }
        public void tagAutoShape(MeTLLib.DataTypes.AutoShape autoshape, int count)
        {
            tagAutoShape(autoshape, Globals.me, count);
        }
        public void tagAutoShape(MeTLLib.DataTypes.AutoShape autoshape, string author, int count)
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
        private bool autoshapeExistsOnCanvas(MeTLLib.DataTypes.AutoShape autoshape)
        {
            foreach (UIElement shape in Children)
                if (shape is MeTLLib.DataTypes.AutoShape)
                    if (autoshapeCompare((MeTLLib.DataTypes.AutoShape)shape, autoshape))
                        return true;
            return false;
        }
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
        private static bool videoCompare(MeTLLib.DataTypes.Video video, MeTLLib.DataTypes.Video currentVideo)
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
        private static bool autoshapeCompare(MeTLLib.DataTypes.AutoShape autoshape, MeTLLib.DataTypes.AutoShape currentAutoshape)
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
                    Commands.SendDirtyImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement
                    (currentSlide, Globals.me, target, ((System.Windows.Controls.Image)image).tag().privacy, ((System.Windows.Controls.Image)image).tag().id));
                    oldTag.privacy = newPrivacy;
                    ((System.Windows.Controls.Image)image).tag(oldTag);
                    Commands.SendImage.ExecuteAsync(new MeTLLib.DataTypes.TargettedImage
                    (currentSlide, Globals.me, target, newPrivacy, (System.Windows.Controls.Image)image));
                }
                foreach (MeTLLib.DataTypes.Video video in GetSelectedElements().ToList().Where(i =>
                    i is MeTLLib.DataTypes.Video
                    && ((MeTLLib.DataTypes.Video)i).tag().privacy != newPrivacy))
                {
                    var oldTag = ((MeTLLib.DataTypes.Video)video).tag();
                    Commands.SendDirtyVideo.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement
                    (currentSlide, Globals.me, target, ((MeTLLib.DataTypes.Video)video).tag().privacy, ((MeTLLib.DataTypes.Video)video).tag().id));
                    oldTag.privacy = newPrivacy;
                    ((MeTLLib.DataTypes.Video)video).tag(oldTag);
                    Commands.SendVideo.ExecuteAsync(new MeTLLib.DataTypes.TargettedVideo
                    (currentSlide, Globals.me, target, newPrivacy, (MeTLLib.DataTypes.Video)video));
                }
            }
            Select(new List<UIElement>());
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
}
