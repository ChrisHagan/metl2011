using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using MeTLLib.Utilities;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbonObjects;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using MeTLLib.Providers;

namespace SandRibbon.Components
{
    public class SizeWithTarget
    {
        public SizeWithTarget(double width, double height, string target)
        {
            Size = new Size(width, height);
            Target = target;
        }

        public string Target { get; private set; }
        public Size Size { get; private set; }
        public double Width { get { return Size.Width; } }
        public double Height { get { return Size.Height; } }
    }

    public struct MoveDeltaMetrics
    {
        public Rect OldRectangle { get; private set; }
        public Rect NewRectangle { get; private set; }

        public Rect OriginalContentBounds { get; private set; }

        public void SetContentBounds(Rect contentBounds)
        {
            OriginalContentBounds = contentBounds;
        }

        public void Update(Rect oldRect, Rect newRect)
        {
            OldRectangle = oldRect;
            NewRectangle = newRect;
        }

        public Vector Delta
        {
            get
            {
                return NewRectangle.Location - OldRectangle.Location;
            }
        }

        public Vector Scale
        {
            get
            {
                return new Vector(NewRectangle.Width / OldRectangle.Width, NewRectangle.Height / OldRectangle.Height);
            }
        }
    }

    public class TagInformation
    {
        public string Author;
        public bool IsPrivate;
        public string Id;
    }
    public class TextInformation : TagInformation
    {
        public TextInformation()
        {
        }

        public TextInformation(TextInformation copyTextInfo)
        {
            Author = copyTextInfo.Author;
            IsPrivate = copyTextInfo.IsPrivate;
            Id = copyTextInfo.Id;
            Size = copyTextInfo.Size;
            Family = copyTextInfo.Family;
            Underline = copyTextInfo.Underline;
            Bold = copyTextInfo.Bold;
            Italics = copyTextInfo.Italics;
            Strikethrough = copyTextInfo.Strikethrough;
            Target = copyTextInfo.Target;
            Color = copyTextInfo.Color;
        }

        public double Size;
        public FontFamily Family;
        public bool Underline;
        public bool Bold;
        public bool Italics;
        public bool Strikethrough;
        public string Target;
        public Color Color;
    }
    public struct ImageDrop
    {
        public string Filename;
        public Point Point;
        public string Target;
        public int Position;
        public bool OverridePoint;
    }
    public enum FileType
    {
        Video,
        Image,
        NotSupported
    }

    public class TypingTimedAction : TimedAction<Queue<Action>>
    {
        protected override void AddAction(Action timedAction)
        {
            timedActions.Enqueue(timedAction);
        }

        protected override Action GetTimedAction()
        {
            if (timedActions.Count == 0)
                return null;

            return timedActions.Dequeue();
        }
    }

    public partial class CollapsedCanvasStack : UserControl, IClipboardHandler
    {
        List<MeTLTextBox> _boxesAtTheStart = new List<MeTLTextBox>();
        private Color _currentColor = Colors.Black;
        private const double DefaultSize = 24.0;
        private readonly FontFamily _defaultFamily = new FontFamily("Arial");
        private double _currentSize = 24.0;
        private FontFamily _currentFamily = new FontFamily("Arial");
        private const bool CanFocus = true;
        private bool _focusable = true;
        public static TypingTimedAction TypingTimer;
        private string _originalText;
        private ContentBuffer contentBuffer;
        private string _target;
        private StackMoveDeltaProcessor moveDeltaProcessor;
        private Privacy _defaultPrivacy;
        private readonly ClipboardManager clipboardManager = new ClipboardManager();
        private string _me = String.Empty;
        public double offsetX
        {
            get { return contentBuffer.logicalX; }
        }
        public double offsetY
        {
            get { return contentBuffer.logicalY; }
        }
        public string me
        {
            get
            {
                if (String.IsNullOrEmpty(_me))
                    return Globals.me;
                return _me;
            }
            set { _me = value; }
        }

        private MeTLTextBox _lastFocusedTextBox;
        private MeTLTextBox myTextBox
        {
            get
            {
                return _lastFocusedTextBox;
            }
            set
            {
                _lastFocusedTextBox = value;
            }
        }

        private Privacy canvasAlignedPrivacy(Privacy incomingPrivacy)
        {
            if (_target == "presentationSpace")
            {
                //if (Globals.conversationDetails.Permissions.studentCanPublish == false)
                //{
                //    if (Globals.conversationDetails.Author != Globals.me)
                //    {
                //        incomingPrivacy = Privacy.Private;
                //    }
                //}
                return incomingPrivacy;
            }
            else
                return _defaultPrivacy;
        }

        private Privacy currentPrivacy
        {
            get { return canvasAlignedPrivacy((Privacy)Enum.Parse(typeof(Privacy), Globals.privacy, true)); }
        }

        private Point pos = new Point(15, 15);
        private void wireInPublicHandlers()
        {
            PreviewKeyDown += keyPressed;
            Work.StrokeCollected += singleStrokeCollected;
            Work.SelectionChanging += selectionChanging;
            Work.SelectionChanged += selectionChanged;
            Work.StrokeErasing += erasingStrokes;
            Work.SelectionMoving += SelectionMovingOrResizing;
            Work.SelectionMoved += SelectionMovedOrResized;
            Work.SelectionResizing += SelectionMovingOrResizing;
            Work.SelectionResized += SelectionMovedOrResized;
            Work.AllowDrop = true;
            Work.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(MyWork_PreviewMouseLeftButtonUp);
            Work.Drop += ImagesDrop;
            Loaded += (a, b) =>
            {
                MouseUp += (c, args) => placeCursor(this, args);
            };
        }

        void MyWork_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            pos = e.GetPosition(this);
        }
        public CollapsedCanvasStack()
        {
            InitializeComponent();
            wireInPublicHandlers();
            contentBuffer = new ContentBuffer();
            contentBuffer.ElementsRepositioned += (sender, args) => { AddAdorners(); };
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, deleteSelectedElements, canExecute));
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            Commands.SetInkCanvasMode.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(setInkCanvasMode));
            Commands.ReceiveStroke.RegisterCommandToDispatcher(new DelegateCommand<TargettedStroke>((stroke) => ReceiveStrokes(new[] { stroke })));
            Commands.ReceiveStrokes.RegisterCommandToDispatcher(new DelegateCommand<IEnumerable<TargettedStroke>>(ReceiveStrokes));
            Commands.ReceiveDirtyStrokes.RegisterCommand(new DelegateCommand<IEnumerable<TargettedDirtyElement>>(ReceiveDirtyStrokes));
            Commands.ZoomChanged.RegisterCommand(new DelegateCommand<double>(ZoomChanged));

            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage>((image) => ReceiveImages(new[] { image })));
            Commands.ReceiveDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyImage));
            Commands.AddImage.RegisterCommandToDispatcher(new DelegateCommand<object>(addImageFromDisk));
            Commands.ReceiveMoveDelta.RegisterCommandToDispatcher(new DelegateCommand<TargettedMoveDelta>((moveDelta) => { ReceiveMoveDelta(moveDelta); }));

            Commands.ReceiveTextBox.RegisterCommandToDispatcher(new DelegateCommand<TargettedTextBox>(ReceiveTextBox));
            Commands.UpdateTextStyling.RegisterCommand(new DelegateCommand<TextInformation>(updateStyling));
            Commands.RestoreTextDefaults.RegisterCommand(new DelegateCommand<object>(resetTextbox));
            Commands.EstablishPrivileges.RegisterCommand(new DelegateCommand<string>(setInkCanvasMode));
            Commands.SetTextCanvasMode.RegisterCommand(new DelegateCommand<string>(setInkCanvasMode));
            Commands.ReceiveDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(receiveDirtyText));

#if TOGGLE_CONTENT
            Commands.SetContentVisibility.RegisterCommandToDispatcher<ContentVisibilityEnum>(new DelegateCommand<ContentVisibilityEnum>(SetContentVisibility));
#endif

            Commands.ExtendCanvasBySize.RegisterCommandToDispatcher<SizeWithTarget>(new DelegateCommand<SizeWithTarget>(extendCanvasBySize));

            Commands.ImageDropped.RegisterCommandToDispatcher(new DelegateCommand<ImageDrop>(imageDropped));
            Commands.ImagesDropped.RegisterCommandToDispatcher(new DelegateCommand<List<ImageDrop>>(imagesDropped));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            Commands.DeleteSelectedItems.RegisterCommandToDispatcher(new DelegateCommand<object>(deleteSelectedItems));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<Privacy>(changeSelectedItemsPrivacy));
            Commands.SetDrawingAttributes.RegisterCommandToDispatcher(new DelegateCommand<DrawingAttributes>(SetDrawingAttributes));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>((_unused) => { JoinConversation(); }));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(hideAdorners));
            Commands.HideConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(HideConversationSearchBox));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (sender, args) => HandlePaste(args), canExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, args) => HandleCopy(args), canExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (sender, args) => HandleCut(args), canExecute));
            Loaded += (_sender, _args) => this.Dispatcher.adoptAsync(delegate
            {
                if (_target == null)
                {
                    _target = (string)FindResource("target");
                    _defaultPrivacy = (Privacy)FindResource("defaultPrivacy");

                    if (_target == "presentationSpace")
                    {
                        Globals.CurrentCanvasClipboardFocus = _target;
                    }

                    moveDeltaProcessor = new StackMoveDeltaProcessor(Work, contentBuffer, _target);
                }
            });
            Commands.ClipboardManager.RegisterCommand(new DelegateCommand<ClipboardAction>((action) => clipboardManager.OnClipboardAction(action)));
            clipboardManager.RegisterHandler(ClipboardAction.Paste, OnClipboardPaste, CanHandleClipboardPaste);
            clipboardManager.RegisterHandler(ClipboardAction.Cut, OnClipboardCut, CanHandleClipboardCut);
            clipboardManager.RegisterHandler(ClipboardAction.Copy, OnClipboardCopy, CanHandleClipboardCopy);
            Work.MouseMove += mouseMove;
            Work.StylusMove += stylusMove;
            Work.IsKeyboardFocusWithinChanged += Work_IsKeyboardFocusWithinChanged;
            Globals.CanvasClipboardFocusChanged += CanvasClipboardFocusChanged;

            //For development
            if (_target == "presentationSpace" && me != Globals.PROJECTOR)
                UndoHistory.ShowVisualiser(Window.GetWindow(this));
        }

        void Work_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Globals.CurrentCanvasClipboardFocus = _target;
            }
        }

        void CanvasClipboardFocusChanged(object sender, EventArgs e)
        {
            if ((string)sender != _target)
            {
                ClipboardFocus.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                ClipboardFocus.BorderBrush = new SolidColorBrush(Colors.Pink);
            }
        }

        private void JoinConversation()
        {
            if (myTextBox != null)
            {
                myTextBox.LostFocus -= textboxLostFocus;
                myTextBox = null;
            }
        }

        private void stylusMove(object sender, StylusEventArgs e)
        {
            GlobalTimers.ResetSyncTimer();
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                GlobalTimers.ResetSyncTimer();
            }
        }

        private void canExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Work.GetSelectedElements().Count > 0 || Work.GetSelectedStrokes().Count > 0 || myTextBox != null;
        }

        private void extendCanvasBySize(SizeWithTarget newSize)
        {
            if (_target == newSize.Target)
            {
                Height = newSize.Height;
                Width = newSize.Width;
            }
        }

        private InkCanvasEditingMode currentMode;
        private void hideAdorners(object obj)
        {
            currentMode = Work.EditingMode;
            Work.Select(new UIElement[] { });
            ClearAdorners();
        }
        private void keyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && (Work.GetSelectedElements().Count > 0 || Work.GetSelectedStrokes().Count > 0))//&& myTextBox == null)
                deleteSelectedElements(null, null);
            if (e.Key == Key.PageUp || (e.Key == Key.Up && myTextBox == null))
            {
                if (Commands.MoveToPrevious.CanExecute(null))
                    Commands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.PageDown || (e.Key == Key.Down && myTextBox == null))
            {
                if (Commands.MoveToNext.CanExecute(null))
                    Commands.MoveToNext.Execute(null);
                e.Handled = true;
            }
        }
        private void SetLayer(string newLayer)
        {
            if (me.ToLower() == Globals.PROJECTOR) return;
            switch (newLayer)
            {
                case "Text":
                    Work.EditingMode = InkCanvasEditingMode.None;
                    Work.UseCustomCursor = Work.EditingMode == InkCanvasEditingMode.Ink;
                    break;
                case "Insert":
                    Work.EditingMode = InkCanvasEditingMode.Select;
                    Work.UseCustomCursor = Work.EditingMode == InkCanvasEditingMode.Ink;
                    Work.Cursor = Cursors.Arrow;
                    break;
                case "Sketch":
                    Work.UseCustomCursor = true;
                    break;
            }
            _focusable = newLayer == "Text";
            setLayerForTextFor(Work);
        }
        private void setLayerForTextFor(InkCanvas canvas)
        {
            var curFocusable = _focusable;
            var curMe = Globals.me;

            foreach (var box in canvas.Children)
            {
                if (box.GetType() == typeof(MeTLTextBox))
                {
                    var tag = ((MeTLTextBox)box).tag();
                    ((MeTLTextBox)box).Focusable = curFocusable && (tag.author == curMe);
                }
            }
            contentBuffer.UpdateAllTextBoxes((textBox) =>
            {
                var tag = textBox.tag();
                textBox.Focusable = curFocusable && (tag.author == curMe);
            });
        }
        private UndoHistory.HistoricalAction deleteSelectedImages(IEnumerable<MeTLImage> selectedElements)
        {
            if (selectedElements.Count() == 0) return new UndoHistory.HistoricalAction(() => { }, () => { }, 0, "No images selected");
            Action undo = () =>
               {
                   foreach (var element in selectedElements)
                   {
                       // if this element hasn't already been added
                       if (Work.ImageChildren().ToList().Where(i => ((Image)i).tag().id == ((Image)element).tag().id).Count() == 0)
                       {
                           contentBuffer.AddImage(element, (child) => Work.Children.Add(child));
                       }
                       sendImage(element);
                   }
                   Work.Select(selectedElements.Select(i => (UIElement)i));
               };
            Action redo = () =>
                {
                    foreach (var element in selectedElements)
                    {
                        var imagesToRemove = Work.ImageChildren().ToList().Where(i => ((Image)i).tag().id == ((Image)element).tag().id);
                        if (imagesToRemove.Count() > 0)
                        {
                            contentBuffer.RemoveImage(imagesToRemove.First(), (image) => Work.Children.Remove(image));
                        }
                        // dirty is now handled by movedelta
                        //dirtyThisElement(element);
                    }
                };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Delete selected images");
        }
        private UndoHistory.HistoricalAction deleteSelectedInk(IEnumerable<PrivateAwareStroke> selectedStrokes)
        {
            Action undo = () =>
                {
                    addStrokes(selectedStrokes);

                    if (Work.EditingMode == InkCanvasEditingMode.Select)
                        Work.Select(new StrokeCollection(selectedStrokes.Select(s => s as Stroke)));

                };
            Action redo = () => removeStrokes(selectedStrokes);
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Delete selected ink");
        }
        private UndoHistory.HistoricalAction deleteSelectedText(IEnumerable<MeTLTextBox> elements)
        {
            var selectedElements = elements.Where(t => t.tag().author == Globals.me).Select(b => ((MeTLTextBox)b).clone()).ToList();
            Action undo = () =>
                              {
                                  myTextBox = selectedElements.LastOrDefault();
                                  foreach (var box in selectedElements)
                                  {
                                      if (!alreadyHaveThisTextBox(box))
                                          AddTextBoxToCanvas(box, false);
                                      box.PreviewKeyDown += box_PreviewTextInput;
                                      sendTextWithoutHistory(box, box.tag().privacy);
                                  }
                              };
            Action redo = () =>
                              {
                                  myTextBox = null;
                                  foreach (var box in selectedElements)
                                  {
                                      // dirty is now handled by move delta
                                      //dirtyTextBoxWithoutHistory(box);
                                      box.PreviewKeyDown -= box_PreviewTextInput;
                                      box.RemovePrivacyStyling(contentBuffer);
                                      RemoveTextBoxWithMatchingId(box.tag().id);
                                  }
                                  // set keyboard focus to the current canvas so the help button does not grey out
                                  Keyboard.Focus(this);
                                  ClearAdorners();
                              };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Delete selected text");
        }

        private void AddTextBoxToCanvas(MeTLTextBox box, Boolean isAdjustedForNegativeCartesian)
        {
            Panel.SetZIndex(box, 3);
            AddTextboxToMyCanvas(box);
        }

        private void deleteSelectedElements(object _sender, ExecutedRoutedEventArgs _handler)
        {
            if (me == Globals.PROJECTOR) return;

            IEnumerable<UIElement> selectedElements = null;
            IEnumerable<MeTLTextBox> selectedTextBoxes = null;
            IEnumerable<MeTLImage> selectedImages = null;
            List<PrivateAwareStroke> selectedStrokes = null;
            Dispatcher.adopt(() =>
                                 {
                                     selectedStrokes = filterOnlyMineExceptIfHammering(Work.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke)).ToList();
                                     selectedElements = filterOnlyMineExceptIfHammering(Work.GetSelectedElements());
                                 });
            selectedTextBoxes = selectedElements.OfType<MeTLTextBox>();
            selectedImages = selectedElements.OfType<MeTLImage>();

            var ink = deleteSelectedInk(selectedStrokes);
            var images = deleteSelectedImages(selectedImages);
            var text = deleteSelectedText(selectedTextBoxes);
            Action undo = () =>
                {
                    ink.undo();
                    text.undo();
                    images.undo();
                    ClearAdorners();
                    Work.Focus();
                };
            Action redo = () =>
                {
                    var identity = Globals.generateId(Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(Globals.slide, Globals.me, _target, currentPrivacy, identity, -1L, new StrokeCollection(selectedStrokes.Select(s => s as Stroke)), selectedTextBoxes.Select(s => s as TextBox), selectedImages.Select(s => s as Image));
                    moveDelta.isDeleted = true;
                    moveDeltaProcessor.rememberSentMoveDelta(moveDelta);
                    Commands.SendMoveDelta.ExecuteAsync(moveDelta);
                    Keyboard.Focus(this); // set keyboard focus to the current canvas so the help button does not grey out

                    ink.redo();
                    text.redo();
                    images.redo();
                    ClearAdorners();
                    Work.Focus();
                };
            UndoHistory.Queue(undo, redo, "Delete selected items");
            redo();
        }
        private void HideConversationSearchBox(object obj)
        {
            Work.EditingMode = currentMode;
            AddAdorners();
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            ClearAdorners();
            if (ConversationDetails.Empty.Equals(details)) return;
            Dispatcher.adoptAsync(delegate
                  {
                      foreach (MeTLImage image in Work.ImageChildren())
                          image.ApplyPrivacyStyling(contentBuffer, _target, image.tag().privacy);
                      foreach (var item in Work.TextChildren())
                      {
                          MeTLTextBox box = null;
                          box = (MeTLTextBox)item;
                          box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy);
                      }

                      if (myTextBox != null)
                      {
                          myTextBox.Focus();
                          Keyboard.Focus(myTextBox);
                      }

                  });
        }
        private void SetPrivacy(string privacy)
        {
            AllowDrop = true;
        }
        private void setInkCanvasMode(string modeString)
        {
            if (me == Globals.PROJECTOR) return;
            Work.EditingMode = (InkCanvasEditingMode)Enum.Parse(typeof(InkCanvasEditingMode), modeString);
            Work.UseCustomCursor = Work.EditingMode == InkCanvasEditingMode.Ink;
        }
        private Double zoom = 1;
        private void ZoomChanged(Double zoom)
        {
            // notepad does not currently support zoom
            if (_target == "notepad")
                return;

            this.zoom = zoom;
            if (Commands.SetDrawingAttributes.IsInitialised && Work.EditingMode == InkCanvasEditingMode.Ink)
            {
                SetDrawingAttributes((DrawingAttributes)Commands.SetDrawingAttributes.LastValue());
            }
        }

        #region Events
        private List<PrivateAwareStroke> strokesAtTheStart = new List<PrivateAwareStroke>();
        private List<UIElement> imagesAtStartOfTheMove = new List<UIElement>();
        private MoveDeltaMetrics moveMetrics;
        private void SelectionMovingOrResizing(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            var inkCanvas = sender as InkCanvas;
            inkCanvas.Select(new StrokeCollection(filterOnlyMineExceptIfHammering(inkCanvas.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke)).Select(s => s as Stroke)), filterOnlyMine(inkCanvas.GetSelectedElements()));
            bool shouldUpdateContentBounds = false;
            var newStrokes = inkCanvas.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke).ToList();
            var strokeEqualityCount = strokesAtTheStart.Select(s => s.tag().id).Union(newStrokes.Select(s => s.tag().id)).Count();
            if (strokeEqualityCount != newStrokes.Count || strokeEqualityCount != strokesAtTheStart.Count)
            {
                shouldUpdateContentBounds = true;
                strokesAtTheStart.Clear();
                strokesAtTheStart.AddRange(newStrokes.Select(s => s.Clone()));
            }

            var newImages = GetSelectedClonedImages().Where(i => i is MeTLImage).Select(i => i as MeTLImage).ToList();
            var imageEqualityCount = imagesAtStartOfTheMove.Where(i => i is MeTLImage).Select(i => (i as MeTLImage).tag().id).Union(newImages.Where(i => i is MeTLImage).Select(i => (i as MeTLImage).tag().id)).Count();
            if (imageEqualityCount != newImages.Count || imageEqualityCount != imagesAtStartOfTheMove.Count)
            {
                shouldUpdateContentBounds = true;
                imagesAtStartOfTheMove.Clear();
                imagesAtStartOfTheMove.AddRange(newImages.Select(i => i as UIElement));
            }

            var newBoxes = inkCanvas.GetSelectedElements().Where(b => b is MeTLTextBox).Select(tb => ((MeTLTextBox)tb).clone()).ToList();
            var boxEqualityCount = _boxesAtTheStart.Where(i => i is MeTLTextBox).Select(i => (i as MeTLTextBox).tag().id).Union(newBoxes.Where(i => i is MeTLTextBox).Select(i => (i as MeTLTextBox).tag().id)).Count();
            if (boxEqualityCount != newBoxes.Count || boxEqualityCount != _boxesAtTheStart.Count)
            {
                shouldUpdateContentBounds = true;
                _boxesAtTheStart.Clear();
                _boxesAtTheStart = newBoxes;
            }

            if (shouldUpdateContentBounds)
            {
                moveMetrics.SetContentBounds(ContentBuffer.getLogicalBoundsOfContent(newImages, newBoxes, newStrokes));
            }
            moveMetrics.Update(e.OldRectangle, e.NewRectangle);
        }

        private UndoHistory.HistoricalAction ImageSelectionMovedOrResized(IEnumerable<MeTLImage> endingElements, List<MeTLImage> startingElements)
        {
            var undoToAdd = startingElements.Select(i => i.Clone());
            var undoToRemove = endingElements.Select(i => i.Clone());
            var redoToAdd = endingElements.Select(i => i.Clone());
            var redoToRemove = startingElements.Select(i => i.Clone());
            Action undo = () =>
                {
                    foreach (var remove in undoToRemove)
                        contentBuffer.RemoveImage(remove, (img) => Work.Children.Remove(img));
                    foreach (var add in undoToAdd)
                    {
                        contentBuffer.AddImage(add, (img) => Work.Children.Add(img));
                        add.ApplyPrivacyStyling(contentBuffer, _target, add.tag().privacy);
                    }
                };
            Action redo = () =>
                {
                    foreach (var remove in redoToRemove)
                        contentBuffer.RemoveImage(remove, (img) => Work.Children.Remove(img));
                    foreach (var add in redoToAdd)
                    {
                        contentBuffer.AddImage(add, (img) => Work.Children.Add(img));
                        add.ApplyPrivacyStyling(contentBuffer, _target, add.tag().privacy);
                    }
                };

            return new UndoHistory.HistoricalAction(undo, redo, 0, "Image selection moved or resized");
        }

        private UndoHistory.HistoricalAction InkSelectionMovedOrResized(IEnumerable<PrivateAwareStroke> selectedStrokes, List<PrivateAwareStroke> undoStrokes)
        {
            Action undo = () =>
                {
                    foreach (var stroke in selectedStrokes)
                    {
                        var strokesToUpdate = Work.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == stroke.tag().id).Select(s => s as PrivateAwareStroke);
                        if (strokesToUpdate.Count() > 0)
                        {
                            contentBuffer.RemoveStroke(strokesToUpdate.First(), (str) => Work.Strokes.Remove(str));
                        }
                    }

                    foreach (var stroke in undoStrokes)
                    {
                        var tmpStroke = stroke.Clone();
                        if (!Work.Strokes.Where(s => s.tag().id == tmpStroke.tag().id).Any())
                        {
                            contentBuffer.AddStroke(tmpStroke, (str) => Work.Strokes.Add(str));
                        }
                    }
                };
            Action redo = () =>
                {
                    foreach (var stroke in undoStrokes)
                    {
                        var strokesToUpdate = Work.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == stroke.tag().id).Select(s => s as PrivateAwareStroke);
                        if (strokesToUpdate.Count() > 0)
                        {
                            contentBuffer.RemoveStroke(strokesToUpdate.First(), (str) =>
                            {
                                Work.Strokes.Remove(str);
                            });
                        }
                    }

                    foreach (var stroke in selectedStrokes)
                    {
                        var tmpStroke = stroke.Clone();
                        if (Work.Strokes.Where(s => s.tag().id == tmpStroke.tag().id).Count() == 0)
                        {
                            var bounds = tmpStroke.GetBounds();
                            contentBuffer.AddStroke(tmpStroke, (str) =>
                            {
                                Work.Strokes.Add(str);
                            });
                        }
                    }
                };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Ink selection moved or resized");
        }


        private UndoHistory.HistoricalAction TextMovedOrResized(IEnumerable<MeTLTextBox> finalBoxes, List<MeTLTextBox> startBoxes)
        {
            Trace.TraceInformation("MovedTextbox");
            var undoBoxesToAdd = startBoxes.Select(t => t.clone());
            var redoBoxesToAdd = finalBoxes.Select(b => b.clone());
            var undoBoxesToRemove = finalBoxes.Select(b => b.clone());
            var redoBoxesToRemove = startBoxes.Select(t => t.clone());
            Action undo = () =>
              {
                  ClearAdorners();
                  foreach (MeTLTextBox box in undoBoxesToRemove)
                  {
                      contentBuffer.RemoveTextBox(box, (txt) => Work.Children.Remove(txt));
                  }
                  foreach (var box in undoBoxesToAdd)
                  {
                      AddTextBoxToCanvas(box, true);
                      box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy);
                  }
              };
            Action redo = () =>
              {
                  ClearAdorners();
                  foreach (var box in redoBoxesToRemove)
                  {
                      contentBuffer.RemoveTextBox(box, (txt) => Work.Children.Remove(txt));
                  }
                  foreach (var box in redoBoxesToAdd)
                  {
                      AddTextBoxToCanvas(box, false);
                      box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy);
                  }
              };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Text selection moved or resized");
        }


        private void refreshWorkSelect(IEnumerable<string> selectedStrokeIds, IEnumerable<string> selectedImageIds, IEnumerable<string> selectedTextBoxIds)
        {
            //This method is responisble for refreshing the selected items for Work
            var selectedStrokCollection = new StrokeCollection();

            if (selectedStrokeIds.Count() > 0)
            {
                foreach (var strokeId in selectedStrokeIds)
                {
                    foreach (var stroke in Work.Strokes)
                    {
                        if (stroke.tag().id == strokeId)
                            selectedStrokCollection.Add(stroke);
                    }
                }
            }

            List<UIElement> selectedElementCollection = new List<UIElement>();

            if (selectedImageIds.Count() > 0)
            {
                foreach (var imageId in selectedImageIds)
                {
                    foreach (var imageChild in Work.ImageChildren())
                    {
                        if (imageChild.tag().id == imageId)
                            selectedElementCollection.Add(imageChild);
                    }
                }
            }

            if (selectedTextBoxIds.Count() > 0)
            {
                foreach (var textBoxId in selectedTextBoxIds)
                {
                    foreach (var textChild in Work.TextChildren())
                    {
                        if (textChild.tag().id == textBoxId)
                            selectedElementCollection.Add(textChild);
                    }
                }
            }

            if (selectedStrokeIds.Count() > 0 && (selectedTextBoxIds.Count() > 0 || selectedImageIds.Count() > 0))
            {
                Work.Select(selectedStrokCollection, selectedElementCollection);
                AddAdorners();
            }
            else if (selectedStrokeIds.Count() > 0)
            {
                Work.Select(selectedStrokCollection);
                AddAdorners();
            }
            else if (selectedTextBoxIds.Count() > 0 || selectedImageIds.Count() > 0)
            {
                Work.Select(selectedElementCollection);
                AddAdorners();
            }
        }

        private void SelectionMovedOrResized(object sender, EventArgs e)
        {
            var startingImages = imagesAtStartOfTheMove.Where(i => i is MeTLImage).Select(i => ((MeTLImage)i).Clone()).ToList();
            var startingStrokes = strokesAtTheStart.Select(stroke => stroke.Clone()).ToList();
            var startingBoxes = _boxesAtTheStart.Select(t => t.clone()).ToList();

            imagesAtStartOfTheMove.Clear();
            strokesAtTheStart.Clear();
            _boxesAtTheStart.Clear();

            var selectedStrokes = filterOnlyMineExceptIfHammering(Work.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => (s as PrivateAwareStroke).Clone()));
            var selectedElements = filterOnlyMineExceptIfHammering(Work.GetSelectedElements());

            var endingStrokes = filterOnlyMineExceptIfHammering(selectedStrokes);
            var endingImages = filterOnlyMineExceptIfHammering(selectedElements.Where(el => el is MeTLImage).Select(el => el as MeTLImage), img => (img as MeTLImage).tag().author).ToList();
            var endingTexts = filterOnlyMineExceptIfHammering(selectedElements.Where(el => el is MeTLTextBox).Select(el => el as MeTLTextBox), el => (el as MeTLTextBox).tag().author).ToList();

            var ink = InkSelectionMovedOrResized(endingStrokes, startingStrokes);
            var images = ImageSelectionMovedOrResized(endingImages.Select(i => (MeTLImage)i).ToList(), startingImages);
            var text = TextMovedOrResized(endingTexts.Select(t => (MeTLTextBox)t).ToList(), startingBoxes);

            var thisXTrans = moveMetrics.Delta.X;
            var thisYTrans = moveMetrics.Delta.Y;
            var thisXScale = moveMetrics.Scale.X;
            var thisYScale = moveMetrics.Scale.Y;

            var startingBounds = ContentBuffer.getLogicalBoundsOfContent(startingImages, startingBoxes, startingStrokes);
            var endingBounds = ContentBuffer.getLogicalBoundsOfContent(endingImages, endingTexts, endingStrokes);

            var thisOriginalBoundsX = moveMetrics.OriginalContentBounds.Left;
            var thisOriginalBoundsY = moveMetrics.OriginalContentBounds.Top;

            var selectedStrokeIds = startingStrokes.Select(stroke => stroke.tag().id);
            var selectedImagesIds = startingImages.Select(image => image.tag().id);
            var selectedTextBoxIds = startingBoxes.Select(textBox => textBox.tag().id);

            Action undo = () =>
                {
                    ClearAdorners();

                    var identity = Globals.generateId(Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(Globals.slide, Globals.me, _target, Privacy.NotSet, identity, -1L,
                        new StrokeCollection(endingStrokes.Select(s => (s as Stroke).Clone())),
                        endingTexts.Select(et => et as TextBox),
                        endingImages.Select(et => et as Image));

                    moveDelta.xTranslate = -thisXTrans;
                    moveDelta.yTranslate = -thisYTrans;
                    moveDelta.xScale = 1 / thisXScale;
                    moveDelta.yScale = 1 / thisYScale;

                    moveDelta.xOrigin = endingBounds.Left;
                    moveDelta.yOrigin = endingBounds.Top;

                    moveDeltaProcessor.rememberSentMoveDelta(moveDelta);
                    Commands.SendMoveDelta.ExecuteAsync(moveDelta);

                    ink.undo();
                    text.undo();
                    images.undo();
                    RefreshCanvas();
                    refreshWorkSelect(selectedStrokeIds, selectedImagesIds, selectedTextBoxIds);

                    AddAdorners();
                    Work.Focus();
                };
            Action redo = () =>
                {
                    ClearAdorners();

                    var identity = Globals.generateId(Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(Globals.slide, Globals.me, _target, Privacy.NotSet, identity, -1L,
                        new StrokeCollection(startingStrokes.Select(s => (s as Stroke).Clone())),
                        startingBoxes.Select(et => et as TextBox),
                        startingImages.Select(et => et as Image));

                    moveDelta.xTranslate = thisXTrans;
                    moveDelta.yTranslate = thisYTrans;
                    moveDelta.xScale = thisXScale;
                    moveDelta.yScale = thisYScale;

                    moveDelta.xOrigin = startingBounds.Left;
                    moveDelta.yOrigin = startingBounds.Top;

                    moveDeltaProcessor.rememberSentMoveDelta(moveDelta);
                    Commands.SendMoveDelta.ExecuteAsync(moveDelta);

                    ink.redo();
                    text.redo();
                    images.redo();

                    RefreshCanvas();

                    refreshWorkSelect(selectedStrokeIds, selectedImagesIds, selectedTextBoxIds);

                    AddAdorners();
                    Work.Focus();
                };
            UndoHistory.Queue(undo, redo, "Selection moved or resized");
            redo();
        }
        private IEnumerable<UIElement> GetSelectedClonedImages()
        {
            var selectedElements = new List<UIElement>();
            foreach (var element in Work.GetSelectedElements())
            {
                if (element is MeTLImage)
                {
                    selectedElements.Add(((MeTLImage)element).Clone());
                    InkCanvas.SetLeft(selectedElements.Last(), InkCanvas.GetLeft(element));
                    InkCanvas.SetTop(selectedElements.Last(), InkCanvas.GetTop(element));
                }
            }
            return selectedElements;
        }

        private void selectionChanging(object sender, InkCanvasSelectionChangingEventArgs e)
        {

            e.SetSelectedElements(filterOnlyMineExceptIfHammering(e.GetSelectedElements()));
            e.SetSelectedStrokes(new StrokeCollection(filterOnlyMineExceptIfHammering(e.GetSelectedStrokes().OfType<PrivateAwareStroke>()).OfType<Stroke>()));
        }

        public List<String> GetSelectedAuthors()
        {
            var authorList = new List<string>();

            var strokeList = Work.GetSelectedStrokes();
            var elementList = Work.GetSelectedElements();

            foreach (var stroke in strokeList)
            {
                authorList.Add(stroke.tag().author);
            }
            foreach (var element in elementList)
            {
                if (element is Image)
                {
                    authorList.Add((element as Image).tag().author);
                }
                else if (element is MeTLTextBox)
                {
                    authorList.Add((element as MeTLTextBox).tag().author);
                }
            }

            return authorList.Distinct().ToList();
        }

        public Dictionary<string, Color> ColourSelectedByAuthor(List<string> authorList)
        {
            var colors = ColorLookup.GetMediaColors();
            var authorColor = new Dictionary<string, Color>();
            foreach (var author in authorList)
                authorColor.Add(author, colors.ElementAt(authorList.IndexOf(author)));

            return authorColor;
        }
        private IEnumerable<T> filterExceptMine<T>(IEnumerable<T> elements) where T : UIElement
        {
            var me = Globals.me;
            var myText = elements.Where(e => e is MeTLTextBox && (e as MeTLTextBox).tag().author != me);
            var myImages = elements.Where(e => e is Image && (e as Image).tag().author != me);
            var myElements = new List<T>();
            myElements.AddRange(myText);
            myElements.AddRange(myImages);
            return myElements;
        }
        private IEnumerable<T> filterExceptMine<T>(IEnumerable<T> elements, Func<T, string> authorExtractor) where T : UIElement
        {
            var me = Globals.me.Trim().ToLower();
            var myText = elements.Where(e => e is MeTLTextBox && authorExtractor(e).Trim().ToLower() == me);
            var myImages = elements.Where(e => e is Image && authorExtractor(e).Trim().ToLower() == me);
            var myElements = new List<T>();
            myElements.AddRange(myText);
            myElements.AddRange(myImages);
            return myElements;
        }

        private IEnumerable<PrivateAwareStroke> filterOnlyMineExceptIfHammering(IEnumerable<PrivateAwareStroke> strokes)
        {
            var me = Globals.me;
            if (Globals.IsBanhammerActive)
            {
                return strokes.Where(s => s.tag().author != me);
            }
            else
            {
                return strokes.Where(s => s.tag().author == me).ToList();
            }
        }
        private IEnumerable<T> filterOnlyMine<T>(IEnumerable<T> elements) where T : UIElement
        {
            var myText = elements.OfType<MeTLTextBox>().Where(t => t.tag().author == Globals.me);
            var myImages = elements.OfType<MeTLImage>().Where(i => i.tag().author == Globals.me);
            var myElements = new List<T>();
            myElements.AddRange(myText.OfType<T>());
            myElements.AddRange(myImages.OfType<T>());
            return myElements;
        }
        private IEnumerable<T> filterOnlyMineExceptIfHammering<T>(IEnumerable<T> elements, Func<T, string> authorExtractor) where T : UIElement
        {
            if (Globals.IsBanhammerActive)
            {
                return filterExceptMine(elements, authorExtractor);
            }
            else
            {
                return filterOnlyMine(elements, authorExtractor);
            }
        }
        private IEnumerable<T> filterOnlyMineExceptIfHammering<T>(IEnumerable<T> elements) where T : UIElement
        {
            if (Globals.IsBanhammerActive)
            {
                return filterExceptMine(elements);
            }
            else
            {
                return filterOnlyMine(elements);
            }
        }
        private IEnumerable<T> filterOnlyMine<T>(IEnumerable<T> elements, Func<T, string> authorExtractor)
        {
            return elements.Where(e => authorExtractor(e).Trim().ToLower() == Globals.me.Trim().ToLower());
        }
        private T filterOnlyMine<T>(UIElement element) where T : UIElement
        {
            UIElement filteredElement = null;

            if (element == null)
                return null;

            if (element is MeTLTextBox)
            {
                filteredElement = ((MeTLTextBox)element).tag().author == Globals.me ? element : null;
            }
            else if (element is Image)
            {
                filteredElement = ((Image)element).tag().author == Globals.me ? element : null;
            }
            return filteredElement as T;
        }

        private void selectionChanged(object sender, EventArgs e)
        {
            myTextBox = (MeTLTextBox)Work.GetSelectedTextBoxes().FirstOrDefault();
            updateTools();
            AddAdorners();
        }

        protected internal void AddAdorners()
        {
            ClearAdorners();
            var selectedStrokes = Work.GetSelectedStrokes();
            var selectedElements = Work.GetSelectedElements();
            if (selectedElements.Count == 0 && selectedStrokes.Count == 0) return;
            var publicStrokes = selectedStrokes.Where(s => s.tag().privacy == Privacy.Public).ToList();
            var publicImages = selectedElements.Where(i => (((i is Image) && ((Image)i).tag().privacy == Privacy.Public))).ToList();
            var publicText = selectedElements.Where(i => (((i is MeTLTextBox) && ((MeTLTextBox)i).tag().privacy == Privacy.Public))).ToList();
            var publicCount = publicStrokes.Count + publicImages.Count + publicText.Count;
            var allElementsCount = selectedStrokes.Count + selectedElements.Count;
            string privacyChoice;
            if (publicCount == 0)
                privacyChoice = "show";
            else if (publicCount == allElementsCount)
                privacyChoice = "hide";
            else
                privacyChoice = "both";
            Commands.AddPrivacyToggleButton.Execute(new PrivacyToggleButton.PrivacyToggleButtonInfo(privacyChoice, allElementsCount != 0, Work.GetSelectionBounds(), _target));
        }
        private void ClearAdorners()
        {
            if (me != Globals.PROJECTOR)
                Commands.RemovePrivacyAdorners.ExecuteAsync(_target);
        }
        #endregion
        #region ink
        private void SetDrawingAttributes(DrawingAttributes logicalAttributes)
        {
            if (logicalAttributes == null) return;
            if (me.ToLower() == Globals.PROJECTOR) return;
            var zoomCompensatedAttributes = logicalAttributes.Clone();
            try
            {
                zoomCompensatedAttributes.Width = logicalAttributes.Width * zoom;
                zoomCompensatedAttributes.Height = logicalAttributes.Height * zoom;
                var visualAttributes = logicalAttributes.Clone();
                visualAttributes.Width = logicalAttributes.Width * 2;
                visualAttributes.Height = logicalAttributes.Height * 2;
                Work.UseCustomCursor = true;
                Work.Cursor = CursorExtensions.generateCursor(visualAttributes);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Cursor failed (no crash):", e.Message);
            }
            Work.DefaultDrawingAttributes = zoomCompensatedAttributes;
        }
        public List<Stroke> PublicStrokes
        {
            get
            {
                var canvasStrokes = new List<Stroke>();
                canvasStrokes.AddRange(Work.Strokes);
                return canvasStrokes.Where(s => s.tag().privacy == Privacy.Public).ToList();
            }
        }
        public List<Stroke> AllStrokes
        {
            get
            {
                var canvasStrokes = new List<Stroke>();
                canvasStrokes.AddRange(Work.Strokes);
                return canvasStrokes;

            }
        }
        public void ReceiveDirtyStrokes(IEnumerable<TargettedDirtyElement> targettedDirtyStrokes)
        {
            if (targettedDirtyStrokes.Count() == 0) return;
            if (!(targettedDirtyStrokes.First().target.Equals(_target)) || targettedDirtyStrokes.First().slide != Globals.slide) return;
            Dispatcher.adopt(delegate
            {
                dirtyStrokes(Work, targettedDirtyStrokes);
            });
        }
        private void dirtyStrokes(InkCanvas canvas, IEnumerable<TargettedDirtyElement> targettedDirtyStrokes)
        {
            var dirtyIds = targettedDirtyStrokes.Select(s => s.identity);
            var dirtyStrokes = canvas.Strokes.Where(s => s is PrivateAwareStroke && dirtyIds.Contains(s.tag().id)).Select(s => s as PrivateAwareStroke).ToList();
            // 1. find the strokes in the contentbuffer that have matching checksums 
            // 2. remove those strokes and corresponding checksums in the content buffer
            // 3. for the strokes that also exist in the canvas, remove them and their checksums
            contentBuffer.RemoveStrokes(dirtyStrokes, strokeCollection =>
            {
                foreach (var stroke in strokeCollection)
                {
                    canvas.Strokes.Remove(stroke);
                }
            });
        }

        public void SetContentVisibility(ContentVisibilityEnum contentVisibility)
        {
            if (_target == "notepad")
                return;

            Commands.UpdateContentVisibility.Execute(contentVisibility);

            ClearAdorners();

            var selectedStrokes = Work.GetSelectedStrokes().Select(stroke => stroke.tag().id);
            var selectedImages = Work.GetSelectedImages().ToList().Select(image => image.tag().id);
            var selectedTextBoxes = Work.GetSelectedTextBoxes().ToList().Select(textBox => textBox.tag().id);

            ReAddFilteredContent(contentVisibility);

            if (me != Globals.PROJECTOR)
                refreshWorkSelect(selectedStrokes, selectedImages, selectedTextBoxes);
        }

        private void ReAddFilteredContent(ContentVisibilityEnum contentVisibility)
        {
            Work.Strokes.Clear();
            Work.Strokes.Add(new StrokeCollection(contentBuffer.FilteredStrokes(contentVisibility).Select(s => s as Stroke)));

            Work.Children.Clear();
            foreach (var child in contentBuffer.FilteredTextBoxes(contentVisibility))
                Work.Children.Add(child);
            foreach (var child in contentBuffer.FilteredImages(contentVisibility))
                Work.Children.Add(child);
        }

        public void ReceiveStrokes(IEnumerable<TargettedStroke> receivedStrokes)
        {
            if (receivedStrokes.Count() == 0) return;
            if (receivedStrokes.First().slide != Globals.slide) return;
            var strokeTarget = _target;
            foreach (var targettedStroke in receivedStrokes.Where(targettedStroke => targettedStroke.target == strokeTarget))
            {
                if (targettedStroke.HasSameAuthor(me) || targettedStroke.HasSamePrivacy(Privacy.Public))
                    AddStrokeToCanvas(new PrivateAwareStroke(targettedStroke.stroke.Clone(), strokeTarget));
            }
        }

        public void AddStrokeToCanvas(PrivateAwareStroke stroke)
        {
            // stroke already exists on the canvas, don't do anything
            if (Work.Strokes.Where(s => s.tag().id == stroke.tag().id).Count() != 0)
                return;
            //change privacy of stroke if it is different from the canvasAlignedPrivacy
            if (canvasAlignedPrivacy(stroke.privacy()) != stroke.privacy())
            {
                var oldTag = stroke.tag();
                var newStroke = stroke.Clone();
                newStroke.tag(new StrokeTag(oldTag.author, canvasAlignedPrivacy(stroke.privacy()), oldTag.id, oldTag.startingSum, stroke.DrawingAttributes.IsHighlighter, oldTag.timestamp));
                stroke = newStroke;
            }
            var bounds = stroke.GetBounds();
            if (bounds.X < 0 || bounds.Y < 0)
            {
                contentBuffer.AddStroke(stroke, (st) =>
                {
                    Work.Strokes.Add(st);
                    RefreshCanvas();
                });
            }
            else
                contentBuffer.AddStroke(stroke, (st) => Work.Strokes.Add(st));
        }

        private double ReturnPositiveValue(double x)
        {
            return Math.Abs(x);
        }
        private PrivateAwareStroke OffsetNegativeCartesianStrokeTranslate(PrivateAwareStroke stroke)
        {
            var newStroke = stroke.Clone();
            var transformMatrix = new Matrix();
            transformMatrix.Translate(stroke.offsetX, stroke.offsetY);
            newStroke.Transform(transformMatrix, false);
            newStroke.offsetX = 0;
            newStroke.offsetY = 0;
            return newStroke;
        }

        private void RemoveExistingStrokeFromCanvas(InkCanvas canvas, Stroke stroke)
        {
            if (canvas.Strokes.Count == 0)
                return;

            var deadStrokes = new StrokeCollection(canvas.Strokes.Where(s => s.tag().id == stroke.tag().id));
            canvas.Strokes.Remove(deadStrokes);
        }

        private void addStrokes(IEnumerable<PrivateAwareStroke> strokes)
        {
            foreach (var stroke in strokes)
            {
                doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
            }
        }
        private void removeStrokes(IEnumerable<PrivateAwareStroke> strokes)
        {
            foreach (var stroke in strokes)
            {
                contentBuffer.RemoveStroke(stroke, (st) => RemoveExistingStrokeFromCanvas(Work, st));
                // deletion handled by move delta batch delete
                //doMyStrokeRemovedExceptHistory(stroke);
            }
        }
        public void deleteSelectedItems(object obj)
        {
            if (CanvasHasActiveFocus())
                deleteSelectedElements(null, null);
        }

        private UndoHistory.HistoricalAction changeSelectedInkPrivacy(IEnumerable<PrivateAwareStroke> selectedStrokes, Privacy newPrivacy, Privacy oldPrivacy)
        {
            Action redo = () =>
            {
                var newStrokes = new List<PrivateAwareStroke>();
                foreach (var stroke in selectedStrokes.Where(i => i != null && i.tag().privacy != newPrivacy))
                {
                    // stroke exists on canvas
                    var strokesToUpdate = Work.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == stroke.tag().id).Select(s => s as PrivateAwareStroke);
                    if (strokesToUpdate.Count() > 0)
                    {
                        contentBuffer.RemoveStroke(strokesToUpdate.First(), (col) => Work.Strokes.Remove(col));
                        // dirty is now handled by move delta
                        //doMyStrokeRemovedExceptHistory(stroke);
                    }

                    var tmpStroke = stroke.Clone();
                    tmpStroke.tag(new StrokeTag(tmpStroke.tag(), newPrivacy));
                    var newStroke = tmpStroke;
                    //new PrivateAwareStroke(tmpStroke, _target);
                    if (Work.Strokes.Where(s => s.tag().id == newStroke.tag().id).Count() == 0)
                    {
                        newStrokes.Add(newStroke);
                        contentBuffer.AddStroke(newStroke, (str) => Work.Strokes.Add(str));
                        //doMyStrokeAddedExceptHistory(newStroke, newPrivacy);
                    }
                }
                Dispatcher.adopt(() => Work.Select(Work.EditingMode == InkCanvasEditingMode.Select ? new StrokeCollection(newStrokes.Select(s => s as Stroke)) : new StrokeCollection()));
            };
            Action undo = () =>
            {
                foreach (var stroke in selectedStrokes.Where(i => i.tag().privacy != newPrivacy))
                {
                    // stroke exists on canvas
                    var strokesToUpdate = Work.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == stroke.tag().id).Select(s => s as PrivateAwareStroke);
                    if (strokesToUpdate.Count() > 0)
                    {
                        contentBuffer.RemoveStroke(strokesToUpdate.First(), (col) => Work.Strokes.Remove(col));
                        // dirty is now handled by move delta
                        //doMyStrokeRemovedExceptHistory(stroke);
                    }

                    var tmpStroke = stroke.Clone();
                    tmpStroke.tag(new StrokeTag(tmpStroke.tag(), oldPrivacy));
                    var newStroke = tmpStroke;
                    //var newStroke = new PrivateAwareStroke(tmpStroke, _target);
                    if (Work.Strokes.Where(s => s.tag().id == newStroke.tag().id).Count() == 0)
                    {
                        contentBuffer.AddStroke(newStroke, (str) => Work.Strokes.Add(str));
                        //doMyStrokeAddedExceptHistory(newStroke, oldPrivacy);
                    }
                }
            };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Change selected ink privacy");
        }
        private UndoHistory.HistoricalAction changeSelectedImagePrivacy(IEnumerable<MeTLImage> selectedElements, Privacy newPrivacy, Privacy oldPrivacy)
        {
            Action redo = () =>
            {
                foreach (var image in selectedElements.Where(i => i.tag().privacy != newPrivacy))
                {
                    // dirty handled by move delta
                    //Commands.SendDirtyImage.ExecuteAsync(new TargettedDirtyElement (Globals.slide, image.tag().author, _target, image.tag().privacy, image.tag().id));

                    var imagesToUpdate = Work.ImageChildren().Where(i => i.tag().id == image.tag().id);
                    if (imagesToUpdate.Count() > 0)
                    {
                        contentBuffer.RemoveImage(imagesToUpdate.First(), (img) => Work.Children.Remove(img));
                    }

                    var tmpImage = image.Clone();
                    tmpImage.tag(new ImageTag(tmpImage.tag(), newPrivacy));
                    if (Work.ImageChildren().Where(i => i.tag().id == tmpImage.tag().id).Count() == 0)
                    {
                        contentBuffer.AddImage(tmpImage, (img) => Work.Children.Add(tmpImage));
                        //sendImageWithoutHistory(tmpImage, tmpImage.tag().privacy);
                    }
                    tmpImage.ApplyPrivacyStyling(contentBuffer, _target, newPrivacy);
                }
            };
            Action undo = () =>
            {
                foreach (var image in selectedElements.Where(i => i.tag().privacy != newPrivacy))
                {
                    // dirty handled by move delta
                    //Commands.SendDirtyImage.ExecuteAsync(new TargettedDirtyElement (Globals.slide, image.tag().author, _target, image.tag().privacy, image.tag().id));
                    var imagesToUpdate = Work.ImageChildren().Where(i => i.tag().id == image.tag().id);
                    if (imagesToUpdate.Count() > 0)
                    {
                        contentBuffer.RemoveImage(imagesToUpdate.First(), (img) => Work.Children.Remove(img));
                    }

                    var tmpImage = image.Clone();
                    tmpImage.tag(new ImageTag(tmpImage.tag(), oldPrivacy));
                    if (Work.ImageChildren().Where(i => i.tag().id == tmpImage.tag().id).Count() == 0)
                    {
                        contentBuffer.AddImage(tmpImage, (img) => Work.Children.Add(tmpImage));
                        //sendImageWithoutHistory(tmpImage, tmpImage.tag().privacy);
                    }
                    tmpImage.ApplyPrivacyStyling(contentBuffer, _target, oldPrivacy);
                }
            };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Image selection privacy changed");
        }
        private UndoHistory.HistoricalAction changeSelectedTextPrivacy(IEnumerable<MeTLTextBox> selectedElements, Privacy newPrivacy, Privacy oldPrivacy)
        {
            Action redo = () =>
            {
                foreach (var text in selectedElements.Where(i => i.tag().privacy != newPrivacy).Cast<MeTLTextBox>())
                {
                    // let move delta handle the dirty
                    //dirtyTextBoxWithoutHistory(textBox);
                    var textToUpdate = Work.TextChildren().Where(t => t.tag().id == text.tag().id);
                    if (textToUpdate.Count() > 0)
                    {
                        contentBuffer.RemoveTextBox(textToUpdate.First(), (txt) => Work.Children.Remove(txt));
                    }

                    var tmpText = text.clone();
                    tmpText.tag(new TextTag(tmpText.tag(), newPrivacy));
                    if (Work.TextChildren().Where(t => t.tag().id == tmpText.tag().id).Count() == 0)
                    {
                        contentBuffer.AddTextBox(tmpText, (txt) => Work.Children.Add(tmpText));
                        //sendTextWithoutHistory(tmpText, tmpText.tag().privacy);
                    }
                    tmpText.ApplyPrivacyStyling(contentBuffer, _target, newPrivacy);
                }
            };
            Action undo = () =>
            {
                foreach (var text in selectedElements.Where(t => t.tag().privacy != newPrivacy).Cast<MeTLTextBox>())
                {
                    // let move delta handle the dirty
                    /*if(Work.TextChildren().ToList().Where(tb => ((MeTLTextBox)tb).tag().id == box.tag().id).ToList().Count != 0)
                        dirtyTextBoxWithoutHistory((MeTLTextBox)Work.TextChildren().ToList().Where(tb => ((MeTLTextBox)tb).tag().id == box.tag().id).ToList().First());*/
                    var textToUpdate = Work.TextChildren().Where(t => t.tag().id == text.tag().id);
                    if (textToUpdate.Count() > 0)
                    {
                        contentBuffer.RemoveTextBox(textToUpdate.First(), (txt) => Work.Children.Remove(txt));
                    }
                    var tmpText = text.clone();
                    tmpText.tag(new TextTag(tmpText.tag(), oldPrivacy));
                    if (Work.TextChildren().Where(t => t.tag().id == tmpText.tag().id).Count() == 0)
                    {
                        contentBuffer.AddTextBox(tmpText, (txt) => Work.Children.Add(tmpText));
                        //sendTextWithoutHistory(tmpText, tmpText.tag().privacy);
                    }
                    tmpText.ApplyPrivacyStyling(contentBuffer, _target, oldPrivacy);
                }
            };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Text selection changed privacy");
        }
        private void changeSelectedItemsPrivacy(Privacy newPrivacy)
        {
            long timestamp = -1L;

            // can't change content privacy for notepad
            if (me == Globals.PROJECTOR || _target == "notepad") return;

            ClearAdorners();
            List<UIElement> selectedElements = null;
            List<PrivateAwareStroke> selectedStrokes = null;

            Dispatcher.adopt(() =>
            {
                selectedElements = Work.GetSelectedElements().ToList();
                selectedStrokes = Work.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke).ToList();
            });

            var oldPrivacy = newPrivacy.InvertPrivacy();

            var selectedTextBoxes = selectedElements.OfType<MeTLTextBox>();
            var selectedImages = selectedElements.OfType<MeTLImage>();

            var ink = changeSelectedInkPrivacy(selectedStrokes, newPrivacy, oldPrivacy);
            var images = changeSelectedImagePrivacy(selectedImages, newPrivacy, oldPrivacy);
            var text = changeSelectedTextPrivacy(selectedTextBoxes, newPrivacy, oldPrivacy);

            Action redo = () =>
                {
                    var identity = Globals.generateId(Guid.NewGuid().ToString());
                    //var moveDelta = TargettedMoveDelta.Create(Globals.slide, Globals.me, _target, currentPrivacy, timestamp, selectedStrokes, selectedTextBoxes, selectedImages);
                    var moveDelta = TargettedMoveDelta.Create(Globals.slide, Globals.me, _target, oldPrivacy, identity, timestamp, selectedStrokes.Select(s => s as Stroke), selectedTextBoxes.Select(s => s as TextBox), selectedImages.Select(s => s as Image));
                    moveDelta.newPrivacy = newPrivacy;
                    var mdb = ContentBuffer.getLogicalBoundsOfContent(selectedImages.ToList(), selectedTextBoxes.ToList(), selectedStrokes);
                    moveDelta.xOrigin = mdb.Left;
                    moveDelta.yOrigin = mdb.Top;
                    Commands.SendMoveDelta.ExecuteAsync(moveDelta);

                    ink.redo();
                    text.redo();
                    images.redo();

                    ClearAdorners();
                    Work.Focus();
                };
            Action undo = () =>
                {
                    var identity = Globals.generateId(Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(Globals.slide, Globals.me, _target, oldPrivacy, identity, timestamp, selectedStrokes.Select(s => s as Stroke), selectedTextBoxes.Select(s => s as TextBox), selectedImages.Select(s => s as Image));
                    moveDelta.newPrivacy = oldPrivacy;
                    var mdb = ContentBuffer.getLogicalBoundsOfContent(selectedImages.ToList(), selectedTextBoxes.ToList(), selectedStrokes);
                    moveDelta.xOrigin = mdb.Left;
                    moveDelta.yOrigin = mdb.Top;
                    moveDeltaProcessor.rememberSentMoveDelta(moveDelta);
                    Commands.SendMoveDelta.ExecuteAsync(moveDelta);

                    ink.undo();
                    text.undo();
                    images.undo();

                    ClearAdorners();
                    Work.Focus();
                };
            redo();
            UndoHistory.Queue(undo, redo, "Selected items changed privacy");
        }

        public void RefreshCanvas()
        {
            Work.Children.Clear();
            Work.Strokes.Clear();

            contentBuffer.AdjustContent();
            ReAddFilteredContent(contentBuffer.CurrentContentVisibility);
        }
        private void singleStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var checksum = e.Stroke.sum().checksum;
            var timestamp = 0L;
            e.Stroke.tag(new StrokeTag(Globals.me, currentPrivacy, checksum.ToString(), checksum, e.Stroke.DrawingAttributes.IsHighlighter, timestamp));
            var privateAwareStroke = new PrivateAwareStroke(e.Stroke, _target);
            privateAwareStroke.offsetX = contentBuffer.logicalX;
            privateAwareStroke.offsetY = contentBuffer.logicalY;
            Work.Strokes.Remove(e.Stroke);
            privateAwareStroke.startingSum(checksum);
            doMyStrokeAdded(privateAwareStroke);
            Commands.RequerySuggested(Commands.Undo);
        }
        public void doMyStrokeAdded(PrivateAwareStroke stroke)
        {
            doMyStrokeAdded(stroke, currentPrivacy);
        }
        public void doMyStrokeAdded(PrivateAwareStroke stroke, Privacy intendedPrivacy)
        {
            intendedPrivacy = canvasAlignedPrivacy(intendedPrivacy);
            var thisStroke = stroke.Clone();
            Action undo = () =>
                {
                    ClearAdorners();
                    var existingStrokes = Work.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == thisStroke.tag().id).Select(s => (s as PrivateAwareStroke).Clone());
                    foreach (var existingStroke in existingStrokes)
                    {
                        if (existingStroke != null)
                        {
                            contentBuffer.RemoveStroke(existingStroke, (str) =>
                            {
                                Work.Strokes.Remove(str);
                            });
                            doMyStrokeRemovedExceptHistory(existingStroke);
                        }
                    }
                };
            Action redo = () =>
                {
                    ClearAdorners();
                    if (Work.Strokes.Where(s => s.tag().id == thisStroke.tag().id).Count() == 0)
                    {
                        var bounds = thisStroke.GetBounds();
                        if (bounds.X < 0 || bounds.Y < 0)
                        {
                            contentBuffer.AddStroke(thisStroke, (st) =>
                            {
                                Work.Strokes.Add(st);
                                RefreshCanvas();
                            });
                        }
                        else
                            contentBuffer.AddStroke(thisStroke, (st) => Work.Strokes.Add(st));
                        doMyStrokeAddedExceptHistory(thisStroke, thisStroke.tag().privacy);
                    }
                    if (Work.EditingMode == InkCanvasEditingMode.Select)
                        Work.Select(new StrokeCollection(new[] { thisStroke }));
                    AddAdorners();
                };
            UndoHistory.Queue(undo, redo, String.Format("Added stroke [{0}]", thisStroke.tag().id));
            //redo();
            doMyStrokeAddedExceptHistory(stroke, intendedPrivacy);
        }
        private void erasingStrokes(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            try
            {
                if (e.Stroke.tag().author != me)
                {
                    e.Cancel = true;
                    return;
                }
                Trace.TraceInformation("ErasingStroke {0}", e.Stroke.tag().id);
                doMyStrokeRemoved(e.Stroke as PrivateAwareStroke);
            }
            catch
            {
                //Tag can be malformed if app state isn't fully logged in
            }
        }

        private void doMyStrokeRemoved(PrivateAwareStroke stroke)
        {
            ClearAdorners();
            var canvas = Work;
            var undo = new Action(() =>
                                 {
                                     // if stroke doesn't exist on the canvas 
                                     if (canvas.Strokes.Where(s => s.tag().id == stroke.tag().id).Count() == 0)
                                     {
                                         contentBuffer.AddStroke(stroke, (col) => canvas.Strokes.Add(col));
                                         doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                                     }
                                 });
            var redo = new Action(() =>
                                 {
                                     var strokesToRemove = canvas.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == stroke.tag().id).Select(s => s as PrivateAwareStroke);
                                     if (strokesToRemove.Count() > 0)
                                     {
                                         contentBuffer.RemoveStroke(strokesToRemove.First(), (col) => canvas.Strokes.Remove(col));
                                         doMyStrokeRemovedExceptHistory(stroke);
                                     }
                                 });
            redo();
            UndoHistory.Queue(undo, redo, String.Format("Deleted stroke [{0}]", stroke.tag().id));
        }

        private void doMyStrokeRemovedExceptHistory(Stroke stroke)
        {
            var strokeTag = stroke.tag();
            Commands.SendDirtyStroke.Execute(new TargettedDirtyElement(Globals.slide, strokeTag.author, _target, strokeTag.privacy, strokeTag.id, strokeTag.timestamp));
        }
        private void doMyStrokeAddedExceptHistory(PrivateAwareStroke stroke, Privacy thisPrivacy)
        {
            var oldTag = stroke.tag();
            stroke.tag(new StrokeTag(oldTag.author, canvasAlignedPrivacy(thisPrivacy), oldTag.id, oldTag.startingSum, stroke.DrawingAttributes.IsHighlighter, oldTag.timestamp));
            SendTargettedStroke(stroke, canvasAlignedPrivacy(thisPrivacy));
        }

        public void SendTargettedStroke(PrivateAwareStroke stroke, Privacy thisPrivacy)
        {
            if (!stroke.shouldPersist()) return;
            //Offset the negative co-ordinates before sending the stroke off the wire
            var translatedStroke = OffsetNegativeCartesianStrokeTranslate(stroke);
            var privateRoom = string.Format("{0}{1}", Globals.slide, translatedStroke.tag().author);
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != translatedStroke.tag().author)
                Commands.SneakInto.Execute(privateRoom);
            Commands.SendStroke.Execute(new TargettedStroke(Globals.slide, translatedStroke.tag().author, _target, translatedStroke.tag().privacy, translatedStroke.tag().id, translatedStroke.tag().timestamp, translatedStroke, translatedStroke.tag().startingSum));
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != stroke.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);
        }
        #endregion
        #region Images

        private void sendImage(MeTLImage newImage)
        {
            newImage.UpdateLayout();
            Commands.SendImage.Execute(new TargettedImage(Globals.slide, me, _target, newImage.tag().privacy, newImage.tag().id, newImage, newImage.tag().timestamp));
        }
        private void dirtyImage(MeTLImage imageToDirty)
        {
            imageToDirty.ApplyPrivacyStyling(contentBuffer, _target, imageToDirty.tag().privacy);
            Commands.SendDirtyImage.Execute(new TargettedDirtyElement(Globals.slide, Globals.me, _target, imageToDirty.tag().privacy, imageToDirty.tag().id, imageToDirty.tag().timestamp));
        }
        public void ReceiveMoveDelta(TargettedMoveDelta moveDelta, bool processHistory = false)
        {
            if (_target == "presentationSpace")
            {
                // don't want to duplicate what has already been done locally
                moveDeltaProcessor.ReceiveMoveDelta(moveDelta, me, processHistory);
                /*
                var inkIds = moveDelta.inkIds.Select(elemId => elemId.Identity).ToList();
                if(inkIds.Count > 0)
                  contentBuffer.adjustStrokesForMoveDelta(inkIds);

                var imageIds = moveDelta.imageIds.Select(elemId => elemId.Identity).ToList();
                if(imageIds.Count > 0)
                  contentBuffer.adjustImagesForMoveDelta(imageIds);

                var textIds = moveDelta.textIds.Select(elemId => elemId.Identity).ToList();
                if(textIds.Count > 0)
                  contentBuffer.adjustTextsForMoveDelta(textIds);
                 */
            }
        }

        public void ReceiveImages(IEnumerable<TargettedImage> images)
        {
            foreach (var image in images)
            {
                if (image.slide == Globals.slide && image.HasSameTarget(_target))
                {
                    TargettedImage image1 = image;
                    if (image.HasSameAuthor(me) || image.HasSamePrivacy(Privacy.Public))
                    {
                        Dispatcher.adoptAsync(() =>
                        {
                            try
                            {
                                var receivedImage = image1.imageSpecification.forceEvaluation();
                                //image.clone();
                                AddImage(Work, receivedImage);
                                receivedImage.ApplyPrivacyStyling(contentBuffer, _target, receivedImage.tag().privacy);
                            }
                            catch (Exception)
                            {
                            }
                        });
                    }
                }
            }
        }
        private void AddImage(InkCanvas canvas, MeTLImage image)
        {
            if (canvas.ImageChildren().Any(i => ((MeTLImage)i).tag().id == image.tag().id)) return;
            //NegativeCartesianImageTranslate(image);
            contentBuffer.AddImage(image, (img) =>
            {
                var imageToAdd = img as MeTLImage;
                var zIndex = imageToAdd.tag().isBackground ? -5 : 2;

                Canvas.SetZIndex(img, zIndex);
                canvas.Children.Add(img);
            });
        }
        /*
                private MeTLImage NegativeCartesianImageTranslate(MeTLImage incomingImage)
                {
                    return contentBuffer.adjustImage(incomingImage, (i) =>
                    {
                        var translateX = ReturnPositiveValue(contentBuffer.logicalX);
                        var translateY = ReturnPositiveValue(contentBuffer.logicalY);
                        InkCanvas.SetLeft(i, (InkCanvas.GetLeft(i) + translateX));
                        InkCanvas.SetTop(i, (InkCanvas.GetTop(i) + translateY));
                        i.offsetX = contentBuffer.logicalX;
                        i.offsetY = contentBuffer.logicalY;
                        return i;
                    });
                }
                */
        private MeTLImage OffsetNegativeCartesianImageTranslate(MeTLImage image)
        {
            var newImage = image.Clone();
            InkCanvas.SetLeft(newImage, (InkCanvas.GetLeft(newImage) + newImage.offsetX));//contentBuffer.logicalX));
            InkCanvas.SetTop(newImage, (InkCanvas.GetTop(newImage) + newImage.offsetY));//contentBuffer.logicalY));
            newImage.offsetX = 0;
            newImage.offsetY = 0;
            return newImage;
        }

        public void ReceiveDirtyImage(TargettedDirtyElement element)
        {
            if (!(element.target.Equals(_target))) return;
            if (element.slide != Globals.slide) return;
            Dispatcher.adoptAsync(() => dirtyImage(element.identity));
        }

        private void dirtyImage(string imageId)
        {
            var imagesToRemove = new List<Image>();
            foreach (var currentImage in Work.Children.OfType<Image>())
            {
                if (imageId.Equals(currentImage.tag().id))
                    imagesToRemove.Add(currentImage);
            }

            foreach (var removeImage in imagesToRemove)
            {
                contentBuffer.RemoveImage(removeImage, (img) => Work.Children.Remove(removeImage));
            }
        }

        #endregion
        #region imagedrop
        private void addImageFromDisk(object obj)
        {
            addResourceFromDisk((files) =>
            {
                var origin = new Point(0, 0);
                int i = 0;
                foreach (var file in files)
                    handleDrop(file, origin, true, i++, (source, offset, count) => { return offset; });
            });
        }

        private void imagesDropped(List<ImageDrop> images)
        {
            foreach (var image in images)
                imageDropped(image);
        }
        private void imageDropped(ImageDrop drop)
        {
            try
            {
                if (drop.Target.Equals(_target) && me != Globals.PROJECTOR)
                    handleDrop(drop.Filename, new Point(0, 0), drop.OverridePoint, drop.Position, (source, offset, count) => { return drop.Point; });
            }
            catch (NotSetException)
            {
                //YAY
            }
        }
        private void addResourceFromDisk(Action<IEnumerable<string>> withResources)
        {
            const string filter = "Image files(*.jpeg;*.gif;*.bmp;*.jpg;*.png)|*.jpeg;*.gif;*.bmp;*.jpg;*.png|All files (*.*)|*.*";
            addResourceFromDisk(filter, withResources);
        }

        private void addResourceFromDisk(string filter, Action<IEnumerable<string>> withResources)
        {
            if (_target == "presentationSpace" && me != Globals.PROJECTOR)
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

        protected void ImageDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (fileNames == null) return;
            foreach (string fileName in fileNames)
            {
                FileType type = GetFileType(fileName);
                if (new[] { FileType.Image }.Contains(type))
                    e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }
        protected void ImagesDrop(object sender, DragEventArgs e)
        {
            if (me == Globals.PROJECTOR) return;
            var validFormats = e.Data.GetFormats();
            var fileNames = new string[0];
            validFormats.Select(vf =>
            {
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
                    else throw new Exception("data was in an unexpected format: (" + outputData.GetType() + ") - " + outputData);
                }
                catch (Exception ex)
                {
                    outputData = "getData failed with exception (" + ex.Message + ")";
                }
                return vf + ":  " + outputData;
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
                MeTLMessage.Information("Cannot drop this onto the canvas");
                return;
            }
            Commands.SetLayer.ExecuteAsync("Insert");
            var pos = e.GetPosition(this);
            var origin = new Point(pos.X, pos.Y);
            var maxHeight = 0d;

            Func<Image, Point, int, Point> positionUpdate = (source, offset, count) =>
                {
                    if (count == 0)
                    {
                        pos.Offset(offset.X, offset.Y);
                        origin.Offset(offset.X, offset.Y);
                    }

                    var curPos = new Point(pos.X, pos.Y);
                    pos.X += source.Source.Width + source.Margin.Left * 2 + 30;
                    if (source.Source.Height + source.Margin.Top * 2 > maxHeight)
                        maxHeight = source.Source.Height + source.Margin.Top * 2;
                    if ((count + 1) % 4 == 0)
                    {
                        pos.X = origin.X;
                        pos.Y += (maxHeight + 30);
                        maxHeight = 0.0;
                    }

                    return curPos;
                };
            //lets try for a 4xN grid
            for (var i = 0; i < fileNames.Count(); i++)
            {
                var filename = fileNames[i];
                handleDrop(filename, origin, true, i, positionUpdate);
            }
            e.Handled = true;
        }
        protected void ImageDragOverCancel(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        public void handleDrop(string fileName, Point origin, bool overridePoint, int count, Func<Image, Point, int, Point> positionUpdate)
        {
            FileType type = GetFileType(fileName);
            origin = new Point(origin.X + contentBuffer.logicalX, origin.Y + contentBuffer.logicalY);
            switch (type)
            {
                case FileType.Image:
                    dropImageOnCanvas(fileName, origin, count, overridePoint, positionUpdate);
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
            if (info.Length > size * MEGABYTE)
            {
                return false;
            }
            return true;
        }
        private int fileSizeLimit = 50;
        private void uploadFileForUse(string unMangledFilename)
        {
            string filePart = Path.GetFileName(unMangledFilename);
            string filename = LocalFileProvider.getUserFile(new string[] { }, filePart + ".MeTLFileUpload");
            if (filename.Length > 260)
            {
                MeTLMessage.Warning("Sorry, your filename is too long, must be less than 260 characters");
                return;
            }
            if (isFileLessThanXMB(unMangledFilename, fileSizeLimit))
            {
                var worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                 {
                     File.Copy(unMangledFilename, filename);
                     App.controller.client.UploadAndSendFile(
                         new MeTLStanzas.LocalFileInformation(Globals.slide, Globals.me, _target, Privacy.Public, -1L, filename, Path.GetFileNameWithoutExtension(filename), false, new FileInfo(filename).Length, DateTimeFactory.Now().Ticks.ToString(), Globals.generateId(filename)));
                     File.Delete(filename);
                 };
                worker.RunWorkerCompleted += (s, a) => Dispatcher.Invoke(DispatcherPriority.Send,
                                                                                   (Action)(() => MeTLMessage.Information(string.Format("Finished uploading {0}.", unMangledFilename))));
                worker.RunWorkerAsync();
            }
            else
            {
                MeTLMessage.Warning(String.Format("Sorry, your file is too large, must be less than {0}mb", fileSizeLimit));
                return;
            }
        }
        public void dropImageOnCanvas(string fileName, Point origin, int count, bool useDefaultMargin, Func<Image, Point, int, Point> positionUpdate)
        {
            if (!isFileLessThanXMB(fileName, fileSizeLimit))
            {
                MeTLMessage.Warning(String.Format("Sorry, your file is too large, must be less than {0}mb", fileSizeLimit));
                return;
            }
            Dispatcher.adoptAsync(() =>
            {
                var imagePos = new Point(0, 0);
                MeTLImage image = null;
                try
                {
                    image = createImageFromUri(new Uri(fileName, UriKind.RelativeOrAbsolute), useDefaultMargin);

                    if (useDefaultMargin)
                    {
                        if (imagePos.X < 50)
                            imagePos.X = 50;
                        if (imagePos.Y < 50)
                            imagePos.Y = 50;
                    }

                    imagePos = positionUpdate(image, imagePos, count);
                }
                catch (Exception e)
                {
                    MeTLMessage.Warning("Sorry could not create an image from this file :" + fileName + "\n Error: " + e.Message);
                    return;
                }
                if (image == null)
                    return;
                // center the image horizonally if there is no margin set. this is only used for dropping quiz result images on the canvas
                if (!useDefaultMargin)
                {
                    imagePos.X = (Globals.DefaultCanvasSize.Width / 2) - ((image.Width + (Globals.QuizMargin * 2)) / 2);
                    //pos.Y = (Globals.DefaultCanvasSize.Height / 2) - (image.Height / 2);
                }
                InkCanvas.SetLeft(image, imagePos.X);
                InkCanvas.SetTop(image, imagePos.Y);
                image.tag(new ImageTag(Globals.me, currentPrivacy, Globals.generateId(), false, -1L));
                var currentSlide = Globals.slide;
                var translatedImage = OffsetNegativeCartesianImageTranslate(image);

                Action undo = () =>
                                  {
                                      ClearAdorners();
                                      contentBuffer.RemoveImage(translatedImage, (img) => Work.Children.Remove(img));
                                      dirtyImage(translatedImage);
                                  };
                Action redo = () =>
                {
                    ClearAdorners();
                    if (!fileName.StartsWith("http"))
                        App.controller.client.UploadAndSendImage(new MeTLStanzas.LocalImageInformation(currentSlide, Globals.me, _target, currentPrivacy, translatedImage, fileName, false));
                    else
                        sendImage(translatedImage);

                    contentBuffer.AddImage(translatedImage, (img) =>
                    {
                        if (!Work.Children.Contains(img))
                            Work.Children.Add(img);
                    });
                    Work.Select(new[] { translatedImage });
                    AddAdorners();
                };
                redo();
                UndoHistory.Queue(undo, redo, "Dropped Image");
            });
        }

        public MeTLImage createImageFromUri(Uri uri, bool useDefaultMargin)
        {
            var image = new MeTLImage();

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            //var newImageHeight = bitmapImage.DecodePixelHeight;
            var newImageHeight = bitmapImage.PixelHeight;
            //var newImageWidth = bitmapImage.DecodePixelWidth;
            var newImageWidth = bitmapImage.PixelWidth;
            image.Source = bitmapImage;
            // images with a high dpi eg 300 were being drawn relatively small compared to images of similar size with dpi ~100
            // which is correct but not what people expect.
            // image size is determined from dpi

            // next two lines were used
            //image.Height = bitmapImage.Height;
            //image.Width = bitmapImage.Width;

            // image size will match reported size
            //image.Height = jpgFrame.PixelHeight;
            //image.Width = jpgFrame.PixelWidth;
            //image.Stretch = Stretch.Uniform;
            image.Stretch = Stretch.Fill;
            image.StretchDirection = StretchDirection.Both;
            image.Height = newImageHeight;
            image.Width = newImageWidth;
            if (useDefaultMargin)
            {
                image.Margin = new Thickness(5);
            }
            else
            {
                image.Margin = new Thickness(Globals.QuizMargin);
            }
            return image;
        }

        public static FileType GetFileType(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLower();
            var imageExtensions = new List<string>() { ".jpg", ".jpeg", ".bmp", ".gif", ".png", ".dib" };
            var videoExtensions = new List<string>() { ".wmv" };

            if (imageExtensions.Contains(extension))
                return FileType.Image;
            if (videoExtensions.Contains(extension))
                return FileType.Video;
            return FileType.NotSupported;
        }

        #endregion
        //text
        #region Text
        private void placeCursor(object sender, MouseButtonEventArgs e)
        {
            if (Work.EditingMode != InkCanvasEditingMode.None) return;
            if (me == Globals.PROJECTOR) return;
            var pos = e.GetPosition(this);
            MeTLTextBox box = createNewTextbox();
            AddTextBoxToCanvas(box, true);
            InkCanvas.SetLeft(box, pos.X);
            InkCanvas.SetTop(box, pos.Y);
            myTextBox = box;
            box.Focus();
        }
        private void AddTextboxToMyCanvas(MeTLTextBox box)
        {
            contentBuffer.AddTextBox(applyDefaultAttributes(box), (text) => Work.Children.Add(text));
        }
        public void DoText(TargettedTextBox targettedBox)
        {
            Dispatcher.adoptAsync(delegate
                                      {
                                          if (targettedBox.target != _target) return;
                                          if (targettedBox.slide == Globals.slide &&
                                              (targettedBox.HasSamePrivacy(Privacy.Public) || targettedBox.HasSameAuthor(me)))
                                          {
                                              var box = UpdateTextBoxWithId(targettedBox);
                                              if (box != null)
                                              {
                                                  if (!(targettedBox.HasSameAuthor(me) && _focusable))
                                                      box.Focusable = false;
                                                  box.ApplyPrivacyStyling(contentBuffer, _target, targettedBox.privacy);
                                              }
                                          }
                                      });

        }

        private MeTLTextBox UpdateTextBoxWithId(TargettedTextBox textbox)
        {
            // find textbox if it exists, otherwise create it
            var createdNew = false;
            var box = textBoxFromId(textbox.identity);
            if (box == null)
            {
                box = textbox.box.toMeTLTextBox();
                createdNew = true;
            }

            var oldBox = textbox.box;
            // update with changes
            box.AcceptsReturn = true;
            box.TextWrapping = TextWrapping.WrapWithOverflow;
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.tag(oldBox.tag());
            box.FontFamily = oldBox.FontFamily;
            box.FontStyle = oldBox.FontStyle;
            box.FontWeight = oldBox.FontWeight;
            box.TextDecorations = oldBox.TextDecorations;
            box.FontSize = oldBox.FontSize;
            box.Foreground = oldBox.Foreground;
            box.TextChanged -= SendNewText;
            var caret = box.CaretIndex;
            box.Text = oldBox.Text;
            box.CaretIndex = caret;
            box.TextChanged += SendNewText;
            box.Width = oldBox.Width;
            //box.Height = OldBox.Height;
            /*InkCanvas.SetLeft(box, InkCanvas.GetLeft(oldBox));
            InkCanvas.SetTop(box, InkCanvas.GetTop(oldBox));*/

            if (createdNew)
                AddTextBoxToCanvas(box, true);
            else
                contentBuffer.adjustText(box, t => t);
            return box;
        }

        private MeTLTextBox applyDefaultAttributes(MeTLTextBox box)
        {
            // make sure these aren't doubled up
            box.GotFocus -= textboxGotFocus;
            box.LostFocus -= textboxLostFocus;
            box.PreviewKeyDown -= box_PreviewTextInput;
            box.PreviewMouseRightButtonUp -= box_PreviewMouseRightButtonUp;

            box.TextChanged -= SendNewText;
            box.AcceptsReturn = true;
            box.TextWrapping = TextWrapping.WrapWithOverflow;
            box.GotFocus += textboxGotFocus;
            box.LostFocus += textboxLostFocus;
            box.PreviewKeyDown += box_PreviewTextInput;
            box.TextChanged += SendNewText;
            box.IsUndoEnabled = false;
            box.UndoLimit = 0;
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.Focusable = CanFocus;
            box.ContextMenu = new ContextMenu { IsEnabled = true };
            box.ContextMenu.IsEnabled = false;
            box.ContextMenu.IsOpen = false;
            box.PreviewMouseRightButtonUp += box_PreviewMouseRightButtonUp;
            return box;
        }

        private void box_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private MeTLTextBox FindOrCreateTextBoxFromId(MeTLTextBox textbox)
        {
            var box = textBoxFromId(textbox.tag().id);
            if (box == null)
            {
                box = textbox.clone();
                AddTextBoxToCanvas(box, true);
            }

            /* 
             InkCanvas.SetLeft(box, InkCanvas.GetLeft(textbox));
             InkCanvas.SetTop(box, InkCanvas.GetTop(textbox));
             */

            return box;
        }
        private MeTLTextBox UpdateTextBoxWithId(MeTLTextBox textbox, string newText)
        {
            // find textbox if it exists, otherwise create it
            var box = FindOrCreateTextBoxFromId(textbox);
            box.TextChanged -= SendNewText;
            box.Text = newText;
            box.CaretIndex = textbox.CaretIndex;
            box.TextChanged += SendNewText;

            return box;
        }
        private void SendNewText(object sender, TextChangedEventArgs e)
        {
            if (_originalText == null) return;
            if (me == Globals.PROJECTOR) return;
            var box = (MeTLTextBox)sender;
            var undoText = _originalText.Clone().ToString();
            var redoText = box.Text.Clone().ToString();
            box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy);
            box.Height = Double.NaN;
            var mybox = box.clone();
            var undoBox = box.clone();
            var redoBox = box.clone();
            Action undo = () =>
            {
                ClearAdorners();
                var myText = undoText;
                var updatedBox = UpdateTextBoxWithId(mybox, myText);
                sendTextWithoutHistory(updatedBox, updatedBox.tag().privacy);
            };
            Action redo = () =>
            {
                ClearAdorners();
                var myText = redoText;
                var updatedBox = UpdateTextBoxWithId(redoBox, myText);
                sendTextWithoutHistory(updatedBox, updatedBox.tag().privacy);
            };
            UndoHistory.Queue(undo, redo, String.Format("Added text [{0}]", redoText));

            // only do the change locally and let the timer do the rest
            ClearAdorners();
            UpdateTextBoxWithId(mybox, redoText);

            var currentSlide = Globals.slide;
            Action typingTimedAction = () => Dispatcher.adoptAsync(delegate
                                                                       {
                                                                           var senderTextBox = sender as MeTLTextBox;
                                                                           sendTextWithoutHistory(senderTextBox, currentPrivacy, currentSlide);
                                                                           TypingTimer = null;
                                                                           GlobalTimers.ExecuteSync();
                                                                       });
            if (TypingTimer == null)
            {
                TypingTimer = new TypingTimedAction();
                TypingTimer.Add(typingTimedAction);
            }
            else
            {
                GlobalTimers.ResetSyncTimer();
                TypingTimer.ResetTimer();
            }
        }
        private void resetTextbox(object obj)
        {
            if (myTextBox == null && Work.GetSelectedElements().Count != 1) return;
            if (myTextBox == null)
                myTextBox = (MeTLTextBox)Work.GetSelectedElements().Where(b => b is MeTLTextBox).First();

            var currentTextBox = filterOnlyMine<MeTLTextBox>(myTextBox);
            if (currentTextBox == null)
                return;

            var undoInfo = getInfoOfBox(currentTextBox);
            Action undo = () =>
            {
                ClearAdorners();
                applyStylingTo(currentTextBox, undoInfo);
                sendTextWithoutHistory(currentTextBox, currentTextBox.tag().privacy);
                updateTools();
            };
            Action redo = () =>
                              {
                                  ClearAdorners();
                                  resetText(currentTextBox);
                                  updateTools();
                              };
            UndoHistory.Queue(undo, redo, "Restored text defaults");
            redo();
        }
        private void resetText(MeTLTextBox box)
        {
            box.RemovePrivacyStyling(contentBuffer);
            _currentColor = Colors.Black;
            box.FontWeight = FontWeights.Normal;
            box.FontStyle = FontStyles.Normal;
            box.TextDecorations = new TextDecorationCollection();
            box.FontFamily = new FontFamily("Arial");
            box.FontSize = 24;
            box.Foreground = Brushes.Black;
            var info = new TextInformation
                           {
                               Family = box.FontFamily,
                               Size = box.FontSize,
                           };
            Commands.TextboxFocused.ExecuteAsync(info);
            sendTextWithoutHistory(box, box.tag().privacy);
        }
        private void updateStyling(TextInformation info)
        {
            try
            {
                var selectedTextBoxes = new List<MeTLTextBox>();
                selectedTextBoxes.AddRange(Work.GetSelectedElements().OfType<MeTLTextBox>());
                if (filterOnlyMine<MeTLTextBox>(myTextBox) != null)
                    selectedTextBoxes.Add(myTextBox);
                var selectedTextBox = selectedTextBoxes.FirstOrDefault(); // only support changing style for one textbox at a time

                if (selectedTextBox != null)
                {
                    // create a clone of the selected textboxes and their textinformation so we can keep a reference to something that won't be changed
                    var clonedTextBox = selectedTextBox.clone();
                    var clonedTextInfo = getInfoOfBox(selectedTextBox);

                    Action undo = () =>
                      {
                          //ClearAdorners();

                          // find the textboxes on the canvas, if they've been deleted recreate and add to the canvas again
                          var activeTextbox = FindOrCreateTextBoxFromId(clonedTextBox);
                          if (activeTextbox != null)
                          {
                              var activeTextInfo = clonedTextInfo;
                              activeTextbox.TextChanged -= SendNewText;
                              applyStylingTo(activeTextbox, activeTextInfo);
                              Commands.TextboxFocused.ExecuteAsync(activeTextInfo);
                              //AddAdorners();
                              sendTextWithoutHistory(activeTextbox, activeTextbox.tag().privacy);
                              activeTextbox.TextChanged += SendNewText;
                          }
                      };
                    Action redo = () =>
                      {
                          //ClearAdorners();

                          var activeTextbox = FindOrCreateTextBoxFromId(clonedTextBox);
                          if (activeTextbox != null)
                          {
                              var activeTextInfo = info;
                              activeTextbox.TextChanged -= SendNewText;
                              applyStylingTo(activeTextbox, activeTextInfo);
                              Commands.TextboxFocused.ExecuteAsync(activeTextInfo);
                              //AddAdorners();
                              sendTextWithoutHistory(activeTextbox, activeTextbox.tag().privacy);
                              activeTextbox.TextChanged += SendNewText;
                          }
                      };
                    UndoHistory.Queue(undo, redo, "Styling of text changed");
                    redo();
                }
            }
            catch (Exception e)
            {
                Logger.Fixed(string.Format("There was an ERROR:{0} INNER:{1}, it is now fixed", e, e.InnerException));
            }

        }
        private static void applyStylingTo(MeTLTextBox currentTextBox, TextInformation info)
        {
            currentTextBox.FontStyle = info.Italics ? FontStyles.Italic : FontStyles.Normal;
            currentTextBox.FontWeight = info.Bold ? FontWeights.Bold : FontWeights.Normal;
            currentTextBox.TextDecorations = new TextDecorationCollection();
            if (info.Underline)
                currentTextBox.TextDecorations = TextDecorations.Underline;
            else if (info.Strikethrough)
                currentTextBox.TextDecorations = TextDecorations.Strikethrough;
            currentTextBox.FontSize = info.Size;
            currentTextBox.FontFamily = info.Family;
            currentTextBox.Foreground = new SolidColorBrush(info.Color);
        }
        private static TextInformation getInfoOfBox(MeTLTextBox box)
        {
            var underline = false;
            var strikethrough = false;
            if (box.TextDecorations.Count > 0)
            {
                underline = box.TextDecorations.First().Location.ToString().ToLower() == "underline";
                strikethrough = box.TextDecorations.First().Location.ToString().ToLower() == "strikethrough";
            }
            return new TextInformation
                       {
                           Bold = box.FontWeight == FontWeights.Bold,
                           Italics = box.FontStyle == FontStyles.Italic,
                           Size = box.FontSize,
                           Underline = underline,
                           Strikethrough = strikethrough,
                           Family = box.FontFamily,
                           Color = ((SolidColorBrush)box.Foreground).Color
                       };
        }

        private void sendBox(MeTLTextBox box, bool localOnly = false)
        {
            myTextBox = box;
            if (!Work.Children.ToList().Any(c => c is MeTLTextBox && ((MeTLTextBox)c).tag().id == box.tag().id))
                AddTextBoxToCanvas(box, true);
            box.PreviewKeyDown += box_PreviewTextInput;
            if (!localOnly)
            {
                sendTextWithoutHistory(box, box.tag().privacy);
            }
        }

        /*private void NegativeCartesianTextTranslate(MeTLTextBox incomingBox)
        {
            contentBuffer.adjustText(incomingBox, (t) =>
            {
                var translateX = ReturnPositiveValue(contentBuffer.logicalX);
                var translateY = ReturnPositiveValue(contentBuffer.logicalY);
                InkCanvas.SetLeft(t, (InkCanvas.GetLeft(t) + translateX));
                InkCanvas.SetTop(t, (InkCanvas.GetTop(t) + translateY));
                t.offsetX = contentBuffer.logicalX;
                t.offsetY = contentBuffer.logicalY;
                return t;
            });
        }
         */

        private MeTLTextBox OffsetNegativeCartesianTextTranslate(MeTLTextBox box)
        {
            //var newBox = (box as MeTLTextBox).clone();
            var newBox = box.clone();
            InkCanvas.SetLeft(newBox, (InkCanvas.GetLeft(newBox) + newBox.offsetX));
            InkCanvas.SetTop(newBox, (InkCanvas.GetTop(newBox) + newBox.offsetY));
            newBox.Height = (Double.IsNaN(box.Height) || box.Height <= 0) ? box.ActualHeight : box.Height;
            newBox.Width = (Double.IsNaN(box.Width) || box.Width <= 0) ? box.ActualWidth : box.Width;
            newBox.offsetX = 0;
            newBox.offsetY = 0;
            return newBox;
        }

        public void sendTextWithoutHistory(MeTLTextBox box, Privacy thisPrivacy)
        {
            sendTextWithoutHistory(box, thisPrivacy, Globals.slide);
        }

        public void sendTextWithoutHistory(MeTLTextBox box, Privacy thisPrivacy, int slide)
        {
            thisPrivacy = canvasAlignedPrivacy(thisPrivacy);

            //Dirty the text box if its privacy does not match the canvas aligned privacy
            if (box.tag().privacy != currentPrivacy)
                dirtyTextBoxWithoutHistory(box, false);
            var oldTextTag = box.tag();
            var newTextTag = new TextTag(oldTextTag.author, thisPrivacy, oldTextTag.id, oldTextTag.timestamp);
            box.tag(newTextTag);
            var translatedTextBox = OffsetNegativeCartesianTextTranslate(box);
            var privateRoom = string.Format("{0}{1}", Globals.slide, translatedTextBox.tag().author);
            /*
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != translatedTextBox.tag().author)
                Commands.SneakInto.Execute(privateRoom);
             */
            Commands.SendTextBox.ExecuteAsync(new TargettedTextBox(slide, translatedTextBox.tag().author, _target, thisPrivacy, translatedTextBox.tag().id, translatedTextBox, translatedTextBox.tag().timestamp));
            /*
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != translatedTextBox.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);
             */
            //NegativeCartesianTextTranslate(translatedTextBox);
            /*var privateRoom = string.Format("{0}{1}", Globals.slide, box.tag().author);
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != box.tag().author)
                Commands.SneakInto.Execute(privateRoom);
            Commands.SendTextBox.ExecuteAsync(new TargettedTextBox(slide, box.tag().author, _target, thisPrivacy, box.tag().id, box, box.tag().timestamp));
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != box.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);*/
        }

        private void dirtyTextBoxWithoutHistory(MeTLTextBox box, bool removeLocal = true)
        {
            dirtyTextBoxWithoutHistory(box, Globals.slide, removeLocal);
        }

        private void dirtyTextBoxWithoutHistory(MeTLTextBox box, int slide, bool removeLocal = true)
        {
            box.RemovePrivacyStyling(contentBuffer);
            if (removeLocal)
                RemoveTextBoxWithMatchingId(box.tag().id);
            Commands.SendDirtyText.ExecuteAsync(new TargettedDirtyElement(slide, box.tag().author, _target, canvasAlignedPrivacy(box.tag().privacy), box.tag().id, box.tag().timestamp));
        }

        public void sendImageWithoutHistory(MeTLImage image, Privacy thisPrivacy)
        {
            sendImageWithoutHistory(image, thisPrivacy, Globals.slide);
        }

        public void sendImageWithoutHistory(MeTLImage image, Privacy thisPrivacy, int slide)
        {
            var privateRoom = string.Format("{0}{1}", Globals.slide, image.tag().author);
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != image.tag().author)
                Commands.SneakInto.Execute(privateRoom);
            Commands.SendImage.ExecuteAsync(new TargettedImage(Globals.slide, image.tag().author, _target, thisPrivacy, image.tag().id, image, image.tag().timestamp));
            if (thisPrivacy == Privacy.Private && Globals.isAuthor && me != image.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);
        }

        private void box_PreviewTextInput(object sender, KeyEventArgs e)
        {
            _originalText = ((MeTLTextBox)sender).Text;
            e.Handled = false;
        }

        private void textboxLostFocus(object sender, RoutedEventArgs e)
        {
            var box = (MeTLTextBox)sender;
            ClearAdorners();
            myTextBox = null;
            requeryTextCommands();
            if (box.Text.Length == 0)
            {
                if (TextBoxExistsOnCanvas(box, true))
                    dirtyTextBoxWithoutHistory(box);
            }
            else
                setAppropriatePrivacyHalo(box);
        }

        private void setAppropriatePrivacyHalo(MeTLTextBox box)
        {
            if (!Work.Children.Contains(box)) return;
            box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy);
        }

        private static void requeryTextCommands()
        {
            Commands.RequerySuggested(new[]{
                                                Commands.UpdateTextStyling,
                                                Commands.RestoreTextDefaults
                                            });
        }
        private void textboxGotFocus(object sender, RoutedEventArgs e)
        {
            if (((MeTLTextBox)sender).tag().author != me) return; //cannot edit other peoples textboxes
            if (me != Globals.PROJECTOR)
            {
                myTextBox = (MeTLTextBox)sender;
            }
            CommandManager.InvalidateRequerySuggested();
            if (myTextBox == null)
                return;
            updateTools();
            requeryTextCommands();
            _originalText = myTextBox.Text;
            Commands.ChangeTextMode.ExecuteAsync("None");
        }
        private void updateTools()
        {
            var info = new TextInformation
            {
                Family = _defaultFamily,
                Size = DefaultSize,
                Bold = false,
                Italics = false,
                Strikethrough = false,
                Underline = false,
                Color = Colors.Black
            };
            if (myTextBox != null)
            {
                info = new TextInformation
                {
                    Family = myTextBox.FontFamily,
                    Size = myTextBox.FontSize,
                    Bold = myTextBox.FontWeight == FontWeights.Bold,
                    Italics = myTextBox.FontStyle == FontStyles.Italic,
                    Color = ((SolidColorBrush)myTextBox.Foreground).Color,
                    Target = _target
                };

                if (myTextBox.TextDecorations.Count > 0)
                {
                    info.Strikethrough = myTextBox.TextDecorations.First().Location.ToString().ToLower() == "strikethrough";
                    info.Underline = myTextBox.TextDecorations.First().Location.ToString().ToLower() == "underline";
                }
                info.IsPrivate = myTextBox.tag().privacy == Privacy.Private ? true : false;
                //Commands.TextboxFocused.ExecuteAsync(info);
                Commands.TextboxSelected.ExecuteAsync(info);
            }

        }

        private bool TextBoxExistsOnCanvas(MeTLTextBox box, bool privacyMatches)
        {
            var result = false;
            Dispatcher.adopt(() =>
            {
                var boxId = box.tag().id;
                var privacy = box.tag().privacy;
                foreach (var text in Work.Children)
                    if (text is MeTLTextBox)
                        if (((MeTLTextBox)text).tag().id == boxId && ((MeTLTextBox)text).tag().privacy == privacy) result = true;
            });
            return result;
        }

        private bool alreadyHaveThisTextBox(MeTLTextBox box)
        {
            return TextBoxExistsOnCanvas(box, true);
        }

        private void RemoveTextBoxWithMatchingId(string id)
        {
            var removeTextbox = textBoxFromId(id);
            if (removeTextbox != null)
            {
                contentBuffer.RemoveTextBox(removeTextbox, (tb) => Work.Children.Remove(tb));
            }
        }
        private void receiveDirtyText(TargettedDirtyElement element)
        {
            if (!(element.target.Equals(_target))) return;
            if (element.slide != Globals.slide) return;
            Dispatcher.adoptAsync(delegate
            {
                if (myTextBox != null && element.HasSameIdentity(myTextBox.tag().id)) return;
                if (element.author == me) return;
                RemoveTextBoxWithMatchingId(element.identity);
            });
        }

        private bool TargettedTextBoxIsFocused(TargettedTextBox targettedBox)
        {
            return myTextBox != null && myTextBox.tag().id == targettedBox.identity;

            /*var focusedTextBox = Keyboard.FocusedElement as MeTLTextBox;
            if (focusedTextBox != null && focusedTextBox.tag().id == targettedBox.identity)
            {
                return true;
            }
            return false;*/
        }

        public void ReceiveTextBox(TargettedTextBox targettedBox)
        {
            if (targettedBox.target != _target) return;

            if (me != Globals.PROJECTOR && TargettedTextBoxIsFocused(targettedBox))
                return;

            /*if (targettedBox.HasSameAuthor(me) && alreadyHaveThisTextBox(targettedBox.box.toMeTLTextBox()) && me != Globals.PROJECTOR)
            {
                DoText(targettedBox);
                return;
            }*/
            //I never want my live text to collide with me.
            if (targettedBox.slide == Globals.slide && ((targettedBox.HasSamePrivacy(Privacy.Private) && !targettedBox.HasSameAuthor(me)) || me == Globals.PROJECTOR))
                RemoveTextBoxWithMatchingId(targettedBox.identity);
            if (targettedBox.slide == Globals.slide && ((targettedBox.HasSamePrivacy(Privacy.Public) || (targettedBox.HasSameAuthor(me)) && me != Globals.PROJECTOR)))
                DoText(targettedBox);
        }

        private bool CanvasHasActiveFocus()
        {
            return Globals.CurrentCanvasClipboardFocus == _target;
        }

        private MeTLTextBox textBoxFromId(string boxId)
        {
            MeTLTextBox result = null;
            var boxes = new List<MeTLTextBox>();
            boxes.AddRange(Work.Children.OfType<MeTLTextBox>());
            Dispatcher.adopt(() =>
            {
                result = boxes.Where(text => text.tag().id == boxId).FirstOrDefault();
            });
            return result;
        }
        public bool CanHandleClipboardPaste()
        {
            return CanvasHasActiveFocus();
        }

        public bool CanHandleClipboardCut()
        {
            return CanvasHasActiveFocus();
        }

        public bool CanHandleClipboardCopy()
        {
            return CanvasHasActiveFocus();
        }

        public void OnClipboardPaste()
        {
            HandlePaste(null);
        }
        public MeTLTextBox createNewTextbox()
        {
            var box = new MeTLTextBox();
            box.offsetX = contentBuffer.logicalX;
            box.offsetY = contentBuffer.logicalY;
            box.tag(new TextTag
                        {
                            author = Globals.me,
                            privacy = currentPrivacy,
                            id = string.Format("{0}:{1}", Globals.me, DateTimeFactory.Now().Ticks)
                        });

            //setting the currentfamily, currentsize, currentcolor, style whenever there is a new box created
            _currentColor = Globals.currentTextInfo.Color;
            _currentSize = Globals.currentTextInfo.Size;
            _currentFamily = Globals.currentTextInfo.Family;

            box.FontStyle = Globals.currentTextInfo.Italics ? FontStyles.Italic : FontStyles.Normal;
            box.FontWeight = Globals.currentTextInfo.Bold ? FontWeights.Bold : FontWeights.Normal;
            box.TextDecorations = new TextDecorationCollection();
            if (Globals.currentTextInfo.Underline)
                box.TextDecorations = TextDecorations.Underline;
            else if (Globals.currentTextInfo.Strikethrough)
                box.TextDecorations = TextDecorations.Strikethrough;

            box.FontFamily = _currentFamily;
            box.FontSize = _currentSize;
            box.Foreground = new SolidColorBrush(_currentColor);
            box.UndoLimit = 0;
            box.LostFocus += (_sender, _args) =>
            {
                myTextBox = null;

            };
            return applyDefaultAttributes(box);
        }
        public void OnClipboardCopy()
        {
            HandleCopy(null);
        }

        public void OnClipboardCut()
        {
            HandleCut(null);
        }
        public void HandleInkPasteUndo(List<Stroke> newStrokes)
        {
            //var selection = new StrokeCollection();
            ClearAdorners();
            foreach (var stroke in newStrokes)
            {
                if (Work.Strokes.Where(s => s.tag().id == stroke.tag().id).Count() > 0) /* MeTLMath.ApproxEqual(s.sum().checksum, stroke.sum().checksum)).Count() > 0)*/
                {
                    //selection = new StrokeCollection(selection.Where(s => !MeTLMath.ApproxEqual(s.sum().checksum, stroke.sum().checksum)));
                    doMyStrokeRemovedExceptHistory(stroke);
                }
            }
        }
        public void HandleInkPasteRedo(List<PrivateAwareStroke> newStrokes)
        {
            //var selection = new StrokeCollection();              
            ClearAdorners();
            foreach (var stroke in newStrokes)
            {
                if (Work.Strokes.Where(s => s.tag().id == stroke.tag().id)/*MeTLMath.ApproxEqual(s.sum().checksum, stroke.sum().checksum))*/.Count() == 0)
                {
                    //stroke.tag(new StrokeTag(stroke.tag().author, privacy, stroke.tag().startingSum, stroke.tag().isHighlighter));
                    //selection.Add(stroke);
                    var clonedStroke = stroke.Clone();
                    var oldTag = clonedStroke.tag();
                    var identity = Globals.generateId(Guid.NewGuid().ToString());
                    clonedStroke.tag(new StrokeTag(oldTag.author, oldTag.privacy, identity, oldTag.startingSum, stroke.DrawingAttributes.IsHighlighter, oldTag.timestamp));
                    doMyStrokeAddedExceptHistory(clonedStroke, clonedStroke.tag().privacy);
                }
            }
        }
        private List<MeTLImage> createImages(List<BitmapSource> selectedImages)
        {
            var images = new List<MeTLImage>();
            foreach (var imageSource in selectedImages)
            {
                var tmpFile = LocalFileProvider.getUserFile(new string[] { }, "tmpImage.png");
                using (FileStream fileStream = new FileStream(tmpFile, FileMode.OpenOrCreate))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(imageSource));
                    encoder.Save(fileStream);
                }
                if (File.Exists(tmpFile))
                {
                    var uri =
                        App.controller.client.NoAuthUploadResource(
                            new Uri(tmpFile, UriKind.RelativeOrAbsolute), Globals.slide);
                    var image = new MeTLImage
                                    {
                                        Source = new BitmapImage(uri),
                                        Width = imageSource.Width,
                                        Height = imageSource.Height,
                                        Stretch = Stretch.Fill
                                    };
                    image.tag(new ImageTag(Globals.me, currentPrivacy, Globals.generateId(), false, -1L, -1)); // ZIndex was -1, timestamp is -1L
                    InkCanvas.SetLeft(image, 15);
                    InkCanvas.SetTop(image, 15);
                    images.Add(image);
                }
            }
            return images;
        }
        private void HandleImagePasteRedo(List<MeTLImage> selectedImages)
        {
            foreach (var image in selectedImages)
                Commands.SendImage.ExecuteAsync(new TargettedImage(Globals.slide, Globals.me, _target, currentPrivacy, image.tag().id, image, image.tag().timestamp));
        }
        private void HandleImagePasteUndo(List<MeTLImage> selectedImages)
        {
            foreach (var image in selectedImages)
                dirtyImage(image);
        }
        private MeTLTextBox setWidthOf(MeTLTextBox box)
        {
            if (box.Text.Length > 540)
                box.Width = 540;
            return box;
        }
        private void HandleTextPasteRedo(List<MeTLTextBox> selectedText, MeTLTextBox currentBox)
        {
            foreach (var textBox in selectedText)
            {
                if (currentBox != null)
                {
                    var caret = currentBox.CaretIndex;
                    var redoText = currentBox.Text.Insert(currentBox.CaretIndex, textBox.Text);
                    ClearAdorners();
                    var box = textBoxFromId(currentBox.tag().id);
                    if (box == null)
                    {
                        AddTextBoxToCanvas(currentBox, true);
                        box = currentBox;
                    }
                    box.TextChanged -= SendNewText;
                    box.Text = redoText;
                    box.CaretIndex = caret + textBox.Text.Length;
                    box = setWidthOf(box);
                    sendTextWithoutHistory(box, box.tag().privacy);
                    box.TextChanged += SendNewText;
                    myTextBox = null;
                }
                else
                {
                    textBox.tag(new TextTag(textBox.tag().author, currentPrivacy, textBox.tag().id, textBox.tag().timestamp));
                    var box = setWidthOf(textBox);
                    AddTextBoxToCanvas(box, true);
                    sendTextWithoutHistory(box, box.tag().privacy);
                }
            }
        }
        private void HandleTextPasteUndo(List<MeTLTextBox> selectedText, MeTLTextBox currentBox)
        {
            foreach (var text in selectedText)
            {
                if (currentBox != null)
                {
                    var undoText = currentBox.Text;
                    var caret = currentBox.CaretIndex;
                    var currentTextBox = currentBox.clone();
                    var box = ((MeTLTextBox)Work.TextChildren().ToList().FirstOrDefault(c => ((MeTLTextBox)c).tag().id == currentTextBox.tag().id));
                    box.TextChanged -= SendNewText;
                    box.Text = undoText;
                    box.CaretIndex = caret;
                    sendTextWithoutHistory(box, box.tag().privacy);
                    box.TextChanged += SendNewText;
                }
                else
                    dirtyTextBoxWithoutHistory(text);
            }
        }
        private List<MeTLTextBox> createPastedBoxes(List<string> selectedText)
        {
            var boxes = new List<MeTLTextBox>();
            foreach (var text in selectedText)
            {
                System.Threading.Thread.Sleep(2);
                MeTLTextBox box = createNewTextbox();
                InkCanvas.SetLeft(box, pos.X);
                InkCanvas.SetTop(box, pos.Y);
                box.Width = 200;
                box.Height = Double.NaN;
                pos = new Point(15, 15);
                box.TextChanged -= SendNewText;
                box.Text = text;
                box.TextChanged += SendNewText;
                boxes.Add(box);
            }
            return boxes;
        }
        protected void HandlePaste(object _args)
        {
            if (me == Globals.PROJECTOR) return;
            var currentBox = myTextBox.clone();

            if (Clipboard.ContainsData(MeTLClipboardData.Type))
            {
                var data = (MeTLClipboardData)Clipboard.GetData(MeTLClipboardData.Type);
                var currentChecksums = Work.Strokes.Select(s => s.sum().checksum);
                var ink = data.Ink.Where(s => !currentChecksums.Contains(s.sum().checksum)).Select(s => new PrivateAwareStroke(s, _target)).ToList();
                var boxes = createPastedBoxes(data.Text.ToList());
                var images = createImages(data.Images.ToList());
                Action undo = () =>
                                  {
                                      ClearAdorners();
                                      HandleInkPasteUndo(ink.Select(s => s as Stroke).ToList());
                                      HandleImagePasteUndo(images);
                                      HandleTextPasteUndo(boxes, currentBox);
                                      AddAdorners();
                                  };
                Action redo = () =>
                                  {
                                      ClearAdorners();
                                      HandleInkPasteRedo(ink);
                                      HandleImagePasteRedo(images);
                                      HandleTextPasteRedo(boxes, currentBox);
                                      AddAdorners();
                                  };
                UndoHistory.Queue(undo, redo, "Pasted items");
                redo();
            }
            else
            {
                if (Clipboard.ContainsText())
                {
                    var boxes = createPastedBoxes(new List<string> { Clipboard.GetText() });
                    Action undo = () => HandleTextPasteUndo(boxes, currentBox);
                    Action redo = () => HandleTextPasteRedo(boxes, currentBox);
                    UndoHistory.Queue(undo, redo, "Pasted text");
                    redo();
                }
                else if (Clipboard.ContainsImage())
                {
                    Action undo = () => HandleImagePasteUndo(createImages(new List<BitmapSource> { Clipboard.GetImage() }));
                    Action redo = () => HandleImagePasteRedo(createImages(new List<BitmapSource> { Clipboard.GetImage() }));
                    UndoHistory.Queue(undo, redo, "Pasted images");
                    redo();
                }
            }
        }
        private List<string> HandleTextCopyRedo(List<UIElement> selectedBoxes, string selectedText)
        {
            var clipboardText = new List<string>();
            if (selectedText != String.Empty && selectedText.Length > 0)
                clipboardText.Add(selectedText);
            clipboardText.AddRange(selectedBoxes.Where(b => b is MeTLTextBox).Select(b => ((MeTLTextBox)b).Text).Where(t => t != selectedText));
            return clipboardText;
        }
        private IEnumerable<BitmapSource> HandleImageCopyRedo(List<UIElement> selectedImages)
        {
            return selectedImages.Where(i => i is Image).Select(i => (BitmapSource)((Image)i).Source);
        }
        private List<Stroke> HandleStrokeCopyRedo(List<Stroke> selectedStrokes)
        {
            return selectedStrokes;
        }
        protected void HandleCopy(object _args)
        {
            if (me == Globals.PROJECTOR) return;
            //text 
            var selectedElements = filterOnlyMineExceptIfHammering(Work.GetSelectedElements()).ToList();
            var selectedStrokes = filterOnlyMineExceptIfHammering(Work.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke)).Select((s => s.Clone())).ToList();
            string selectedText = selectedText = myTextBox.SelectedText;

            // copy previously was an undoable action, ie restore the clipboard to what it previously was
            var images = HandleImageCopyRedo(selectedElements);
            var text = HandleTextCopyRedo(selectedElements, selectedText);
            var copiedStrokes = HandleStrokeCopyRedo(selectedStrokes.Select(s => s as Stroke).ToList());
            Clipboard.SetData(MeTLClipboardData.Type, new MeTLClipboardData(text, images, copiedStrokes));
        }
        private void HandleImageCutUndo(IEnumerable<MeTLImage> selectedImages)
        {
            var selection = new List<UIElement>();
            foreach (var element in selectedImages)
            {
                if (!Work.ImageChildren().Contains(element))
                    Work.Children.Add(element);
                sendImage(element);
                selection.Add(element);
            }
            Work.Select(selection);
        }
        private IEnumerable<BitmapSource> HandleImageCutRedo(IEnumerable<MeTLImage> selectedImages)
        {
            foreach (var img in selectedImages)
            {
                img.ApplyPrivacyStyling(contentBuffer, _target, img.tag().privacy);
                Work.Children.Remove(img);
                Commands.SendDirtyImage.Execute(new TargettedDirtyElement(Globals.slide, Globals.me, _target, canvasAlignedPrivacy(img.tag().privacy), img.tag().id, img.tag().timestamp));
            }
            return selectedImages.Select(i => (BitmapSource)i.Source);
        }
        protected void HandleTextCutUndo(IEnumerable<MeTLTextBox> selectedElements, MeTLTextBox currentTextBox)
        {
            if (currentTextBox != null && currentTextBox.SelectionLength > 0)
            {
                var text = currentTextBox.Text;
                var start = currentTextBox.SelectionStart;
                var length = currentTextBox.SelectionLength;
                if (!Work.TextChildren().Select(t => ((MeTLTextBox)t).tag().id).Contains(currentTextBox.tag().id))
                {
                    var box = applyDefaultAttributes(currentTextBox);
                    box.tag(new TextTag(box.tag().author, canvasAlignedPrivacy(box.tag().privacy), Globals.generateId(), box.tag().timestamp));
                    Work.Children.Add(box);

                }
                var activeTextbox = ((MeTLTextBox)Work.TextChildren().ToList().FirstOrDefault(c => ((MeTLTextBox)c).tag().id == currentTextBox.tag().id));
                activeTextbox.Text = text;
                activeTextbox.CaretIndex = start + length;
                sendTextWithoutHistory(currentTextBox, currentTextBox.tag().privacy);

            }
            else
            {
                var mySelectedElements = selectedElements.Where(t => t is MeTLTextBox).Select(t => ((MeTLTextBox)t).clone());
                foreach (var box in mySelectedElements)
                    sendBox(box);
            }
        }
        protected IEnumerable<string> HandleTextCutRedo(IEnumerable<MeTLTextBox> elements, MeTLTextBox currentTextBox)
        {
            var clipboardText = new List<string>();
            if (currentTextBox != null && currentTextBox.SelectionLength > 0)
            {
                var selection = currentTextBox.SelectedText;
                var start = currentTextBox.SelectionStart;
                var length = currentTextBox.SelectionLength;
                clipboardText.Add(selection);
                var activeTextbox = ((MeTLTextBox)Work.TextChildren().ToList().Where(c => ((MeTLTextBox)c).tag().id == currentTextBox.tag().id).FirstOrDefault());
                if (activeTextbox == null) return clipboardText;
                activeTextbox.Text = activeTextbox.Text.Remove(start, length);
                activeTextbox.CaretIndex = start;
                if (activeTextbox.Text.Length == 0)
                {
                    ClearAdorners();
                    myTextBox = null;
                    dirtyTextBoxWithoutHistory(currentTextBox);
                }
            }
            else
            {
                var listToCut = new List<MeTLTextBox>();
                var selectedElements = elements.Where(t => t is MeTLTextBox).Select(tb => ((MeTLTextBox)tb).clone()).ToList();
                foreach (MeTLTextBox box in selectedElements)
                {
                    clipboardText.Add(box.Text);
                    listToCut.Add(box);
                }
                myTextBox = null;
                foreach (var element in listToCut)
                    dirtyTextBoxWithoutHistory(element);
            }
            return clipboardText;
        }
        private void HandleInkCutUndo(IEnumerable<PrivateAwareStroke> strokesToCut)
        {
            foreach (var s in strokesToCut)
            {
                Work.Strokes.Add(s);
                doMyStrokeAddedExceptHistory(s, s.tag().privacy);
            }
        }
        private IEnumerable<PrivateAwareStroke> HandleInkCutRedo(IEnumerable<PrivateAwareStroke> selectedStrokes)
        {
            var listToCut = selectedStrokes.Select(stroke => new TargettedDirtyElement(Globals.slide, stroke.tag().author, _target, canvasAlignedPrivacy(stroke.tag().privacy), stroke.tag().id /* stroke.sum().checksum.ToString()*/, stroke.tag().timestamp)).ToList();
            foreach (var element in listToCut)
                Commands.SendDirtyStroke.Execute(element);
            return selectedStrokes.ToList();
        }
        protected void HandleCut(object _args)
        {
            if (me == Globals.PROJECTOR) return;
            var strokesToCut = filterOnlyMineExceptIfHammering(Work.GetSelectedStrokes().OfType<PrivateAwareStroke>()).Select(s => s.Clone());
            var currentTextBox = myTextBox.clone();
            var selectedImages = filterOnlyMineExceptIfHammering(Work.GetSelectedImages()).Select(i => i.Clone());
            var selectedText = filterOnlyMineExceptIfHammering(Work.GetSelectedTextBoxes()).Select(t => t.clone());

            Action redo = () =>
            {
                ClearAdorners();
                var text = HandleTextCutRedo(selectedText, currentTextBox);
                var images = HandleImageCutRedo(selectedImages);
                var ink = HandleInkCutRedo(strokesToCut);
                Clipboard.SetData(MeTLClipboardData.Type, new MeTLClipboardData(text, images, ink.Select(s => s as Stroke).ToList()));
            };
            Action undo = () =>
            {
                ClearAdorners();
                if (Clipboard.ContainsData(MeTLClipboardData.Type))
                {
                    Clipboard.GetData(MeTLClipboardData.Type);
                    HandleTextCutUndo(selectedText, currentTextBox);
                    HandleImageCutUndo(selectedImages);
                    HandleInkCutUndo(strokesToCut);
                }
            };
            redo();
            UndoHistory.Queue(undo, redo, "Cut items");
        }
        #endregion
        private void MoveTo(int _slide)
        {
            if (contentBuffer != null)
            {
                contentBuffer.Clear();
            }
            if (moveDeltaProcessor != null)
            {
                moveDeltaProcessor.clearRememberedSentMoveDeltas();
            }
            if (myTextBox != null)
            {
                var textBox = myTextBox;
                textBox.Focusable = false;
            }
            myTextBox = null;
        }
        public void Flush()
        {
            ClearAdorners();
            Work.Strokes.Clear();
            Work.Children.Clear();
            Height = Double.NaN;
            Width = Double.NaN;
            //Negative Cartesian Resolution - Changing the co-ordinates to 0,0
            contentBuffer.logicalX = 0.0;
            contentBuffer.logicalY = 0.0;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CollapsedCanvasStackAutomationPeer(this);
        }
    }
}