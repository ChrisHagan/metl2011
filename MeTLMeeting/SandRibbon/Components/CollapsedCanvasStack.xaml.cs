using System;
using System.Collections.Generic;
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
using Awesomium.Windows.Controls;
using Awesomium.Core;
using SandRibbon.Pages.Collaboration.Layout;
using Xceed.Wpf.Toolkit;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;

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
        public static TypingTimedAction TypingTimer;
        public ContentBuffer contentBuffer
        {
            get;
            protected set;
        }
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
                    return rootPage.NetworkController.credentials.name;
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

        private Privacy currentPrivacy
        {
            get
            {
                return canvasAlignedPrivacy(rootPage.UserConversationState.Privacy);
            }
        }

        public SlideAwarePage rootPage { get; set; }
        public CollapsedCanvasStack()
        {
            InitializeComponent();
            wireInPublicHandlers();
            var deleteCommandBinding = new CommandBinding(ApplicationCommands.Delete, deleteSelectedElements, canExecute);
            var setInkCanvasModeCommand = new DelegateCommand<string>(setInkCanvasMode);
            var receiveStrokeCommand = new DelegateCommand<TargettedStroke>((stroke) => ReceiveStrokes(new[] { stroke }));
            var receiveStrokesCommand = new DelegateCommand<IEnumerable<TargettedStroke>>(ReceiveStrokes);
            var receiveDirtyStrokesCommand = new DelegateCommand<IEnumerable<TargettedDirtyElement>>(ReceiveDirtyStrokes);
            var zoomChangedCommand = new DelegateCommand<double>(ZoomChanged);
            var receiveImageCommand = new DelegateCommand<TargettedImage>((image) => ReceiveImages(new[] { image }));
            var receiveDirtyImageCommand = new DelegateCommand<TargettedDirtyElement>(ReceiveDirtyImage);
            var addImageCommand = new DelegateCommand<object>(addImageFromDisk);
            var receiveMoveDeltaCommand = new DelegateCommand<TargettedMoveDelta>((moveDelta) => { ReceiveMoveDelta(moveDelta); });
            var receiveTextBoxCommand = new DelegateCommand<TargettedTextBox>(ReceiveTextBox);
            var establishPrivilegesCommand = new DelegateCommand<string>(setInkCanvasMode);
            var setTextCanvasModeCommand = new DelegateCommand<string>(setInkCanvasMode);
            var receiveDirtyTextCommand = new DelegateCommand<TargettedDirtyElement>(receiveDirtyText);
            var setContentVisibilityCommand = new DelegateCommand<List<ContentVisibilityDefinition>>(SetContentVisibility);
            var extendCanvasBySizeCommand = new DelegateCommand<SizeWithTarget>(extendCanvasBySize);
            var extendCanvasUpCommand = new DelegateCommand<object>(extendCanvasUp);
            var extendCanvasDownCommand = new DelegateCommand<object>(extendCanvasDown);
            var imageDroppedCommand = new DelegateCommand<ImageDrop>(imageDropped);
            var imagesDroppedCommand = new DelegateCommand<List<ImageDrop>>(imagesDropped);
            var setLayerCommand = new DelegateCommand<string>(SetLayer);
            var deleteSelectedItemsCommand = new DelegateCommand<object>(deleteSelectedItems);
            var setPrivacyOfItemsCommand = new DelegateCommand<Privacy>(changeSelectedItemsPrivacy);
            var setDrawingAttributesCommand = new DelegateCommand<DrawingAttributes>(SetDrawingAttributes);
            var setPenAttributesCommand = new DelegateCommand<PenAttributes>(SetPenAttributes);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdateConversationDetails);
            var pasteCommandBinding = new CommandBinding(ApplicationCommands.Paste, (sender, args) => HandlePaste(args), canExecute);
            var copyCommandBinding = new CommandBinding(ApplicationCommands.Copy, (sender, args) => HandleCopy(args), canExecute);
            var cutCommandBinding = new CommandBinding(ApplicationCommands.Cut, (sender, args) => HandleCut(args), canExecute);
            var clipboardManagerCommand = new DelegateCommand<ClipboardAction>((action) => clipboardManager.OnClipboardAction(action));
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }                
                CommandBindings.Add(deleteCommandBinding);
                CommandBindings.Add(pasteCommandBinding);
                CommandBindings.Add(copyCommandBinding);
                CommandBindings.Add(cutCommandBinding);
                Commands.SetInkCanvasMode.RegisterCommand(setInkCanvasModeCommand);
                Commands.ReceiveStroke.RegisterCommand(receiveStrokeCommand);
                Commands.ReceiveStrokes.RegisterCommand(receiveStrokesCommand);
                Commands.ReceiveDirtyStrokes.RegisterCommand(receiveDirtyStrokesCommand);
                Commands.ZoomChanged.RegisterCommand(zoomChangedCommand);
                Commands.ReceiveImage.RegisterCommand(receiveImageCommand);
                Commands.ReceiveDirtyImage.RegisterCommand(receiveDirtyImageCommand);
                Commands.AddImage.RegisterCommand(addImageCommand);
                Commands.ReceiveMoveDelta.RegisterCommand(receiveMoveDeltaCommand);
                Commands.ReceiveTextBox.RegisterCommand(receiveTextBoxCommand);
                Commands.EstablishPrivileges.RegisterCommand(establishPrivilegesCommand);
                Commands.SetTextCanvasMode.RegisterCommand(setTextCanvasModeCommand);
                Commands.ReceiveDirtyText.RegisterCommand(receiveDirtyTextCommand);
                Commands.SetContentVisibility.RegisterCommand(setContentVisibilityCommand);
                Commands.ExtendCanvasBySize.RegisterCommand(extendCanvasBySizeCommand);
                Commands.ExtendCanvasUp.RegisterCommand(extendCanvasUpCommand);
                Commands.ExtendCanvasDown.RegisterCommand(extendCanvasDownCommand);
                Commands.ImageDropped.RegisterCommand(imageDroppedCommand);
                Commands.ImagesDropped.RegisterCommand(imagesDroppedCommand);
                Commands.SetLayer.RegisterCommand(setLayerCommand);
                Commands.DeleteSelectedItems.RegisterCommand(deleteSelectedItemsCommand);
                Commands.SetPrivacyOfItems.RegisterCommand(setPrivacyOfItemsCommand);
                Commands.SetDrawingAttributes.RegisterCommand(setDrawingAttributesCommand);
                Commands.SetPenAttributes.RegisterCommand(setPenAttributesCommand);
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.ClipboardManager.RegisterCommand(clipboardManagerCommand);
                clipboardManager.RegisterHandler(ClipboardAction.Paste, OnClipboardPaste, CanHandleClipboardPaste);
                clipboardManager.RegisterHandler(ClipboardAction.Cut, OnClipboardCut, CanHandleClipboardCut);
                clipboardManager.RegisterHandler(ClipboardAction.Copy, OnClipboardCopy, CanHandleClipboardCopy);
                Globals.CanvasClipboardFocusChanged += CanvasClipboardFocusChanged;
                Dispatcher.adopt(delegate
                {
                    if (_target == null)
                    {
                        _target = (string)FindResource("target");
                        _defaultPrivacy = (Privacy)FindResource("defaultPrivacy");

                        if (_target == "presentationSpace")
                        {
                            Globals.CurrentCanvasClipboardFocus = _target;
                        }                        
                    }
                    Contextualise();
                });
            };
            Unloaded += (s, e) =>
            {
                CommandBindings.Remove(deleteCommandBinding);
                CommandBindings.Remove(pasteCommandBinding);
                CommandBindings.Remove(copyCommandBinding);
                CommandBindings.Remove(cutCommandBinding);
                Commands.SetInkCanvasMode.UnregisterCommand(setInkCanvasModeCommand);
                Commands.ReceiveStroke.UnregisterCommand(receiveStrokeCommand);
                Commands.ReceiveStrokes.UnregisterCommand(receiveStrokesCommand);
                Commands.ReceiveDirtyStrokes.UnregisterCommand(receiveDirtyStrokesCommand);
                Commands.ZoomChanged.UnregisterCommand(zoomChangedCommand);
                Commands.ReceiveImage.UnregisterCommand(receiveImageCommand);
                Commands.ReceiveDirtyImage.UnregisterCommand(receiveDirtyImageCommand);
                Commands.AddImage.UnregisterCommand(addImageCommand);
                Commands.ReceiveMoveDelta.UnregisterCommand(receiveMoveDeltaCommand);
                Commands.ReceiveTextBox.UnregisterCommand(receiveTextBoxCommand);
                Commands.EstablishPrivileges.UnregisterCommand(establishPrivilegesCommand);
                Commands.SetTextCanvasMode.UnregisterCommand(setTextCanvasModeCommand);
                Commands.ReceiveDirtyText.UnregisterCommand(receiveDirtyTextCommand);
                Commands.SetContentVisibility.UnregisterCommand(setContentVisibilityCommand);
                Commands.ExtendCanvasBySize.UnregisterCommand(extendCanvasBySizeCommand);
                Commands.ImageDropped.UnregisterCommand(imageDroppedCommand);
                Commands.ImagesDropped.UnregisterCommand(imagesDroppedCommand);
                Commands.SetLayer.UnregisterCommand(setLayerCommand);
                Commands.DeleteSelectedItems.UnregisterCommand(deleteSelectedItemsCommand);
                Commands.SetPrivacyOfItems.UnregisterCommand(setPrivacyOfItemsCommand);
                Commands.SetDrawingAttributes.UnregisterCommand(setDrawingAttributesCommand);
                Commands.SetPenAttributes.UnregisterCommand(setPenAttributesCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.ClipboardManager.UnregisterCommand(clipboardManagerCommand);
                clipboardManager.ClearAllHandlers();
                Globals.CanvasClipboardFocusChanged -= CanvasClipboardFocusChanged;
            };

            //For development
            if (_target == "presentationSpace" && me != GlobalConstants.PROJECTOR)
                rootPage.UserConversationState.UndoHistory.ShowVisualiser(Window.GetWindow(this));
        }

        public void Contextualise()
        {
            contentBuffer = new ContentBuffer(rootPage);
            me = rootPage.NetworkController.credentials.name;
            if (moveDeltaProcessor == null)
            {
                moveDeltaProcessor = new StackMoveDeltaProcessor(Work, contentBuffer, _target, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
            }
            else
            {
                moveDeltaProcessor.contentBuffer = contentBuffer;
            }
            
        }

        double shift = 200;
        private void extendCanvasDown(object o)
        {
            Dispatcher.adopt(delegate
            {
                Height += shift;
            });
        }

        private void extendCanvasUp(object o)
        {
            Dispatcher.adopt(delegate
            {

                Height += shift;
                contentBuffer.logicalY -= shift;
                contentBuffer.AdjustContent();
            });
        }

        private void extendCanvasByFactor(SizeWithTarget obj)
        {
            if (_target == obj.Target)
            {
                Dispatcher.adopt(delegate
                {

                    Height = Height * obj.Height;
                    Width = Width * obj.Width;
                });
            }
        }

        private void ContentElementsChanged(object sender, EventArgs e)
        {
            AddAdorners();
            BroadcastRegions(sender as ContentBuffer);
        }

        private void BroadcastRegions(ContentBuffer buffer)
        {
            var regions = new List<SignedBounds>();
            regions.AddRange(buffer.strokeFilter.Strokes.Select(s => s.signedBounds()));
            Commands.SignedRegions.Execute(regions);
        }
        private Privacy canvasAlignedPrivacy(Privacy incomingPrivacy)
        {
            if (_target == "presentationSpace")
            {
                return incomingPrivacy;
            }
            else
                return _defaultPrivacy;
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
            Work.Drop += ImagesDrop;
            Loaded += (a, b) =>
            {
                MouseUp += (c, args) => placeCursor(this, args);
            };
            Work.MouseMove += mouseMove;
            Work.StylusMove += stylusMove;
            Work.IsKeyboardFocusWithinChanged += Work_IsKeyboardFocusWithinChanged;
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
        /*
        private void JoinConversation()
        {
            if (myTextBox != null)
            {
                myTextBox.LostFocus -= textboxLostFocus;
                myTextBox = null;
            }
        }
        */
        private void stylusMove(object sender, StylusEventArgs e)
        {
            GlobalTimers.ResetSyncTimer();
        }

        protected Point lastPosition = new Point(0, 0);
        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                GlobalTimers.ResetSyncTimer();
                if (currentMode == InkCanvasEditingMode.None || currentMode == InkCanvasEditingMode.GestureOnly)
                {
                    var newPos = e.GetPosition(Work);
                    var newX = newPos.X - lastPosition.X;
                    var newY = newPos.Y - lastPosition.Y;
                    Commands.MoveCanvasByDelta.Execute(new Point(newX, newY));
                    lastPosition = new Point(newPos.X, newPos.Y);
                }
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
                Dispatcher.adopt(delegate
                {

                    Height = newSize.Height;
                    Width = newSize.Width;
                });
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
            if (me.ToLower() == GlobalConstants.PROJECTOR) return;
            html.IsHitTestVisible = false;
            Work.IsHitTestVisible = true;
            Dispatcher.adopt(delegate
            {

                switch (newLayer)
                {
                    case "Select":
                        Work.EditingMode = InkCanvasEditingMode.Select;
                        Work.UseCustomCursor = Work.EditingMode == InkCanvasEditingMode.Ink;
                        Work.Cursor = Cursors.Arrow;
                        break;
                    case "View":
                        Work.EditingMode = InkCanvasEditingMode.GestureOnly;
                        Work.UseCustomCursor = Work.EditingMode == InkCanvasEditingMode.Ink;
                        Work.Cursor = Cursors.Hand;
                        break;
                    case "Text":
                        Work.EditingMode = InkCanvasEditingMode.None;
                        Work.UseCustomCursor = false;
                        break;
                    case "Insert":
                        Work.EditingMode = InkCanvasEditingMode.Select;
                        Work.UseCustomCursor = false;
                        Work.Cursor = Cursors.Arrow;
                        break;
                    case "Sketch":
                        Work.EditingMode = InkCanvasEditingMode.Ink;
                        Work.UseCustomCursor = true;
                        break;
                }
                setLayerForTextFor(Work);
            });
        }
        private void setLayerForTextFor(InkCanvas canvas)
        {
            var curMe = rootPage.NetworkController.credentials.name;

            foreach (var box in canvas.Children)
            {
                if (box.GetType() == typeof(MeTLTextBox))
                {
                    var tag = ((MeTLTextBox)box).tag();
                    ((MeTLTextBox)box).Focusable = tag.author == curMe;
                }
            }
            contentBuffer.UpdateAllTextBoxes((textBox) =>
            {
                var tag = textBox.tag();
                textBox.Focusable = tag.author == curMe;
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
            var selectedElements = elements.Where(t => t.tag().author == rootPage.NetworkController.credentials.name).Select(b => ((MeTLTextBox)b).clone()).ToList();
            Action undo = () =>
                              {
                                  myTextBox = selectedElements.LastOrDefault();
                                  foreach (var box in selectedElements)
                                  {
                                      if (!alreadyHaveThisTextBox(box))
                                          AddTextBoxToCanvas(box, false);
                                      sendTextWithoutHistory(box, box.tag().privacy);
                                  }
                              };
            Action redo = () =>
                              {
                                  myTextBox = null;
                                  foreach (var box in selectedElements)
                                  {
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
            if (me == GlobalConstants.PROJECTOR) return;

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
                    var identity = Globals.generateId(rootPage.NetworkController.credentials.name, Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, currentPrivacy, identity, -1L, new StrokeCollection(selectedStrokes.Select(s => s as Stroke)), selectedTextBoxes.Select(s => s as Xceed.Wpf.Toolkit.RichTextBox), selectedImages.Select(s => s as Image));
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
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, "Delete selected items");
            redo();
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            Dispatcher.adopt(delegate
            {

                ClearAdorners();
            });
            if (ConversationDetails.Empty.Equals(details)) return;
            Dispatcher.adoptAsync(delegate
                  {
                      foreach (MeTLImage image in Work.ImageChildren())
                          image.ApplyPrivacyStyling(contentBuffer, _target, image.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                      foreach (var item in Work.TextChildren())
                      {
                          MeTLTextBox box = null;
                          box = (MeTLTextBox)item;
                          box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                      }

                      if (myTextBox != null)
                      {
                          myTextBox.Focus();
                          Keyboard.Focus(myTextBox);
                      }

                  });
        }
        private void setInkCanvasMode(string modeString)
        {
            if (me == GlobalConstants.PROJECTOR) return;
            Dispatcher.adopt(delegate
            {
                Work.EditingMode = (InkCanvasEditingMode)Enum.Parse(typeof(InkCanvasEditingMode), modeString);
                Work.UseCustomCursor = Work.EditingMode == InkCanvasEditingMode.Ink;
            });
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
                        add.ApplyPrivacyStyling(contentBuffer, _target, add.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                    }
                };
            Action redo = () =>
                {
                    foreach (var remove in redoToRemove)
                        contentBuffer.RemoveImage(remove, (img) => Work.Children.Remove(img));
                    foreach (var add in redoToAdd)
                    {
                        contentBuffer.AddImage(add, (img) => Work.Children.Add(img));
                        add.ApplyPrivacyStyling(contentBuffer, _target, add.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
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
                      box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
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
                      box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
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

                    var identity = Globals.generateId(rootPage.NetworkController.credentials.name, Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, Privacy.NotSet, identity, -1L,
                        new StrokeCollection(endingStrokes.Select(s => (s as Stroke).Clone())),
                        endingTexts.Select(et => et as Xceed.Wpf.Toolkit.RichTextBox),
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

                    var identity = Globals.generateId(rootPage.NetworkController.credentials.name, Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, Privacy.NotSet, identity, -1L,
                        new StrokeCollection(startingStrokes.Select(s => (s as Stroke).Clone())),
                        startingBoxes.Select(et => et as Xceed.Wpf.Toolkit.RichTextBox),
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
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, "Selection moved or resized");
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
            var me = rootPage.NetworkController.credentials.name;
            var myText = elements.Where(e => e is MeTLTextBox && (e as MeTLTextBox).tag().author != me);
            var myImages = elements.Where(e => e is Image && (e as Image).tag().author != me);
            var myElements = new List<T>();
            myElements.AddRange(myText);
            myElements.AddRange(myImages);
            return myElements;
        }
        private IEnumerable<T> filterExceptMine<T>(IEnumerable<T> elements, Func<T, string> authorExtractor) where T : UIElement
        {
            var me = rootPage.NetworkController.credentials.name;
            var myText = elements.Where(e => e is MeTLTextBox && authorExtractor(e).Trim().ToLower() == me);
            var myImages = elements.Where(e => e is Image && authorExtractor(e).Trim().ToLower() == me);
            var myElements = new List<T>();
            myElements.AddRange(myText);
            myElements.AddRange(myImages);
            return myElements;
        }

        private IEnumerable<PrivateAwareStroke> filterOnlyMineExceptIfHammering(IEnumerable<PrivateAwareStroke> strokes)
        {
            var me = rootPage.NetworkController.credentials.name;
            if (rootPage.UserSlideState.BanhammerActive)
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
            var myText = elements.OfType<MeTLTextBox>().Where(t => t.tag().author == rootPage.NetworkController.credentials.name);
            var myImages = elements.OfType<MeTLImage>().Where(i => i.tag().author == rootPage.NetworkController.credentials.name);
            var myElements = new List<T>();
            myElements.AddRange(myText.OfType<T>());
            myElements.AddRange(myImages.OfType<T>());
            return myElements;
        }
        private IEnumerable<T> filterOnlyMineExceptIfHammering<T>(IEnumerable<T> elements, Func<T, string> authorExtractor) where T : UIElement
        {
            if (rootPage.UserSlideState.BanhammerActive)
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
            if (rootPage.UserSlideState.BanhammerActive)
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
            return elements.Where(e => authorExtractor(e).Trim().ToLower() == rootPage.NetworkController.credentials.name);
        }
        private T filterOnlyMine<T>(UIElement element) where T : UIElement
        {
            UIElement filteredElement = null;

            if (element == null)
                return null;

            if (element is MeTLTextBox)
            {
                filteredElement = ((MeTLTextBox)element).tag().author == rootPage.NetworkController.credentials.name ? element : null;
            }
            else if (element is Image)
            {
                filteredElement = ((Image)element).tag().author == rootPage.NetworkController.credentials.name ? element : null;
            }
            return filteredElement as T;
        }

        private void selectionChanged(object sender, EventArgs e)
        {
            myTextBox = (MeTLTextBox)Work.GetSelectedTextBoxes().FirstOrDefault();
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
            Commands.AddPrivacyToggleButton.Execute(new PrivacyToggleButton.PrivacyToggleButtonInfo(rootPage, privacyChoice, allElementsCount != 0, Work.GetSelectionBounds(), _target));
        }
        private void ClearAdorners()
        {
            if (me != GlobalConstants.PROJECTOR)

                Commands.RemovePrivacyAdorners.ExecuteAsync(_target);
        }
        #endregion
        #region ink
        private void SetDrawingAttributes(DrawingAttributes logicalAttributes)
        {
            if (logicalAttributes == null) return;
            if (me.ToLower() == GlobalConstants.PROJECTOR) return;
            Dispatcher.adopt(delegate
            {

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
                setInkCanvasMode("Ink");
                Work.DefaultDrawingAttributes = zoomCompensatedAttributes;
            });
        }
        private void SetPenAttributes(PenAttributes logicalAttributes)
        {
            Commands.SetDrawingAttributes.Execute(logicalAttributes.attributes);
            Commands.SetLayer.Execute("Sketch");
            Commands.SetInkCanvasMode.Execute(logicalAttributes.mode.ToString());
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
            if (!(targettedDirtyStrokes.First().target.Equals(_target)) || targettedDirtyStrokes.First().slide != rootPage.Slide.id) return;
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

        public void SetContentVisibility(List<ContentVisibilityDefinition> contentVisibility)
        {
            if (_target == "notepad")
                return;

            Commands.UpdateContentVisibility.Execute(contentVisibility);
            Dispatcher.adopt(delegate
            {

                ClearAdorners();

                var selectedStrokes = Work.GetSelectedStrokes().Select(stroke => stroke.tag().id);
                var selectedImages = Work.GetSelectedImages().ToList().Select(image => image.tag().id);
                var selectedTextBoxes = Work.GetSelectedTextBoxes().ToList().Select(textBox => textBox.tag().id);

                ReAddFilteredContent(contentVisibility);

                if (me != GlobalConstants.PROJECTOR)
                    refreshWorkSelect(selectedStrokes, selectedImages, selectedTextBoxes);
            });
        }

        private void ReAddFilteredContent(List<ContentVisibilityDefinition> contentVisibility)
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
            Dispatcher.adopt(delegate
            {
                var strokeTarget = _target;
                foreach (var targettedStroke in receivedStrokes.Where(targettedStroke => targettedStroke.target == strokeTarget))
                {
                    if (targettedStroke.HasSameAuthor(me) || targettedStroke.HasSamePrivacy(Privacy.Public))
                        try
                        {
                            AddStrokeToCanvas(new PrivateAwareStroke(targettedStroke.stroke.Clone(), strokeTarget, rootPage.ConversationDetails));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception while receiving stroke: {0}", e.Message);
                        }
                }
            });
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
                    Commands.StrokePlaced.Execute(st);
                });
            }
            else
            {
                contentBuffer.AddStroke(stroke, (st) =>
                {
                    Work.Strokes.Add(st);
                    Commands.StrokePlaced.Execute(st);
                });
            }
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
            Dispatcher.adopt(delegate
            {

                if (CanvasHasActiveFocus())
                    deleteSelectedElements(null, null);
            });
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
                    //Commands.SendDirtyImage.ExecuteAsync(new TargettedDirtyElement (rootPage.slide.id, image.tag().author, _target, image.tag().privacy, image.tag().id));

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
                    tmpImage.ApplyPrivacyStyling(contentBuffer, _target, newPrivacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                }
            };
            Action undo = () =>
            {
                foreach (var image in selectedElements.Where(i => i.tag().privacy != newPrivacy))
                {
                    // dirty handled by move delta
                    //Commands.SendDirtyImage.ExecuteAsync(new TargettedDirtyElement (rootPage.slide.id, image.tag().author, _target, image.tag().privacy, image.tag().id));
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
                    tmpImage.ApplyPrivacyStyling(contentBuffer, _target, oldPrivacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
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
                    tmpText.ApplyPrivacyStyling(contentBuffer, _target, newPrivacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
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
                    tmpText.ApplyPrivacyStyling(contentBuffer, _target, oldPrivacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                }
            };
            return new UndoHistory.HistoricalAction(undo, redo, 0, "Text selection changed privacy");
        }
        private void changeSelectedItemsPrivacy(Privacy newPrivacy)
        {
            long timestamp = -1L;

            // can't change content privacy for notepad
            if (me == GlobalConstants.PROJECTOR || _target == "notepad") return;

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
                    var identity = Globals.generateId(rootPage.NetworkController.credentials.name, Guid.NewGuid().ToString());
                    //var moveDelta = TargettedMoveDelta.Create(rootPage.slide.id, rootPage.networkController.credentials.name, _target, currentPrivacy, timestamp, selectedStrokes, selectedTextBoxes, selectedImages);
                    var moveDelta = TargettedMoveDelta.Create(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, oldPrivacy, identity, timestamp, selectedStrokes.Select(s => s as Stroke), selectedTextBoxes.Select(s => s as Xceed.Wpf.Toolkit.RichTextBox), selectedImages.Select(s => s as Image));
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
                    var identity = Globals.generateId(rootPage.NetworkController.credentials.name, Guid.NewGuid().ToString());
                    var moveDelta = TargettedMoveDelta.Create(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, oldPrivacy, identity, timestamp, selectedStrokes.Select(s => s as Stroke), selectedTextBoxes.Select(s => s as Xceed.Wpf.Toolkit.RichTextBox), selectedImages.Select(s => s as Image));
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
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, "Selected items changed privacy");
        }

        public void RefreshCanvas()
        {
            Work.Children.Clear();
            Work.Strokes.Clear();

            contentBuffer.AdjustContent();
            ReAddFilteredContent(rootPage.UserConversationState.ContentVisibility);
        }
        private void singleStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var inner = e.Stroke.GetBounds();
            var outer = new Rect(Work.RenderSize);
            if (!outer.Contains(inner))
            {
                var d = 1;
            }
            var checksum = e.Stroke.sum().checksum;
            var timestamp = 0L;
            e.Stroke.tag(new StrokeTag(rootPage.NetworkController.credentials.name, currentPrivacy, checksum.ToString(), checksum, e.Stroke.DrawingAttributes.IsHighlighter, timestamp));
            var privateAwareStroke = new PrivateAwareStroke(e.Stroke, _target, rootPage.ConversationDetails);
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
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, String.Format("Added stroke [{0}]", thisStroke.tag().id));
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
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, String.Format("Deleted stroke [{0}]", stroke.tag().id));
        }

        private void doMyStrokeRemovedExceptHistory(Stroke stroke)
        {
            var strokeTag = stroke.tag();
            Commands.SendDirtyStroke.Execute(new TargettedDirtyElement(rootPage.Slide.id, strokeTag.author, _target, strokeTag.privacy, strokeTag.id, strokeTag.timestamp));
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
            var privateRoom = string.Format("{0}{1}", rootPage.Slide.id, translatedStroke.tag().author);
            if (thisPrivacy == Privacy.Private && rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name) && me != translatedStroke.tag().author)
                rootPage.NetworkController.client.JoinRoom(privateRoom);
            Commands.SendStroke.Execute(new TargettedStroke(rootPage.Slide.id, translatedStroke.tag().author, _target, translatedStroke.tag().privacy, translatedStroke.tag().id, translatedStroke.tag().timestamp, translatedStroke, translatedStroke.tag().startingSum));
            if (thisPrivacy == Privacy.Private && rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name) && me != stroke.tag().author)
                rootPage.NetworkController.client.LeaveRoom(privateRoom);
        }
        #endregion
        #region Images

        private void sendImage(MeTLImage newImage)
        {
            newImage.UpdateLayout();
            Commands.SendImage.Execute(new TargettedImage(rootPage.Slide.id, me, _target, newImage.tag().privacy, newImage.tag().id, newImage, newImage.tag().resourceIdentity, newImage.tag().timestamp));
        }
        private void dirtyImage(MeTLImage imageToDirty)
        {
            imageToDirty.ApplyPrivacyStyling(contentBuffer, _target, imageToDirty.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
            Commands.SendDirtyImage.Execute(new TargettedDirtyElement(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, imageToDirty.tag().privacy, imageToDirty.tag().id, imageToDirty.tag().timestamp));
        }
        public void ReceiveMoveDelta(TargettedMoveDelta moveDelta, bool processHistory = false)
        {
            if (_target == "presentationSpace")
            {
                Dispatcher.adopt(delegate
                {
                    moveDeltaProcessor.ReceiveMoveDelta(moveDelta, me, processHistory);
                });
            }
        }

        public void ReceiveImages(IEnumerable<TargettedImage> images)
        {
            foreach (var image in images)
            {
                if (image.HasSameTarget(_target))
                {
                    TargettedImage image1 = image;
                    if (image.HasSameAuthor(me) || image.HasSamePrivacy(Privacy.Public))
                    {
                        Dispatcher.adopt(() =>
                        {
                            try
                            {
                                var receivedImage = image1.imageSpecification.forceEvaluation();
                                AddImage(Work, receivedImage);
                                receivedImage.ApplyPrivacyStyling(contentBuffer, _target, receivedImage.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error in receiving image: {0}", e.Message);
                            }
                        });
                    }
                }
            }
        }
        private void AddImage(InkCanvas canvas, MeTLImage image)
        {
            if (canvas.ImageChildren().Any(i => ((MeTLImage)i).tag().id == image.tag().id)) return;
            contentBuffer.AddImage(image, (img) =>
            {
                var imageToAdd = img as MeTLImage;
                var zIndex = imageToAdd.tag().isBackground ? -5 : 2;

                Canvas.SetZIndex(img, zIndex);
                canvas.Children.Add(img);
            });
        }
        private MeTLImage OffsetNegativeCartesianImageTranslate(MeTLImage image)
        {
            var newImage = image.Clone();
            InkCanvas.SetLeft(newImage, (InkCanvas.GetLeft(newImage) + newImage.offsetX));//contentBuffer.logicalX));
            InkCanvas.SetTop(newImage, (InkCanvas.GetTop(newImage) + newImage.offsetY));//contentBuffer.logicalY));
            newImage.offsetX = 0;
            newImage.offsetY = 0;
            return newImage;
        }
        private Point OffsetNegativeCartesianPointTranslate(Point point)
        {
            return new Point(point.X + contentBuffer.logicalX, point.Y + contentBuffer.logicalY);
        }

        public void ReceiveDirtyImage(TargettedDirtyElement element)
        {
            if (!(element.target.Equals(_target))) return;
            if (element.slide != rootPage.Slide.id) return;
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
            Dispatcher.adopt(delegate
            {
                addResourceFromDisk((files) =>
                {
                    var origin = new Point(0, 0);
                    int i = 0;
                    foreach (var file in files)
                        handleDrop(file, origin, true, i++, (source, offset, count) => { return offset; });
                });
            });
        }

        private void imagesDropped(List<ImageDrop> images)
        {
            foreach (var image in images)
                imageDropped(image);
        }
        private void imageDropped(ImageDrop drop)
        {
            if (drop.Target.Equals(_target) && me != GlobalConstants.PROJECTOR)
            {
                Dispatcher.adopt(delegate
                {

                    handleDrop(drop.Filename, new Point(0, 0), drop.OverridePoint, drop.Position, (source, offset, count) => { return drop.Point; });
                });
            }
        }
        private void addResourceFromDisk(Action<IEnumerable<string>> withResources)
        {
            const string filter = "Image files(*.jpeg;*.gif;*.bmp;*.jpg;*.png)|*.jpeg;*.gif;*.bmp;*.jpg;*.png|All files (*.*)|*.*";
            addResourceFromDisk(filter, withResources);
        }

        private void addResourceFromDisk(string filter, Action<IEnumerable<string>> withResources)
        {
            if (_target == "presentationSpace" && me != GlobalConstants.PROJECTOR)
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
            if (me == GlobalConstants.PROJECTOR) return;
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
                     rootPage.NetworkController.client.UploadAndSendFile(
                         new MeTLStanzas.LocalFileInformation(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, Privacy.Public, -1L, filename, Path.GetFileNameWithoutExtension(filename), false, new FileInfo(filename).Length, DateTimeFactory.Now().Ticks.ToString(), Globals.generateId(rootPage.NetworkController.credentials.name, filename)));
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
            if (origin.X < 50)
                origin.X = 50;
            if (origin.Y < 50)
                origin.Y = 50;
            var newPoint = OffsetNegativeCartesianPointTranslate(origin);
            // should calculate the original image dimensions before sending it away.
            var width = 320;
            var height = 240;
            rootPage.NetworkController.client.UploadAndSendImage(new MeTLStanzas.LocalImageInformation(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, rootPage.UserConversationState.Privacy, newPoint.X, newPoint.Y, width, height, fileName));
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
            if (me == GlobalConstants.PROJECTOR) return;
            /*
            var pos = e.GetPosition(this);
            var res = html.ExecuteJavascriptWithResult(string.Format("MeTLText.append({0},{1})", pos.X, pos.Y));
            
            var pos = e.GetPosition(this);                                               
            MeTLTextBox box = createNewTextbox();
            AddTextBoxToCanvas(box, true);
            InkCanvas.SetLeft(box, pos.X);
            InkCanvas.SetTop(box, pos.Y);
            myTextBox = box;
            */
            var box = createNewTextbox();
            AddTextBoxToCanvas(box, true);
            RichTextBoxFormatBarManager.SetFormatBar(box, new RichTextBoxFormatBar());
            box.Width = 250;
            var pos = e.GetPosition(this);
            InkCanvas.SetLeft(box, pos.X);
            InkCanvas.SetTop(box, pos.Y);
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
                try
                {
                    if (targettedBox.target != _target) return;
                    if (targettedBox.HasSamePrivacy(Privacy.Public) || targettedBox.HasSameAuthor(me))
                    {
                        var box = UpdateTextBoxWithId(targettedBox);
                        if (box != null)
                        {
                            if (!(targettedBox.HasSameAuthor(me)))
                                box.Focusable = false;
                            box.ApplyPrivacyStyling(contentBuffer, _target, targettedBox.privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while receiving text: {0}", e.Message);
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
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
            box.tag(oldBox.tag());
            box.FontFamily = oldBox.FontFamily;
            box.FontStyle = oldBox.FontStyle;
            box.FontWeight = oldBox.FontWeight;
            box.FontSize = oldBox.FontSize;
            box.Foreground = oldBox.Foreground;
            box.TextChanged -= SendNewText;
            box.Text = oldBox.Text;
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
            box.PreviewMouseRightButtonUp -= box_PreviewMouseRightButtonUp;

            box.TextChanged -= SendNewText;
            box.AcceptsReturn = true;
            box.GotFocus += textboxGotFocus;
            box.LostFocus += textboxLostFocus;
            box.TextChanged += SendNewText;
            box.IsUndoEnabled = false;
            box.UndoLimit = 0;
            box.BorderThickness = new Thickness(0);
            box.BorderBrush = new SolidColorBrush(Colors.Transparent);
            box.Background = new SolidColorBrush(Colors.Transparent);
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
            box.TextChanged += SendNewText;

            return box;
        }
        private void SendNewText(object sender, TextChangedEventArgs e)
        {
            if (me == GlobalConstants.PROJECTOR) return;
            var box = (MeTLTextBox)sender;
            var undoText = box.Text;
            var redoText = box.Text;
            box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
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
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, String.Format("Added text [{0}]", redoText));

            // only do the change locally and let the timer do the rest
            ClearAdorners();
            UpdateTextBoxWithId(mybox, redoText);

            var currentSlide = rootPage.Slide.id;
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

        private void sendBox(MeTLTextBox box, bool localOnly = false)
        {
            myTextBox = box;
            if (!Work.Children.ToList().Any(c => c is MeTLTextBox && ((MeTLTextBox)c).tag().id == box.tag().id))
                AddTextBoxToCanvas(box, true);
            if (!localOnly)
            {
                sendTextWithoutHistory(box, box.tag().privacy);
            }
        }

        private MeTLTextBox OffsetNegativeCartesianTextTranslate(MeTLTextBox box)
        {
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
            sendTextWithoutHistory(box, thisPrivacy, rootPage.Slide.id);
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
            var privateRoom = string.Format("{0}{1}", rootPage.Slide.id, translatedTextBox.tag().author);
            /*
            if (thisPrivacy == Privacy.Private && rootPage.details.isAuthor(rootPage.networkController.credentials.name) && me != translatedTextBox.tag().author)
                Commands.SneakInto.Execute(privateRoom);
             */
            Commands.SendTextBox.ExecuteAsync(new TargettedTextBox(slide, translatedTextBox.tag().author, _target, thisPrivacy, translatedTextBox.tag().id, translatedTextBox, translatedTextBox.tag().timestamp));
            /*
            if (thisPrivacy == Privacy.Private && rootPage.details.isAuthor(rootPage.networkController.credentials.name) && me != translatedTextBox.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);
             */
            //NegativeCartesianTextTranslate(translatedTextBox);
            /*var privateRoom = string.Format("{0}{1}", rootPage.slide.id, box.tag().author);
            if (thisPrivacy == Privacy.Private && rootPage.details.isAuthor(rootPage.networkController.credentials.name) && me != box.tag().author)
                Commands.SneakInto.Execute(privateRoom);
            Commands.SendTextBox.ExecuteAsync(new TargettedTextBox(slide, box.tag().author, _target, thisPrivacy, box.tag().id, box, box.tag().timestamp));
            if (thisPrivacy == Privacy.Private && rootPage.details.isAuthor(rootPage.networkController.credentials.name) && me != box.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);*/
        }

        private void dirtyTextBoxWithoutHistory(MeTLTextBox box, bool removeLocal = true)
        {
            dirtyTextBoxWithoutHistory(box, rootPage.Slide.id, removeLocal);
        }

        private void dirtyTextBoxWithoutHistory(MeTLTextBox box, int slide, bool removeLocal = true)
        {
            box.RemovePrivacyStyling(contentBuffer);
            if (removeLocal)
                RemoveTextBoxWithMatchingId(box.tag().id);
            Commands.SendDirtyText.ExecuteAsync(new TargettedDirtyElement(slide, box.tag().author, _target, canvasAlignedPrivacy(box.tag().privacy), box.tag().id, box.tag().timestamp));
        }
        /*
        public void sendImageWithoutHistory(MeTLImage image, Privacy thisPrivacy)
        {
            sendImageWithoutHistory(image, thisPrivacy, rootPage.slide.id);
        }

        public void sendImageWithoutHistory(MeTLImage image, Privacy thisPrivacy, int slide)
        {
            var privateRoom = string.Format("{0}{1}", rootPage.slide.id, image.tag().author);
            if (thisPrivacy == Privacy.Private && rootPage.details.isAuthor(rootPage.networkController.credentials.name) && me != image.tag().author)
                Commands.SneakInto.Execute(privateRoom);
            Commands.SendImage.ExecuteAsync(new TargettedImage(rootPage.slide.id, image.tag().author, _target, thisPrivacy, image.tag().id, image, image.tag().timestamp));
            if (thisPrivacy == Privacy.Private && rootPage.details.isAuthor(rootPage.networkController.credentials.name) && me != image.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);
        }
        */
        private void textboxLostFocus(object sender, RoutedEventArgs e)
        {
            var box = (MeTLTextBox)sender;
            ClearAdorners();
            myTextBox = null;
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
            box.ApplyPrivacyStyling(contentBuffer, _target, box.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
        }

        private void textboxGotFocus(object sender, RoutedEventArgs e)
        {
            if (((MeTLTextBox)sender).tag().author != me) return; //cannot edit other peoples textboxes
            if (me != GlobalConstants.PROJECTOR)
            {
                myTextBox = (MeTLTextBox)sender;
            }
            CommandManager.InvalidateRequerySuggested();
            if (myTextBox == null)
                return;
            Commands.ChangeTextMode.ExecuteAsync("None");
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
            if (element.slide != rootPage.Slide.id) return;
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
        }

        public void ReceiveTextBox(TargettedTextBox targettedBox)
        {
            if (targettedBox.target != _target) return;

            if (me != GlobalConstants.PROJECTOR && TargettedTextBoxIsFocused(targettedBox))
                return;
            Dispatcher.adopt(delegate
            {
                if (targettedBox.slide == rootPage.Slide.id && ((targettedBox.HasSamePrivacy(Privacy.Private) && !targettedBox.HasSameAuthor(me)) || me == GlobalConstants.PROJECTOR))
                    RemoveTextBoxWithMatchingId(targettedBox.identity);
                if (targettedBox.slide == rootPage.Slide.id && ((targettedBox.HasSamePrivacy(Privacy.Public) || (targettedBox.HasSameAuthor(me)) && me != GlobalConstants.PROJECTOR)))
                    DoText(targettedBox);
            });
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
                author = rootPage.NetworkController.credentials.name,
                privacy = currentPrivacy,
                id = string.Format("{0}:{1}", rootPage.NetworkController.credentials.name, DateTimeFactory.Now().Ticks)
            });
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
                    var identity = Globals.generateId(rootPage.NetworkController.credentials.name, Guid.NewGuid().ToString());
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
                        rootPage.NetworkController.client.NoAuthUploadResource(
                            new Uri(tmpFile, UriKind.RelativeOrAbsolute), rootPage.Slide.id);
                    var image = new MeTLImage
                    {
                        Source = new BitmapImage(rootPage.NetworkController.config.getImage(currentPrivacy == Privacy.Public ? rootPage.Slide.id.ToString() : rootPage.Slide.id.ToString() + rootPage.NetworkController.credentials.name, uri)),
                        Width = imageSource.Width,
                        Height = imageSource.Height,
                        Stretch = Stretch.Fill
                    };
                    image.tag(new ImageTag(rootPage.NetworkController.credentials.name, currentPrivacy, Globals.generateId(rootPage.NetworkController.credentials.name), false, -1L, uri.ToString(), -1)); // ZIndex was -1, timestamp is -1L
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
                Commands.SendImage.ExecuteAsync(new TargettedImage(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, currentPrivacy, image.tag().id, image, image.tag().resourceIdentity, image.tag().timestamp));
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
            if (me == GlobalConstants.PROJECTOR) return;
            var currentBox = myTextBox.clone();

            if (Clipboard.ContainsData(MeTLClipboardData.Type))
            {
                var data = (MeTLClipboardData)Clipboard.GetData(MeTLClipboardData.Type);
                var currentChecksums = Work.Strokes.Select(s => s.sum().checksum);
                var ink = data.Ink.Where(s => !currentChecksums.Contains(s.sum().checksum)).Select(s => new PrivateAwareStroke(s, _target, rootPage.ConversationDetails)).ToList();
                var boxes = createPastedBoxes(data.Text.ToList());
                var images = createImages(data.Images.ToList());
                Action undo = () =>
                                  {
                                      ClearAdorners();
                                      HandleInkPasteUndo(ink.Select(s => s as Stroke).ToList());
                                      HandleImagePasteUndo(images);
                                      AddAdorners();
                                  };
                Action redo = () =>
                                  {
                                      ClearAdorners();
                                      HandleInkPasteRedo(ink);
                                      HandleImagePasteRedo(images);
                                      AddAdorners();
                                  };
                rootPage.UserConversationState.UndoHistory.Queue(undo, redo, "Pasted items");
                redo();
            }
            else
            {
                if (Clipboard.ContainsImage())
                {
                    Action undo = () => HandleImagePasteUndo(createImages(new List<BitmapSource> { Clipboard.GetImage() }));
                    Action redo = () => HandleImagePasteRedo(createImages(new List<BitmapSource> { Clipboard.GetImage() }));
                    rootPage.UserConversationState.UndoHistory.Queue(undo, redo, "Pasted images");
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
            if (me == GlobalConstants.PROJECTOR) return;
            //text 
            var selectedElements = filterOnlyMineExceptIfHammering(Work.GetSelectedElements()).ToList();
            var selectedStrokes = filterOnlyMineExceptIfHammering(Work.GetSelectedStrokes().Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke)).Select((s => s.Clone())).ToList();
            string selectedText = null;
            if (myTextBox != null)
                selectedText = myTextBox.Selection.Text;

            // copy previously was an undoable action, ie restore the clipboard to what it previously was
            var images = HandleImageCopyRedo(selectedElements);
            var text = new List<string>();
            if (selectedText != null)
            {
                text = HandleTextCopyRedo(selectedElements, selectedText);
            }
            var copiedStrokes = HandleStrokeCopyRedo(selectedStrokes.Select(s => s as Stroke).ToList());
            Clipboard.SetData(MeTLClipboardData.Type, new MeTLClipboardData(rootPage, text, images, copiedStrokes));
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
                img.ApplyPrivacyStyling(contentBuffer, _target, img.tag().privacy, rootPage.ConversationDetails, rootPage.NetworkController.credentials.name);
                Work.Children.Remove(img);
                Commands.SendDirtyImage.Execute(new TargettedDirtyElement(rootPage.Slide.id, rootPage.NetworkController.credentials.name, _target, canvasAlignedPrivacy(img.tag().privacy), img.tag().id, img.tag().timestamp));
            }
            return selectedImages.Select(i => (BitmapSource)i.Source);
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
            var listToCut = selectedStrokes.Select(stroke => new TargettedDirtyElement(rootPage.Slide.id, stroke.tag().author, _target, canvasAlignedPrivacy(stroke.tag().privacy), stroke.tag().id /* stroke.sum().checksum.ToString()*/, stroke.tag().timestamp)).ToList();
            foreach (var element in listToCut)
                Commands.SendDirtyStroke.Execute(element);
            return selectedStrokes.ToList();
        }
        protected void HandleCut(object _args)
        {
            if (me == GlobalConstants.PROJECTOR) return;
            var strokesToCut = filterOnlyMineExceptIfHammering(Work.GetSelectedStrokes().OfType<PrivateAwareStroke>()).Select(s => s.Clone());
            var currentTextBox = myTextBox.clone();
            var selectedImages = filterOnlyMineExceptIfHammering(Work.GetSelectedImages()).Select(i => i.Clone());
            var selectedText = filterOnlyMineExceptIfHammering(Work.GetSelectedTextBoxes()).Select(t => t.clone());

            Action redo = () =>
            {
                ClearAdorners();
                var images = HandleImageCutRedo(selectedImages);
                var ink = HandleInkCutRedo(strokesToCut);
                Clipboard.SetData(MeTLClipboardData.Type, new MeTLClipboardData(rootPage, new List<String>(), images, ink.Select(s => s as Stroke).ToList()));
            };
            Action undo = () =>
            {
                ClearAdorners();
                if (Clipboard.ContainsData(MeTLClipboardData.Type))
                {
                    Clipboard.GetData(MeTLClipboardData.Type);
                    HandleImageCutUndo(selectedImages);
                    HandleInkCutUndo(strokesToCut);
                }
            };
            redo();
            rootPage.UserConversationState.UndoHistory.Queue(undo, redo, "Cut items");
        }
        #endregion

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
    }
}
