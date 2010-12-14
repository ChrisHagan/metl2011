using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Windows.Ink;
using SandRibbon.Providers;
using SandRibbon.Utils;
using System.Windows.Media;
using SandRibbonObjects;
using System.Windows.Media.Effects;
using System.Threading;
using SandRibbon.Components.Utility;

namespace SandRibbon.Components.Canvas
{
    public class MasterCanvas : InkCanvas
    {
        private static readonly int PADDING = 5;
        public string target { get; set; }
        public MasterCanvas(){
            DragOver += ImageDragOver;
            Drop += ImagesDrop;
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Loaded += (_sender, _args) => this.Dispatcher.adoptAsync(delegate
            {
                if (target == null)
                    target = (string)FindResource("target");
            });
            Commands.MoveTo.RegisterCommandToDispatcher(new DelegateCommand<object>(MoveTo));
            Commands.DoWithCurrentSelection.RegisterCommand(new DelegateCommand<Action<SelectedIdentity>>(DoWithCurrentSelection));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<string>(SetPrivacyOfItems));
            Commands.PreParserAvailable.RegisterCommandToDispatcher(new DelegateCommand<PreParser>(PreParserAvailable));
            Commands.SetInkCanvasMode.RegisterCommandToDispatcher(new DelegateCommand<string>(SetInkCanvasMode));
            Commands.SetLayer.RegisterCommandToDispatcher(new DelegateCommand<string>(SetLayer));
            setupForInk();
            setupForImages();
        }
        private void SetLayer(string layer) {
            switch (layer) { 
                case "Sketch":
                    EditingMode = InkCanvasEditingMode.Ink;
                break;
                case "Text":
                    EditingMode = InkCanvasEditingMode.None;
                break;
                case "Image":
                    EditingMode = InkCanvasEditingMode.None;
                break;
            }
        }
        private void SetInkCanvasMode(string mode) {
            EditingMode = (InkCanvasEditingMode)Enum.Parse(typeof(InkCanvasEditingMode), mode);
        }
        private void setupForInk()
        {
            StrokeCollected += singleStrokeCollected;
            SelectionChanging += selectingStrokes;
            StrokeErasing += erasingStrokes;
            SelectionMoving += dirtySelectedRegions;
            SelectionMoved += transmitSelectionAltered;
            SelectionResizing += dirtySelectedRegions;
            SelectionResized += transmitSelectionAltered;
            DefaultDrawingAttributesReplaced += announceDrawingAttributesChanged;
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, deleteSelectedStrokes));
            Commands.ActualChangePenSize.RegisterCommand(new DelegateCommand<double>(penSize =>
            {
                var newAttributes = DefaultDrawingAttributes.Clone();
                newAttributes.FitToCurve = true;
                newAttributes.IgnorePressure = false;
                newAttributes.Width = penSize;
                newAttributes.Height = penSize;
                if (newAttributes.Height > DrawingAttributes.MinHeight && newAttributes.Height < DrawingAttributes.MaxHeight
                    && newAttributes.Width > DrawingAttributes.MinWidth && newAttributes.Width < DrawingAttributes.MaxWidth)
                    DefaultDrawingAttributes = newAttributes;
            }));
            Commands.IncreasePenSize.RegisterCommand(new DelegateCommand<object>(_obj =>
            {
                var newAttributes = DefaultDrawingAttributes.Clone();
                newAttributes.FitToCurve = true;
                newAttributes.IgnorePressure = false;
                newAttributes.Width += 5;
                newAttributes.Height += 5;
                DefaultDrawingAttributes = newAttributes;
            }));
            Commands.DecreasePenSize.RegisterCommand(new DelegateCommand<object>(_obj =>
             {
                 if ((DefaultDrawingAttributes.Width - 0.5) <= 0) return;
                 var newAttributes = DefaultDrawingAttributes.Clone();
                 newAttributes.FitToCurve = true;
                 newAttributes.IgnorePressure = false;
                 newAttributes.Width -= .5;
                 newAttributes.Height -= .5;
                 DefaultDrawingAttributes = newAttributes;
             }));
            Commands.RestorePenSize.RegisterCommand(new DelegateCommand<object>(_obj =>
            {
                var newAttributes = DefaultDrawingAttributes.Clone();
                newAttributes.FitToCurve = true;
                newAttributes.IgnorePressure = false;
                DefaultDrawingAttributes = newAttributes;
            }));
            Commands.ActualSetDrawingAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>(attributes =>
             {
                 Dispatcher.adoptAsync(() =>
                 DefaultDrawingAttributes = attributes);
             }));
            Commands.ToggleHighlighterMode.RegisterCommand(new DelegateCommand<object>(_obj =>
            {
                var newAttributes = DefaultDrawingAttributes.Clone();
                newAttributes.FitToCurve = true;
                newAttributes.IgnorePressure = false;
                newAttributes.IsHighlighter = !newAttributes.IsHighlighter;
                DefaultDrawingAttributes = newAttributes;
            }));
            Commands.SetHighlighterMode.RegisterCommand(new DelegateCommand<bool>(newIsHighlighter =>
            {
                var newAttributes = DefaultDrawingAttributes.Clone();
                newAttributes.IsHighlighter = newIsHighlighter;
                newAttributes.FitToCurve = true;
                newAttributes.IgnorePressure = false;
                DefaultDrawingAttributes = newAttributes;
            }));
            Commands.UpdateCursor.RegisterCommand(new DelegateCommand<Cursor>(UpdateCursor));
            Commands.SetPenColor.RegisterCommand(new DelegateCommand<object>(SetPenColor));
            Commands.ReceiveStroke.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.TargettedStroke>((stroke) => ReceiveStrokes(new[] { stroke })));
            Commands.ReceiveStrokes.RegisterCommandToDispatcher(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedStroke>>(ReceiveStrokes));
            Commands.SetPrivacyOfItems.RegisterCommandToDispatcher(new DelegateCommand<string>(SetPrivacyOfItems));
            Commands.ReceiveDirtyStrokes.RegisterCommandToDispatcher(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedDirtyElement>>(ReceiveDirtyStrokes));
            Commands.DeleteSelectedItems.RegisterCommandToDispatcher(new DelegateCommand<object>(DeleteSelectedItems));
        }
        private void setupForImages() { 
        }
        private void DeleteSelectedItems(object _arg){
            foreach (var stroke in GetSelectedStrokes())
                doMyStrokeRemoved(stroke);
        }
        private void SetPrivacyOfItems(string newPrivacy)
        {
            foreach (var stroke in GetSelectedStrokes())
            {
                var t = stroke.tag();
                t.privacy = newPrivacy;
                stroke.tag(t);
            }
            foreach (var element in GetSelectedElements())
            {
                var t = element.tag();
                t.privacy = newPrivacy;
                element.tag(t);
            }
        }
        private void UpdateCursor(Cursor cursor)
        {
            UseCustomCursor = true;
            Cursor = cursor;
        }
        public void SetPenColor(object colorObj){
            var newAttributes = DefaultDrawingAttributes.Clone();
            if (colorObj is Color)
                newAttributes.Color = (Color)colorObj;
            else if (colorObj is string)
                newAttributes.Color = ColorLookup.ColorOf((string)colorObj);
            newAttributes.FitToCurve = true;
            newAttributes.IgnorePressure = false;
            DefaultDrawingAttributes = newAttributes;
        }
        private void announceDrawingAttributesChanged(object sender, DrawingAttributesReplacedEventArgs e)
        {
            Commands.ActualReportDrawingAttributes.ExecuteAsync(this.DefaultDrawingAttributes);
        }
        private void UpdateConversationDetails(ConversationDetails details) {
            Strokes.Clear();
            Children.Clear();
            Commands.SetPrivacy.Execute(Globals.isAuthor ? "public" : "private");
        }
        private void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            ReceiveStrokes(parser.ink);
            ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                doText(text);
            foreach (var video in parser.videos)
            {
                var srVideo = ((MeTLLib.DataTypes.TargettedVideo)video.Value).video;
                srVideo.VideoWidth = srVideo.MediaElement.NaturalVideoWidth;
                srVideo.VideoHeight = srVideo.MediaElement.NaturalVideoHeight;
                srVideo.MediaElement.LoadedBehavior = MediaState.Manual;
                srVideo.MediaElement.ScrubbingEnabled = true;
                AddVideo(srVideo);
            } 
        }
        public void doText(MeTLLib.DataTypes.TargettedTextBox targettedBox)
        {
            var author = targettedBox.author == Globals.conversationDetails.Author ? "Teacher" : targettedBox.author;
            if (targettedBox.target != target) return;
            if (targettedBox.slide == Globals.location.currentSlide &&
                (targettedBox.privacy == "public" || targettedBox.author == Globals.me))
            {
                var box = targettedBox.box;
                removeDoomedTextBoxes(targettedBox);
                Children.Add(applyDefaultAttributes(box));
                if (!(targettedBox.author == Globals.me))
                    box.Focusable = false;
                ApplyPrivacyStylingToElement(box, targettedBox.privacy);
            }
        }
        private void removeDoomedTextBoxes(MeTLLib.DataTypes.TargettedTextBox targettedBox)
        {
            var box = targettedBox.box;
            var doomedChildren = new List<FrameworkElement>();
            foreach (var child in Children)
            {
                if (child is TextBox)
                    if (((TextBox)child).tag().id.Equals(box.tag().id))
                        doomedChildren.Add((FrameworkElement)child);
            }
            foreach (var child in doomedChildren)
                Children.Remove(child);
        }
        private TextBox applyDefaultAttributes(TextBox box)
        {
            box.AcceptsReturn = true;
            box.TextWrapping = TextWrapping.WrapWithOverflow;
            box.GotFocus += textboxGotFocus;
            box.LostFocus += textboxLostFocus;
            box.TextChanged += SendNewText;
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.Focusable = true;
            return box;
        }
        private void textboxLostFocus(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            var currentTag = box.tag();
            if (currentTag.privacy != Globals.privacy)
            {
                Commands.SendDirtyText.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement(Globals.location.currentSlide, Globals.me, target, currentTag.privacy, currentTag.id));
                currentTag.privacy = Globals.privacy;
                box.tag(currentTag);
                Commands.SendTextBox.ExecuteAsync(new MeTLLib.DataTypes.TargettedTextBox(Globals.location.currentSlide, Globals.me, target, currentTag.privacy, box));
            }
            if (box.Text.Length == 0)
                Children.Remove(box);
            else
                setAppropriatePrivacyHalo(box);
        }
        private void setAppropriatePrivacyHalo(TextBox box)
        {
            if (!Children.Contains(box)) return;
            ApplyPrivacyStylingToElement(box, Globals.privacy);
        }
        private void textboxGotFocus(object sender, RoutedEventArgs e)
        {
            updateTools();
        }
        private void updateTools()
        {
            bool strikethrough = false;
            bool underline = false;
            var focussedBox = (TextBox)FocusManager.GetFocusedElement(this);
            if (focussedBox == null) return;
            if (focussedBox.TextDecorations.Count > 0)
            {
                strikethrough = focussedBox.TextDecorations.First().Location.ToString().ToLower() == "strikethrough";
                underline = focussedBox.TextDecorations.First().Location.ToString().ToLower() == "underline";
            }
            var info = new TextInformation
                           {
                               family = focussedBox.FontFamily,
                               size = focussedBox.FontSize,
                               bold = focussedBox.FontWeight == FontWeights.Bold,
                               italics = focussedBox.FontStyle == FontStyles.Italic,
                               strikethrough = strikethrough,
                               underline = underline

                           };
            Commands.TextboxFocused.ExecuteAsync(info);
        }
        public static Timer typingTimer = null;
        private void SendNewText(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            ApplyPrivacyStylingToElement(box, box.tag().privacy);
            box.Height = Double.NaN;
            if (typingTimer == null)
            {
                typingTimer = new Timer(delegate
                {
                    Dispatcher.adoptAsync(delegate
                                                    {
                                                        sendText((TextBox)sender);
                                                        typingTimer = null;
                                                    });
                }, null, 600, Timeout.Infinite);
            }
            else
            {
                GlobalTimers.resetSyncTimer();
                typingTimer.Change(600, Timeout.Infinite);
            }
        }
        public void sendText(TextBox box)
        {
            sendText(box, Globals.privacy);
        }
        public void sendText(TextBox box, string intendedPrivacy)
        {
            UndoHistory.Queue(
            () =>
            {
                dirtyTextBoxWithoutHistory(box);
            },
            () =>
            {
                sendText(box);
            });
            GlobalTimers.resetSyncTimer();
            sendTextWithoutHistory(box, intendedPrivacy);
        }
        private void dirtyTextBoxWithoutHistory(TextBox box)
        {
            RemovePrivacyStylingFromElement(box);
            Commands.SendDirtyText.ExecuteAsync(new MeTLLib.DataTypes.TargettedDirtyElement(Globals.location.currentSlide, Globals.me, target, box.tag().privacy, box.tag().id));
        }
        private void sendTextWithoutHistory(TextBox box, string thisPrivacy)
        {
            RemovePrivacyStylingFromElement(box);
            if (box.tag().privacy != Globals.privacy)
                dirtyTextBoxWithoutHistory(box);
            var oldTextTag = box.tag();
            var newTextTag = new MeTLLib.DataTypes.TextTag(oldTextTag.author, thisPrivacy, oldTextTag.id);
            box.tag(newTextTag);
            Commands.SendTextBox.ExecuteAsync(new MeTLLib.DataTypes.TargettedTextBox(Globals.location.currentSlide, Globals.me, target, thisPrivacy, box));
        }
        private void ReceiveImage(MeTLLib.DataTypes.TargettedImage image)
        {
            AddImage(image.image);
        }
        public void ReceiveImages(IEnumerable<MeTLLib.DataTypes.TargettedImage> images)
        {
            var safeImages = images.Where(shouldDisplay).ToList();
            foreach (var image in safeImages)
                ReceiveImage(image);
            updatePrivacyOnChildren();
        }
        private void updatePrivacyOnChildren() {
            foreach (var child in Children)
                ApplyPrivacyStylingToElement((FrameworkElement)child, Globals.privacy);
        }
        private void ApplyPrivacyStylingToElement(FrameworkElement element, string privacy)
        {
            if (!Globals.isAuthor || Globals.conversationDetails.Permissions == MeTLLib.DataTypes.Permissions.LECTURE_PERMISSIONS) return;
            if (privacy != "private")
                RemovePrivacyStylingFromElement(element);
            else
            {
                element.Effect = new DropShadowEffect { BlurRadius = 50, Color = Colors.Black, ShadowDepth = 0, Opacity = 1 };
                element.Opacity = 0.7;
            }
        }
        private void RemovePrivacyStylingFromElement(FrameworkElement element){
            element.Effect = null;
        }
        private void ReceiveVideos(IEnumerable<MeTLLib.DataTypes.TargettedVideo> videos)
        {
            var safeVideos = videos.Where(shouldDisplay).ToList();
            foreach (var video in safeVideos)
                ReceiveVideo(video);
            updatePrivacyOnChildren();
        }
        private void ReceiveVideo(TargettedVideo video)
        {
            video.video.MediaElement.LoadedBehavior = MediaState.Manual;
            video.video.MediaElement.ScrubbingEnabled = true;
            AddVideo(video.video);
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
        private bool videoExistsOnCanvas(MeTLLib.DataTypes.Video testVideo)
        {
            foreach (UIElement video in Children)
                if (video is MeTLLib.DataTypes.Video)
                    if (videoCompare((MeTLLib.DataTypes.Video)video, testVideo))
                        return true;
            return false;
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
        private bool shouldDisplay(MeTLLib.DataTypes.TargettedElement element)
        {
            return !(element.slide != Globals.location.currentSlide ||
                !(element.target.Equals(target)) ||
                (!(element.privacy == "public" || (element.author == Globals.me))));
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
        public void ReceiveStrokes(IEnumerable<MeTLLib.DataTypes.TargettedStroke> receivedStrokes)
        {
            if (receivedStrokes.Count() == 0) return;
            if (receivedStrokes.First().slide != Globals.location.currentSlide) return;
            var strokeTarget = target;
            var start = SandRibbonObjects.DateTimeFactory.Now();
            var strokes = Strokes.Select(s => s.sum());
            var newStrokes = new StrokeCollection(
                receivedStrokes.Where(ts => ts.target == strokeTarget)
                .Where(s => s.privacy == "public" || (s.author == Globals.me))
                .Select(s => (Stroke)new PrivateAwareStroke(s.stroke))
                .Where(s => !(strokes.Contains(s.sum()))));
            Strokes.Add(newStrokes);
        }
        private void MoveTo(object _args) {
            ClearAdorners();
            Strokes.Clear();
            Children.Clear();
        }
        public void DoWithCurrentSelection(Action<SelectedIdentity> todo)
        {
            foreach (var stroke in GetSelectedStrokes())
                todo(new SelectedIdentity(stroke.startingSum().ToString(),this.target));
            foreach (var element in GetSelectedElements())
                todo(new SelectedIdentity((string)((FrameworkElement)element).Tag,this.target));
        }
        private Adorner[] getPrivacyAdorners()
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer == null) return null;
            return adornerLayer.GetAdorners(this);
        }
        private void ClearAdorners() {
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if(adornerLayer == null) return;
            var adorners = adornerLayer.GetAdorners(this);
            if (adorners != null)
                foreach (var adorner in adorners)
                    adornerLayer.Remove(adorner);
        }
        void ImageDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (fileNames == null) return;
            foreach (string fileName in fileNames)
            {
                FileType type = Image.GetFileType(fileName);
                if (new[] { FileType.Image, FileType.Video }.Contains(type))
                    e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }
        void ImagesDrop(object sender, DragEventArgs e)
        {
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (fileNames == null)
            {
                MessageBox.Show("Cannot drop this onto the canvas");
                return;
            }
            Commands.SetLayer.ExecuteAsync("Insert");
            var pos = e.GetPosition(this);
            var origX = pos.X;
            var height = 0.0;
            for (var i = 0; i < fileNames.Count(); i++)
            {
                var filename = fileNames[i];
                Commands.ImageDropped.Execute(new ImageDrop { filename = filename, point = pos, target = target, position = i });
            }
            e.Handled = true;
        }
        public StrokeCollection GetSelectedStrokes()
        {
            return filter(base.GetSelectedStrokes(), Globals.me);
        }
        private void selectingStrokes(object sender, InkCanvasSelectionChangingEventArgs e)
        {
            var selectedStrokes = e.GetSelectedStrokes();
            var myStrokes = filter(selectedStrokes, Globals.me);
            e.SetSelectedStrokes(myStrokes);
            ClearAdorners();
        }
        private void singleStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            e.Stroke.tag(new MeTLLib.DataTypes.StrokeTag { author = Globals.me, privacy = Globals.privacy, isHighlighter = e.Stroke.DrawingAttributes.IsHighlighter });
            var privateAwareStroke = new PrivateAwareStroke(e.Stroke);
            Strokes.Remove(e.Stroke);
            privateAwareStroke.startingSum(privateAwareStroke.sum().checksum);
            Strokes.Add(privateAwareStroke);
            doMyStrokeAdded(privateAwareStroke);
            Commands.RequerySuggested(Commands.Undo);
        }
        private void erasingStrokes(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            if (!(filter(Strokes, Globals.me).Contains(e.Stroke)))
            {
                e.Cancel = true;
                return;
            }
            doMyStrokeRemoved(e.Stroke);
        }
        private void doMyStrokeRemoved(Stroke stroke)
        {
            ClearAdorners();
            doMyStrokeRemovedExceptHistory(stroke);
            UndoHistory.Queue(
                () =>
                {
                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                    {
                        Strokes.Add(stroke);
                        doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                    }
                },
                () =>
                {
                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                    {
                        Strokes.Remove(stroke);
                        doMyStrokeRemoved(stroke);
                    }
                });
        }
        private void doMyStrokeRemovedExceptHistory(Stroke stroke)
        {
            var sum = stroke.sum().checksum.ToString();
            var bounds = stroke.GetBounds();
            Commands.SendDirtyStroke.Execute(new MeTLLib.DataTypes.TargettedDirtyElement(Globals.location.currentSlide,Globals.me,target,stroke.tag().privacy,sum));
        }
        private void transmitSelectionAltered(object sender, EventArgs e)
        {
            foreach (var stroke in GetSelectedStrokes())
                doMyStrokeAdded(stroke, stroke.tag().privacy);
        }
        private void deleteSelectedStrokes(object _sender, ExecutedRoutedEventArgs _handler)
        {
            dirtySelectedRegions(null, null);
        }
        private void dirtySelectedRegions(object _sender, InkCanvasSelectionEditingEventArgs _e)
        {
            foreach (var stroke in GetSelectedStrokes())
                doMyStrokeRemoved(stroke);
        }
        #region utilityFunctions
        private StrokeCollection filter(IEnumerable<Stroke> from, string author)
        {
            return new StrokeCollection(from.Where(s => s.tag().author == author));
        }
        public void doMyStrokeAdded(Stroke stroke)
        {
            doMyStrokeAdded(stroke, Globals.privacy);
        }
        public void doMyStrokeAdded(Stroke stroke, string intendedPrivacy)
        {
            doMyStrokeAddedExceptHistory(stroke, intendedPrivacy);
            UndoHistory.Queue(
                () =>
                {
                    var existingStroke = Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).FirstOrDefault();
                    if (existingStroke != null)
                        doMyStrokeRemovedExceptHistory(existingStroke);
                },
                () =>
                {
                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                    {
                        Strokes.Add(stroke);
                        doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                    }
                });
        }
        private void doMyStrokeAddedExceptHistory(Stroke stroke, string thisPrivacy)
        {
            stroke.tag(new MeTLLib.DataTypes.StrokeTag { author = Globals.me, privacy = thisPrivacy, isHighlighter = stroke.DrawingAttributes.IsHighlighter });
            SendTargettedStroke(stroke, thisPrivacy);
        }
        public void SendTargettedStroke(Stroke stroke, string thisPrivacy)
        {
            if (!stroke.shouldPersist()) return;
            Commands.ActualReportStrokeAttributes.ExecuteAsync(stroke.DrawingAttributes);
            Commands.SendStroke.Execute(new MeTLLib.DataTypes.TargettedStroke(Globals.location.currentSlide,Globals.me,target,stroke.tag().privacy,stroke, stroke.tag().startingSum));
        }
        public void Disable()
        {
            EditingMode = InkCanvasEditingMode.None;
        }
        public void Enable()
        {
            if (EditingMode == InkCanvasEditingMode.None)
                EditingMode = InkCanvasEditingMode.Ink;
        }
        public void setPenColor(System.Windows.Media.Color color)
        {
            DefaultDrawingAttributes.Color = color;
        }
        public void SetEditingMode(InkCanvasEditingMode mode)
        {
            EditingMode = mode;
        }
        #endregion
        protected void HandlePaste()
        {
            var strokesBeforePaste = Strokes.Select(s => s).ToList();
            Paste();
            var newStrokes = Strokes.Where(s => !strokesBeforePaste.Contains(s)).ToList();
            foreach (var stroke in newStrokes)
                doMyStrokeAdded(stroke, stroke.tag().privacy);
        }
        protected void HandleCopy()
        {
            CopySelection();
        }
        protected void HandleCut()
        {
            var listToCut = new List<MeTLLib.DataTypes.TargettedDirtyElement>();
            foreach (var stroke in GetSelectedStrokes())
                listToCut.Add(new MeTLLib.DataTypes.TargettedDirtyElement(Globals.location.currentSlide,Globals.me,target,stroke.tag().privacy,stroke.sum().checksum.ToString()));
            CutSelection();
            foreach (var element in listToCut)
                Commands.SendDirtyStroke.Execute(element);
        }
        public void ReceiveDirtyStrokes(IEnumerable<MeTLLib.DataTypes.TargettedDirtyElement> targettedDirtyStrokes)
        {
            if (targettedDirtyStrokes.Count() == 0) return;
            if (!(targettedDirtyStrokes.First().target.Equals(target)) || targettedDirtyStrokes.First().slide != Globals.location.currentSlide) return;
            Dispatcher.adopt(delegate
            {
                var dirtyChecksums = targettedDirtyStrokes.Select(t => t.identifier);
                var presentDirtyStrokes = Strokes.Where(s => dirtyChecksums.Contains(s.sum().checksum.ToString())).ToList();
                for (int i = 0; i < presentDirtyStrokes.Count(); i++)
                {
                    var stroke = presentDirtyStrokes[i];
                    Strokes.Remove(stroke);

                }
            });
        }
    }
}
