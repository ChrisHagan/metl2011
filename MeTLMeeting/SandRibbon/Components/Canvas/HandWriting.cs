using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbonInterop;
using SandRibbonObjects;
using System.Windows;
using MeTLLib.DataTypes;

namespace SandRibbon.Components.Canvas
{
    public class HandWriting : AbstractCanvas
    {
        public HandWriting()
        {
            Loaded += HandWriting_Loaded;
            StrokeCollected += singleStrokeCollected;
            SelectionChanging += selectingStrokes;
            SelectionChanged += selectionChanged;
            StrokeErasing += erasingStrokes;
            SelectionMoving += selectionMoving;
            SelectionMoved += selectionMoved;
            SelectionResizing += selectionMoving;
            SelectionResized += selectionMoved;
            DefaultDrawingAttributesReplaced += announceDrawingAttributesChanged;
            Background = Brushes.Transparent;
            defaultWidth = DefaultDrawingAttributes.Width;
            defaultHeight = DefaultDrawingAttributes.Height;
            modeChangedCommand = new DelegateCommand<string>(setInkCanvasMode, canChangeMode);
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, deleteSelectedStrokes));
            Commands.SetInkCanvasMode.RegisterCommandToDispatcher<string>(modeChangedCommand);
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
                newAttributes.Width = defaultWidth;
                newAttributes.Height = defaultHeight;
                DefaultDrawingAttributes = newAttributes;
            }));
            Commands.ActualSetDrawingAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>(attributes => Dispatcher.adoptAsync(()=>
                                                                                                                                           DefaultDrawingAttributes = attributes)));
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
            colorChangedCommand = new DelegateCommand<object>((colorObj) =>
            {
                var newAttributes = DefaultDrawingAttributes.Clone();
                if (colorObj is Color)
                    newAttributes.Color = (Color)colorObj;
                else if (colorObj is string)
                    newAttributes.Color = ColorLookup.ColorOf((string)colorObj);
                newAttributes.FitToCurve = true;
                newAttributes.IgnorePressure = false;
                DefaultDrawingAttributes = newAttributes;
            });
            Commands.UpdateCursor.RegisterCommand(new DelegateCommand<Cursor>(UpdateCursor));
            Commands.SetPenColor.RegisterCommand(colorChangedCommand);
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedStroke>((stroke) => ReceiveStrokes(new[] { stroke })));
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedStroke>>(ReceiveStrokes));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<string>(changeSelectedItemsPrivacy));
            Commands.ReceiveDirtyStrokes.RegisterCommand(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedDirtyElement>>(ReceiveDirtyStrokes));
            Commands.DeleteSelectedItems.RegisterCommand(new DelegateCommand<object>(deleteSelectedItems));
            Commands.HideConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(hideConversationSearchBox));
        }

        private void hideConversationSearchBox(object obj)
        {
            addAdorners();
        }

        private void UpdateCursor(object obj)
        {
            if (obj is Cursor)
            {
                Dispatcher.adoptAsync(() =>
                {
                    UseCustomCursor = true;
                    Cursor = (Cursor)obj;
                });
            }
        }
        private void deleteSelectedItems(object obj)
        {
            Dispatcher.adopt(delegate
            {
                deleteSelectedStrokes(null, null);
                ClearAdorners();
            });
        }
        private void HandWriting_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DefaultPenAttributes();
        }
        public static Guid STROKE_PROPERTY = Guid.NewGuid();
        public List<MeTLLib.DataTypes.StrokeChecksum> strokes = new List<MeTLLib.DataTypes.StrokeChecksum>();
        private DelegateCommand<string> modeChangedCommand;
        private DelegateCommand<object> colorChangedCommand;
        protected override void CanEditChanged()
        {
            canEdit = base.canEdit;
            if (privacy == "private") canEdit = true;
        }
        private bool canEdit
        {
            get { return base.canEdit; }
            set
            {
                base.canEdit = value;
                SetEditingMode();
                modeChangedCommand.RaiseCanExecuteChanged();
            }
        }
        private double defaultWidth;
        private double defaultHeight;
        private bool canChangeMode(string arg)
        {
            return true;
        }
        private void setInkCanvasMode(string modeString)
        {
            if (!canEdit)
                EditingMode = InkCanvasEditingMode.None;
            else
            {
                EditingMode = (InkCanvasEditingMode)Enum.Parse(typeof(InkCanvasEditingMode), modeString);
                if (EditingMode == InkCanvasEditingMode.Ink)
                {
                    UseCustomCursor = true;
                }
                else 
                {
                    UseCustomCursor = false;
                }
            }
        }
        public void SetEditingMode()
        {
            if (canEdit)
                Enable();
            else
                Disable();
        }
        public void DefaultPenAttributes()
        {
            DefaultDrawingAttributes = new DrawingAttributes
                                           {
                                               Color = Colors.Black,
                                               Width = 1,
                                               Height = 1,
                                               IsHighlighter = false
                                           };
        }
        private void announceDrawingAttributesChanged(object sender, DrawingAttributesReplacedEventArgs e)
        {
            Commands.ActualReportDrawingAttributes.ExecuteAsync(this.DefaultDrawingAttributes);
        }
        private static List<TimeSpan> strokeReceiptDurations = new List<TimeSpan>();
        private static double averageStrokeReceiptDuration()
        {
            return strokeReceiptDurations.Aggregate(0.0, (acc, item) => acc + item.TotalMilliseconds) / strokeReceiptDurations.Count();
        }
        public void ReceiveStrokes(IEnumerable<TargettedStroke> receivedStrokes)
        {
            if (receivedStrokes.Count() == 0) return;
            if (receivedStrokes.First().slide != currentSlide) return;
            var strokeTarget = target;
            Dispatcher.adopt(
                delegate
                {
                    var start = DateTimeFactory.Now();
                    var newStrokes = new StrokeCollection(
                        receivedStrokes.Where(ts => ts.target == strokeTarget)
                        .Where(s => s.privacy == "public" || (s.author == Globals.me && me != "projector"))
                        .Select(s => (Stroke)new PrivateAwareStroke(s.stroke))
                        .Where(s => !(this.strokes.Contains(s.sum()))));
                    Strokes.Add(newStrokes);
                    this.strokes.AddRange(newStrokes.Select(s => s.sum()));
                    var duration = DateTimeFactory.Now() - start;
                    strokeReceiptDurations.Add(duration);
                });
        }
        public void ReceiveDirtyStrokes(IEnumerable<MeTLLib.DataTypes.TargettedDirtyElement> targettedDirtyStrokes)
        {
            if (targettedDirtyStrokes.Count() == 0) return;
            if (!(targettedDirtyStrokes.First().target.Equals(target)) || targettedDirtyStrokes.First().slide != currentSlide) return;
            Dispatcher.adopt(delegate
            {
                var dirtyChecksums = targettedDirtyStrokes.Select(t => t.identifier);
                var presentDirtyStrokes = Strokes.Where(s => dirtyChecksums.Contains(s.sum().checksum.ToString())).ToList();
                for (int i = 0; i < presentDirtyStrokes.Count(); i++)
                {
                    var stroke = presentDirtyStrokes[i];
                    strokes.Remove(stroke.sum());
                    Strokes.Remove(stroke);

                }
            });
        }
        #region eventHandlers
        private void selectionChanged(object sender, EventArgs e)
        {
            Dispatcher.adoptAsync((Action)addAdorners);
        }

        protected internal void addAdorners()
        {
            var selectedStrokes = GetSelectedStrokes();
            if (selectedStrokes.Count == 0) return;
            var publicStrokes = selectedStrokes.Where(s => s.tag().privacy.ToLower() == "public").ToList();
            string privacyChoice;
            if (publicStrokes.Count == 0)
                privacyChoice = "show";
            else if (publicStrokes.Count == selectedStrokes.Count)
                privacyChoice = "hide";
            else
                privacyChoice = "both";
            Commands.AddPrivacyToggleButton.Execute(new PrivacyToggleButton.PrivacyToggleButtonInfo(privacyChoice, GetSelectionBounds()));
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
            e.Stroke.tag(new StrokeTag { author = Globals.me, privacy = Globals.privacy, isHighlighter = e.Stroke.DrawingAttributes.IsHighlighter });
            var privateAwareStroke = new PrivateAwareStroke(e.Stroke);
            Strokes.Remove(e.Stroke);
            privateAwareStroke.startingSum(privateAwareStroke.sum().checksum);
            Strokes.Add(privateAwareStroke);
            doMyStrokeAdded(privateAwareStroke);
            Commands.RequerySuggested(Commands.Undo);
        }
        private void erasingStrokes(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            try
            {
                if (!(filter(Strokes, Globals.me).Contains(e.Stroke)))
                {
                    e.Cancel = true;
                    return;
                }
                doMyStrokeRemoved(e.Stroke);
            }
            catch (Exception ex)
            {
                //Tag can be malformed if app state isn't fully logged in
            }
        }
        public void doMyStrokeAdded(Stroke stroke)
        {
            doMyStrokeAdded(stroke, privacy);
        }
        public void doMyStrokeAdded(Stroke stroke, string intendedPrivacy)
        {
            doMyStrokeAddedExceptHistory(stroke, intendedPrivacy);
            var thisStroke = stroke.Clone();
            UndoHistory.Queue(
                () =>
                {
                    var existingStroke = Strokes.Where(s => s.sum().checksum == thisStroke.sum().checksum).FirstOrDefault();
                    if (existingStroke != null)
                    {
                        Strokes.Remove(existingStroke);
                        doMyStrokeRemovedExceptHistory(existingStroke);
                    }
                    addAdorners();
                },
                () =>
                {
                    if (Strokes.Where(s => s.sum().checksum == thisStroke.sum().checksum).Count() == 0)
                    {
                        Strokes.Add(thisStroke);
                        doMyStrokeAddedExceptHistory(thisStroke, thisStroke.tag().privacy);
                    }
                });
        }
        private void doMyStrokeAddedExceptHistory(Stroke stroke, string thisPrivacy)
        {
            if (!strokes.Contains(stroke.sum()))
                strokes.Add(stroke.sum());
            stroke.tag(new StrokeTag { author = Globals.me, privacy = thisPrivacy, isHighlighter = stroke.DrawingAttributes.IsHighlighter });
            SendTargettedStroke(stroke, thisPrivacy);
        }
        public void SendTargettedStroke(Stroke stroke, string thisPrivacy)
        {
            if (!stroke.shouldPersist()) return;
            Commands.ActualReportStrokeAttributes.ExecuteAsync(stroke.DrawingAttributes);
            Commands.SendStroke.Execute(new TargettedStroke(currentSlide,Globals.me,target,stroke.tag().privacy,stroke, stroke.tag().startingSum));
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
                        Strokes.Remove(Strokes.Where(s=> s.sum().checksum == stroke.sum().checksum).First());
                        strokes.Remove(stroke.sum());
                        doMyStrokeRemovedExceptHistory(stroke);
                    }
                });
        }
        private void doMyStrokeRemovedExceptHistory(Stroke stroke)
        {
            var sum = stroke.sum().checksum.ToString();
            Commands.SendDirtyStroke.Execute(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide,Globals.me,target,stroke.tag().privacy,sum));
        }
        List<Stroke> strokesAtTheStart = new List<Stroke>();
        private void selectionMoving (object _sender, InkCanvasSelectionEditingEventArgs _e)
        {
            strokesAtTheStart.Clear();
            foreach (var stroke in GetSelectedStrokes())
            {
                strokesAtTheStart.Add(stroke.Clone());
            }
        }
        private void selectionMoved(object sender, EventArgs e)
        {
            var selectedStrokes = GetSelectedStrokes().Select(stroke => stroke.Clone()).ToList();
            var undoStrokes = strokesAtTheStart.Select(stroke => stroke.Clone()).ToList();
            Action redo = () =>
                {
                    var newSelection = new StrokeCollection();
                    Select(newSelection);
                    foreach (var stroke in undoStrokes)
                    {
                        if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                        {
                            Strokes.Remove(Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First());
                            strokes.Remove(stroke.sum());
                        }
                        doMyStrokeRemovedExceptHistory(stroke);
                    }
                    foreach (var stroke in selectedStrokes)
                    {
                        newSelection.Add(stroke);
                        if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                        {
                            Strokes.Add(stroke);
                        }
                        doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                    }
                    Select(newSelection);
                    addAdorners();
                };
            Action undo = () =>
                {
                   
                    var newSelection = new StrokeCollection();
                    foreach (var stroke in selectedStrokes)
                    {
                        if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                            Strokes.Remove(Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First());
                        doMyStrokeRemovedExceptHistory(stroke);
                    }
                    foreach (var stroke in undoStrokes)
                    {
                        newSelection.Add(stroke);
                        if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                            Strokes.Add(stroke);
                        doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                    }
                    Select(newSelection);
                    addAdorners();
                };
            redo(); 
            addAdorners();
            UndoHistory.Queue(undo, redo);
        
        }
        private void dirtySelectedRegions(object _sender, InkCanvasSelectionEditingEventArgs _e)
        {
            var selectedStrokes = GetSelectedStrokes();
            Action redo = () =>
                {
                    foreach (var stroke in selectedStrokes)
                    {
                        if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                        {
                            Strokes.Remove(Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First());
                            doMyStrokeRemovedExceptHistory(stroke);
                        }
                    }
                };
            Action undo = () =>
                {
                    var newSelection = new StrokeCollection();
                    foreach (var stroke in selectedStrokes)
                    {
                        newSelection.Add(stroke);
                        if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                        {
                            Strokes.Add(stroke);
                            doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                        }
                    }
                    Select(newSelection);
                    addAdorners();
                };
            redo();
            UndoHistory.Queue(undo, redo);
        }
        private void changeSelectedItemsPrivacy(string newPrivacy)
        {
            if (me == "projector") return;
            List<Stroke> selectedStrokes = new List<Stroke>();
            Dispatcher.adopt(() => selectedStrokes = GetSelectedStrokes().ToList());
            Action redo = () =>
            {
                foreach (Stroke stroke in selectedStrokes.Where(i => i is Stroke && i.tag().privacy != newPrivacy))
                {
                    var oldTag = ((Stroke)stroke).tag();

                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                    {
                        Strokes.Remove(Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First());
                        doMyStrokeRemovedExceptHistory(stroke);
                    }
                    var newStroke = stroke.Clone();
                    ((Stroke)newStroke).tag(new MeTLLib.DataTypes.StrokeTag { author = oldTag.author, privacy = newPrivacy, startingSum = oldTag.startingSum });
                    if (Strokes.Where(s => s.sum().checksum == newStroke.sum().checksum).Count() == 0)
                    {
                        Strokes.Add(newStroke);
                        doMyStrokeAddedExceptHistory(newStroke, newPrivacy);
                    }
                }
                Dispatcher.adopt(() => Select(new StrokeCollection()));
            };
            Action undo = () =>
            {
                foreach (Stroke stroke in selectedStrokes.Where(i => i is Stroke && i.tag().privacy != newPrivacy))
                {
                    var oldTag = ((Stroke)stroke).tag();
                    var newStroke = stroke.Clone();
                    ((Stroke)newStroke).tag(new MeTLLib.DataTypes.StrokeTag { author = oldTag.author, privacy = newPrivacy, startingSum = oldTag.startingSum });

                    if (Strokes.Where(s => s.sum().checksum == newStroke.sum().checksum).Count() > 0)
                    {
                        Strokes.Remove(Strokes.Where(s => s.sum().checksum == newStroke.sum().checksum).First());
                        doMyStrokeRemovedExceptHistory(newStroke);
                    }
                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                    {
                        Strokes.Add(stroke);
                        doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                    }
                    
                }
                Dispatcher.adopt(() => Select(new StrokeCollection()));
            };
            redo();
            UndoHistory.Queue(undo, redo);
        }
        private void deleteSelectedStrokes(object _sender, ExecutedRoutedEventArgs _handler)
        {
            dirtySelectedRegions(null, null);
        }
        #endregion
        #region CommandMethods
        private void alwaysTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion
        #region utilityFunctions
        private StrokeCollection filter(IEnumerable<Stroke> from, string author)
        {
            if (inMeeting()) return new StrokeCollection(from);
            return new StrokeCollection(from.Where(s => s.tag().author == author));
        }
        
        public void FlushStrokes()
        {
            Dispatcher.adoptAsync(() =>Strokes.Clear());
            strokes = new List<StrokeChecksum>();
        }
        public void Disable()
        {
            EditingMode = InkCanvasEditingMode.None;
        }
        public void Enable()
        {
            Dispatcher.adoptAsync(() =>
            {
                if (EditingMode == InkCanvasEditingMode.None)
                    EditingMode = InkCanvasEditingMode.Ink;
            });
        }
        public void setPenColor(Color color)
        {
            DefaultDrawingAttributes.Color = color;
        }
        public void SetEditingMode(InkCanvasEditingMode mode)
        {
            EditingMode = mode;
        }
        #endregion
        protected override void HandlePaste()
        {
            var strokesBeforePaste = Strokes.Select(s => s).ToList();
            Paste();
            var newStrokes = Strokes.Where(s => !strokesBeforePaste.Contains(s)).ToList();
            ClearAdorners();
            foreach (var stroke in newStrokes)
            {
                doMyStrokeAdded(stroke, stroke.tag().privacy);
            }
            addAdorners();
        }
        protected override void HandleCopy()
        {
            CopySelection();
        }
        protected override void HandleCut()
        {
            var listToCut = new List<MeTLLib.DataTypes.TargettedDirtyElement>();
            foreach (var stroke in GetSelectedStrokes())
                listToCut.Add(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide,Globals.me,target,stroke.tag().privacy,stroke.sum().checksum.ToString()));
            CutSelection();
            foreach (var element in listToCut)
                Commands.SendDirtyStroke.Execute(element);
        }
        
        public override void showPrivateContent()
        {
        }
        public override void hidePrivateContent()
        {
          
        }
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new HandWritingAutomationPeer(this);
        }
    }
    public class PrivateAwareStroke: Stroke
    {
        private Stroke stroke;
        private Pen pen = new Pen();
        private bool isPrivate;
        private bool shouldShowPrivacy;
        private StreamGeometry geometry;
        private Stroke whiteStroke;
        private StrokeTag tag
        {
            get { return stroke.tag(); }
        }
        private StrokeChecksum sum
        {
            get { return stroke.sum(); }

        }
        public PrivateAwareStroke(Stroke stroke) : base(stroke.StylusPoints, stroke.DrawingAttributes)
        {
            var cs = new[] {55, 0, 0, 0}.Select(i => (byte) i).ToList();
            pen = new Pen(new SolidColorBrush(
                new Color{
                       A=cs[0],
                       R=stroke.DrawingAttributes.Color.R,
                       G=stroke.DrawingAttributes.Color.G,
                       B=stroke.DrawingAttributes.Color.B
                    }), stroke.DrawingAttributes.Width * 4);
            this.stroke = stroke;
            
            this.tag(stroke.tag());
            isPrivate = this.tag().privacy == "private";
            shouldShowPrivacy = (this.tag().author == Globals.conversationDetails.Author || Globals.conversationDetails.Permissions.studentCanPublish);
            
            if (!isPrivate) return;

            pen.Freeze();
        }
        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (isPrivate && shouldShowPrivacy)
            {
            
                if (!stroke.DrawingAttributes.IsHighlighter)
                {

                    whiteStroke = stroke.Clone();
                    whiteStroke.DrawingAttributes.Color = Colors.White;
                }
                var wideStroke = this.stroke.GetGeometry().GetWidenedPathGeometry(pen).GetFlattenedPathGeometry()
                    .Figures
                    .SelectMany(f => f.Segments.Where(s => s is PolyLineSegment).SelectMany(s=> ((PolyLineSegment)s).Points));
                geometry = new StreamGeometry();
                using(var context = geometry.Open())
                {
                    context.BeginFigure(wideStroke.First(),false , false);
                    for (var i = 0; i < stroke.StylusPoints.Count; i++)
                        context.LineTo(stroke.StylusPoints.ElementAt(i).ToPoint(), true, true);
                    context.LineTo(wideStroke.Reverse().First(), false, false);
                }
                    drawingContext.DrawGeometry(null, pen, geometry);
            }
            if(whiteStroke != null && isPrivate)
               whiteStroke.Draw(drawingContext);
            else
               base.DrawCore(drawingContext, drawingAttributes);
        }
    }
    class HandWritingAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public HandWritingAutomationPeer(HandWriting parent) : base(parent) { }
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;
            return base.GetPattern(patternInterface);
        }
        private HandWriting HandWriting
        {
            get { return (HandWriting)base.Owner; }
        }
        protected override string GetAutomationIdCore()
        {
            return "handwriting";
        }
        public void SetValue(string value)
        {
            HandWriting.ParseInjectedStream(value, element => HandWriting.Dispatcher.adopt((Action)delegate
                                            {
                                                foreach (var ink in element.SelectElements<MeTLStanzas.Ink>(true))
                                                {
                                                    var stroke = ink.Stroke.stroke;
                                                    HandWriting.doMyStrokeAdded(stroke);
                                                    HandWriting.strokes.Remove(stroke.sum());//Pretend we haven't seen it - IRL it would be on the screen already.
                                                }
                                            }));
        }
        bool IValueProvider.IsReadOnly
        {
            get { return false; }
        }
        string IValueProvider.Value
        {
            get
            {
                var hw = (HandWriting)base.Owner;
                var sb = new StringBuilder("<strokes>");
                foreach (var toString in from stroke in hw.Strokes
                                         select new MeTLLib.DataTypes.MeTLStanzas.Ink(new MeTLLib.DataTypes.TargettedStroke(Globals.slide,Globals.me,hw.target,hw.privacy,stroke))
                                         .ToString())
                    sb.Append(toString);
                sb.Append("</strokes>");
                return sb.ToString();
            }
        }
    }
    public class LiveInkCanvas : HandWriting
    {//Warning!  This one is the biggest message hog in the universe!  But it's live transmitting ink
        public LiveInkCanvas()
            : base()
        {
            this.StylusPlugIns.Add(new LiveNotifier(this));
        }

    }
    public class LiveNotifier : StylusPlugIn
    {
        private LiveInkCanvas parent;
        public LiveNotifier(LiveInkCanvas parent)
        {
            this.parent = parent;
        }
        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            base.OnStylusDown(rawStylusInput);
            rawStylusInput.NotifyWhenProcessed(null);
        }
        protected override void OnStylusMove(RawStylusInput rawStylusInput)
        {
            base.OnStylusMove(rawStylusInput);
            rawStylusInput.NotifyWhenProcessed(rawStylusInput.GetStylusPoints());
        }
        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            base.OnStylusUp(rawStylusInput);
            rawStylusInput.NotifyWhenProcessed(null);
        }
        protected override void OnStylusDownProcessed(object callbackData, bool targetVerified)
        {
            base.OnStylusDownProcessed(callbackData, targetVerified);
            if (parent.target == "presentationSpace")
                Projector.PenUp();
        }
        protected override void OnStylusMoveProcessed(object callbackData, bool targetVerified)
        {
            base.OnStylusMoveProcessed(callbackData, targetVerified);
            if (parent.target == "presentationSpace")
                Projector.PenMoving(callbackData as StylusPointCollection);
        }
        protected override void OnStylusUpProcessed(object callbackData, bool targetVerified)
        {
            base.OnStylusUpProcessed(callbackData, targetVerified);
            if (parent.target == "presentationSpace")
                Projector.PenUp();
        }
    }
}
