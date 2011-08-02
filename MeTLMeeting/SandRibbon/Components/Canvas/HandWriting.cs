using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using MeTLLib;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbonObjects;
using MeTLLib.DataTypes;
using System.Diagnostics;

namespace SandRibbon.Components.Canvas
{
    public class HandWriting : AbstractCanvas
    {
        public HandWriting()
        {
            Loaded += HandWritingLoaded;
            StrokeCollected += singleStrokeCollected;
            SelectionChanging += selectingStrokes;
            SelectionChanged += selectionChanged;
            StrokeErasing += erasingStrokes;
            SelectionMoving += selectionMoving;
            SelectionMoved += selectionMoved;
            SelectionResizing += selectionMoving;
            SelectionResized += selectionMoved;
            Background = Brushes.Transparent;
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, deleteSelectedStrokes));
            Commands.SetInkCanvasMode.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(setInkCanvasMode, canChangeMode));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>((stroke) => ReceiveStrokes(new[] { stroke })));
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<IEnumerable<TargettedStroke>>(ReceiveStrokes));
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<string>(changeSelectedItemsPrivacy));
            Commands.ReceiveDirtyStrokes.RegisterCommand(new DelegateCommand<IEnumerable<TargettedDirtyElement>>(ReceiveDirtyStrokes));
            Commands.DeleteSelectedItems.RegisterCommandToDispatcher(new DelegateCommand<object>(deleteSelectedItems));
            Commands.HideConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(hideConversationSearchBox));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<object>(updateStrokePrivacy));
            Commands.ZoomChanged.RegisterCommand(new DelegateCommand<Double>(ZoomChanged));
            Commands.SetDrawingAttributes.RegisterCommandToDispatcher(new DelegateCommand<DrawingAttributes>(SetDrawingAttributes));
        }
        private void updateStrokePrivacy(object obj)
        {
            ClearAdorners();

            var newStrokes = new StrokeCollection(Strokes.Select(s => (Stroke)new PrivateAwareStroke(s, target)));
            Strokes.Clear();
            Strokes.Add(newStrokes);
        }
        private void hideConversationSearchBox(object obj)
        {
            addAdorners();
        }
        private Double zoom = 1;
        private void ZoomChanged(Double zoom)
        {
            this.zoom = zoom;
            try
            {
                SetDrawingAttributes((DrawingAttributes)Commands.SetDrawingAttributes.lastValue());
            }
            catch (NotSetException) { }
        }
        private void SetDrawingAttributes(DrawingAttributes logicalAttributes)
        {
            if (me.ToLower() == "projector") return;
            var zoomCompensatedAttributes = logicalAttributes.Clone();
            try
            {
                zoomCompensatedAttributes.Width = logicalAttributes.Width * zoom;
                zoomCompensatedAttributes.Height = logicalAttributes.Height * zoom;
                var visualAttributes = logicalAttributes.Clone();
                visualAttributes.Width = logicalAttributes.Width * 2;
                visualAttributes.Height = logicalAttributes.Height * 2;
                UseCustomCursor = true;
                Cursor = CursorExtensions.generateCursor(visualAttributes);
            }
            catch (Exception e) {
                Trace.TraceInformation("Cursor failed (no crash):", e.Message);
            }
            DefaultDrawingAttributes = zoomCompensatedAttributes;
        }
        public List<string> getSelectedAuthors()
        {
            return GetSelectedStrokes().Where(s => s.tag().author != me).Select(s => s.tag().author).Distinct().ToList();
        }

        public void deleteSelectedItems(object obj)
        {
            if(GetSelectedStrokes().Count == 0) return;
            deleteSelectedStrokes(null, null);
            ClearAdorners();
        }
        private void HandWritingLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DefaultPenAttributes();
        }
        public static Guid STROKE_PROPERTY = Guid.NewGuid();
        public List<StrokeChecksum> strokes = new List<StrokeChecksum>();
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
                Commands.RequerySuggested(Commands.SetInkCanvasMode);
            }
        }
        private static bool canChangeMode(string arg)
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
                UseCustomCursor = EditingMode == InkCanvasEditingMode.Ink;
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
        public void ReceiveStrokes(IEnumerable<TargettedStroke> receivedStrokes)
        {
            if (receivedStrokes.Count() == 0) return;
            if (receivedStrokes.First().slide != currentSlide) return;
            var strokeTarget = target;
            Dispatcher.adopt(
                delegate
                {
                    var newStrokes = new StrokeCollection(
                        receivedStrokes.Where(ts => ts.target == strokeTarget)
                        .Where(s => s.privacy == "public" || (s.author == Globals.me && me != "projector"))
                        .Select(s => (Stroke)new PrivateAwareStroke(s.stroke, target))
                        .Where(s => !(strokes.Contains(s.sum()))));
                    Strokes.Add(newStrokes);
                    strokes.AddRange(newStrokes.Select(s => s.sum()));
                });
        }
        public void ReceiveDirtyStrokes(IEnumerable<TargettedDirtyElement> targettedDirtyStrokes)
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
            Dispatcher.adoptAsync(addAdorners);
        }

        protected internal void addAdorners()
        {
            ClearAdorners();
            var selectedStrokes = GetSelectedStrokes();
            if (selectedStrokes.Count == 0) return;
            var publicStrokes = selectedStrokes.Where(s => s.tag().privacy.ToLower() == "public").ToList();
            var myStrokes = selectedStrokes.Where(s => s.tag().author == Globals.me);
            string privacyChoice;
            if (publicStrokes.Count == 0)
                privacyChoice = "show";
            else if (publicStrokes.Count == selectedStrokes.Count)
                privacyChoice = "hide";
            else
                privacyChoice = "both";
            Commands.AddPrivacyToggleButton.Execute(new PrivacyToggleButton.PrivacyToggleButtonInfo(privacyChoice, myStrokes.Count() != 0, GetSelectionBounds()));
        }
        public StrokeCollection GetMySelectedStrokes()
        {
            return filter(base.GetSelectedStrokes(), Globals.me, true);
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
            GlobalTimers.resetSyncTimer();
            var privateAwareStroke = new PrivateAwareStroke(e.Stroke, target);
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
                Trace.TraceInformation("ErasingStroke {0}", e.Stroke.sum().checksum);
                doMyStrokeRemoved(e.Stroke);
            }
            catch
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
                    ClearAdorners();
                    var existingStroke = Strokes.Where(s => s.sum().checksum == thisStroke.sum().checksum).FirstOrDefault();
                    if (existingStroke != null)
                    {
                        Strokes.Remove(existingStroke);
                        doMyStrokeRemovedExceptHistory(existingStroke);
                    }
                },
                () =>
                {
                    ClearAdorners();
                    if (Strokes.Where(s => s.sum().checksum == thisStroke.sum().checksum).Count() == 0)
                    {
                        Strokes.Add(thisStroke);
                        doMyStrokeAddedExceptHistory(thisStroke, thisStroke.tag().privacy);
                    }
                    if(EditingMode == InkCanvasEditingMode.Select)
                        Select(new StrokeCollection(new [] {thisStroke}));
                    addAdorners();
                });
        }
        private void doMyStrokeAddedExceptHistory(Stroke stroke, string thisPrivacy)
        {
            if (!strokes.Contains(stroke.sum()))
                strokes.Add(stroke.sum());
            stroke.tag(new StrokeTag { author = stroke.tag().author, privacy = thisPrivacy, isHighlighter = stroke.DrawingAttributes.IsHighlighter });
            SendTargettedStroke(stroke, thisPrivacy);
        }
        public void SendTargettedStroke(Stroke stroke, string thisPrivacy)
        {
            if (!stroke.shouldPersist()) return;
            var privateRoom = string.Format("{0}{1}", currentSlide, stroke.tag().author);
            if(thisPrivacy.ToLower() == "private" && Globals.isAuthor && Globals.me != stroke.tag().author)
                Commands.SneakInto.Execute(privateRoom);
            Commands.SendStroke.Execute(new TargettedStroke(currentSlide,stroke.tag().author,target,stroke.tag().privacy,stroke, stroke.tag().startingSum));
            if (thisPrivacy.ToLower() == "private" && Globals.isAuthor && Globals.me != stroke.tag().author)
                Commands.SneakOutOf.Execute(privateRoom);
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
            Commands.SendDirtyStroke.Execute(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, stroke.tag().author,target,stroke.tag().privacy,sum));
        }

        private List<Stroke> strokesAtTheStart = new List<Stroke>();
        private void selectionMoving (object _sender, InkCanvasSelectionEditingEventArgs _e)
        {
            strokesAtTheStart.Clear();
            foreach (var stroke in GetSelectedStrokes())
            {
                strokesAtTheStart.Add(stroke.Clone());
            }
        }

        protected static void abosoluteizeStrokes(List<Stroke> selectedElements)
        {
            foreach (var stroke in selectedElements)
            {
                if (stroke.GetBounds().Top < 0)
                    stroke.GetBounds().Offset(0, Math.Abs(stroke.GetBounds().Top));
                if (stroke.GetBounds().Left < 0)
                    stroke.GetBounds().Offset(Math.Abs(stroke.GetBounds().Left), 0);
                if (stroke.GetBounds().Left < 0 && stroke.GetBounds().Top < 0)
                    stroke.GetBounds().Offset(Math.Abs(stroke.GetBounds().Left), Math.Abs(stroke.GetBounds().Top));
            }
           
        }
        private void selectionMoved(object sender, EventArgs e)
        {
            var selectedStrokes = GetSelectedStrokes().Select(stroke => stroke.Clone()).ToList();
            Trace.TraceInformation("MovingStrokes {0}", string.Join(",", selectedStrokes.Select(s => s.sum().checksum.ToString()).ToArray()));
            var undoStrokes = strokesAtTheStart.Select(stroke => stroke.Clone()).ToList();
            abosoluteizeStrokes(selectedStrokes);
            Action redo = () =>
                {
                    removeStrokes(undoStrokes); 
                    addStrokes(selectedStrokes); 
                    if(EditingMode == InkCanvasEditingMode.Select)
                        Select(new StrokeCollection(selectedStrokes));
                };
            Action undo = () =>
                {
                   
                    removeStrokes(selectedStrokes);
                    addStrokes(undoStrokes); 
                    
                    if(EditingMode == InkCanvasEditingMode.Select)
                        Select(new StrokeCollection(undoStrokes));

                };
            ClearAdorners();
            removeStrokes(undoStrokes);
            addStrokes(selectedStrokes);
            addAdorners();
            UndoHistory.Queue(undo, redo);
        }
        private void removeStrokes(IEnumerable<Stroke> undoStrokes)
        {
            foreach (var stroke in undoStrokes)
            {
                if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                {
                    Strokes.Remove(Strokes.Where(s => Math.Round(s.sum().checksum, 1) == Math.Round(stroke.sum().checksum, 1)).First());
                    strokes.Remove(stroke.sum());
                }
                doMyStrokeRemovedExceptHistory(stroke);
            }
        }

        private void addStrokes(IEnumerable<Stroke> selectedStrokes)
        {
            foreach (var stroke in selectedStrokes)
            {
                if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                {
                    Strokes.Add(stroke);
                }
                doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
            }
        }

        private void dirtySelectedRegions(object _sender, InkCanvasSelectionEditingEventArgs _e)
        {
            var selectedStrokes = GetMySelectedStrokes();
            Action redo = () =>
                {
                    ClearAdorners();
                   removeStrokes(selectedStrokes.ToList()); 
                };
            Action undo = () =>
                {
                    ClearAdorners();
                    addStrokes(selectedStrokes.ToList());
                    
                    if(EditingMode == InkCanvasEditingMode.Select)
                        Select(selectedStrokes);

                    addAdorners();
                };
            redo();
            UndoHistory.Queue(undo, redo);
        }
        private void changeSelectedItemsPrivacy(string newPrivacy)
        {
            if (me == "projector") return;
            var selectedStrokes = new List<Stroke>();
            Dispatcher.adopt(() => selectedStrokes = GetSelectedStrokes().ToList());
            if (selectedStrokes.Count == 0) return;
            Action redo = () =>
            {

                var newStrokes = new StrokeCollection();
                foreach (var stroke in selectedStrokes.Where(i => i != null && i.tag().privacy != newPrivacy))
                {
                    var oldTag = stroke.tag();

                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() > 0)
                    {
                        Strokes.Remove(Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First());
                        doMyStrokeRemovedExceptHistory(stroke);
                    }
                    var newStroke = stroke.Clone();
                    newStroke.tag(new StrokeTag { author = oldTag.author, privacy = newPrivacy, startingSum = oldTag.startingSum });
                    if (Strokes.Where(s => s.sum().checksum == newStroke.sum().checksum).Count() == 0)
                    {
                        newStrokes.Add(newStroke);
                        Strokes.Add(newStroke);
                        doMyStrokeAddedExceptHistory(newStroke, newPrivacy);
                    }
                   
                }
                
                if(EditingMode == InkCanvasEditingMode.Select)
                    Select(newStrokes);
                Dispatcher.adopt(() => Select(new StrokeCollection()));
            };
            Action undo = () =>
            {
                var newStrokes = new StrokeCollection();
                foreach (var stroke in selectedStrokes.Where(i => i is Stroke && i.tag().privacy != newPrivacy))
                {
                    var oldTag = stroke.tag();
                    var newStroke = stroke.Clone();
                    newStroke.tag(new StrokeTag { author = oldTag.author, privacy = newPrivacy, startingSum = oldTag.startingSum });

                    if (Strokes.Where(s => s.sum().checksum == newStroke.sum().checksum).Count() > 0)
                    {
                        Strokes.Remove(Strokes.Where(s => s.sum().checksum == newStroke.sum().checksum).First());
                        doMyStrokeRemovedExceptHistory(newStroke);
                    }
                    if (Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).Count() == 0)
                    {
                        newStrokes.Add(newStroke);
                        Strokes.Add(stroke);
                        doMyStrokeAddedExceptHistory(stroke, stroke.tag().privacy);
                    }
                    Select(newStrokes);
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
            return filter(from, author, false);
        }
        private StrokeCollection filter(IEnumerable<Stroke> from, string author, bool justAuthors)
        {
            //Banhammer line of code
            //if (!justAuthors && (inMeeting() || Globals.conversationDetails.Author == Globals.me)) return new StrokeCollection(from);
            if (!justAuthors && inMeeting()) return new StrokeCollection(from);
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
        #endregion

        public override void showPrivateContent()
        {
        }

        public override void hidePrivateContent()
        {
        }

        protected override void HandlePaste()
        {
            var strokesBeforePaste = Strokes.Select(s => s).ToList();
            Paste();
            var newStrokes = Strokes.Where(s => !strokesBeforePaste.Contains(s)).ToList();
            var selection = new StrokeCollection();
            ClearAdorners();
            foreach (var stroke in newStrokes)
            {
                selection.Add(stroke);
                doMyStrokeAdded(stroke, stroke.tag().privacy);
            }
            
            if(EditingMode == InkCanvasEditingMode.Select)
                Select(selection);
            addAdorners();
        }
        protected override void HandleCopy()
        {
            CopySelection();
        }
        protected override void HandleCut()
        {
            var listToCut = GetSelectedStrokes().Select(stroke => new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide, stroke.tag().author, target, stroke.tag().privacy, stroke.sum().checksum.ToString())).ToList();
            var strokesToCut = GetSelectedStrokes().Select(s => s.Clone());
            var topPoint = GetSelectionBounds().TopLeft;
            CutSelection();
            ClearAdorners();
            Action redo = () =>
            {
                foreach (var element in listToCut)
                    Commands.SendDirtyStroke.Execute(element);
            };
            Action undo = () =>
            {
                
                ClearAdorners();
                foreach (var s in strokesToCut)
                {
                    Strokes.Add(s);
                    doMyStrokeAddedExceptHistory(s, s.tag().privacy);
                }
                addAdorners();
            };
            redo();
            UndoHistory.Queue(undo, redo);

        }
        protected override AutomationPeer OnCreateAutomationPeer()
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
        private string target;
        private Stroke whiteStroke;
        private StrokeTag tag
        {
            get { return stroke.tag(); }
        }
        private StrokeChecksum sum
        {
            get { return stroke.sum(); }

        }
        public PrivateAwareStroke(Stroke stroke, string target) : base(stroke.StylusPoints, stroke.DrawingAttributes)
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
            this.target = target; 
            this.tag(stroke.tag());
            isPrivate = this.tag().privacy == "private";
            shouldShowPrivacy = (this.tag().author == Globals.conversationDetails.Author || Globals.conversationDetails.Permissions.studentCanPublish);
            
            if (!isPrivate) return;

            pen.Freeze();
        }
        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (isPrivate && shouldShowPrivacy && target != "notepad")
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
    /*
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
     * */
}
