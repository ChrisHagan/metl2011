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
//using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;
using System.Windows;
using MeTLLib.DataTypes;

namespace SandRibbon.Components.Canvas
{
    public class HandWriting : AbstractCanvas
    {

        private Dictionary<string, List<MeTLLib.DataTypes.TargettedStroke>> userStrokes;
        public HandWriting()
        {
            userStrokes = new Dictionary<string, List<MeTLLib.DataTypes.TargettedStroke>>();
            Loaded += new System.Windows.RoutedEventHandler(HandWriting_Loaded);
            StrokeCollected += singleStrokeCollected;
            SelectionChanging += selectingStrokes;
            SelectionChanged += selectionChanged;
            StrokeErasing += erasingStrokes;
            SelectionMoving += dirtySelectedRegions;
            SelectionMoved += transmitSelectionAltered;
            SelectionResizing += dirtySelectedRegions;
            SelectionResized += transmitSelectionAltered;
            DefaultDrawingAttributesReplaced += announceDrawingAttributesChanged;
            Background = Brushes.Transparent;
            defaultWidth = DefaultDrawingAttributes.Width;
            defaultHeight = DefaultDrawingAttributes.Height;
            modeChangedCommand = new DelegateCommand<string>(setInkCanvasMode, canChangeMode);
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, deleteSelectedStrokes));
            Commands.SetInkCanvasMode.RegisterCommand(modeChangedCommand);
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
            Commands.ActualSetDrawingAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>(attributes =>
             {
                 DefaultDrawingAttributes = attributes;
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
            Commands.SetPrivacyOfItems.RegisterCommand(new DelegateCommand<string>(changeSelectedItemsPrivacy));
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedStroke>>(ReceiveStrokes));
            Commands.ReceiveDirtyStrokes.RegisterCommand(new DelegateCommand<IEnumerable<MeTLLib.DataTypes.TargettedDirtyElement>>(ReceiveDirtyStrokes));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(MoveTo));
            Commands.DeleteSelectedItems.RegisterCommand(new DelegateCommand<object>(deleteSelectedItems));
            Commands.UserVisibility.RegisterCommand(new DelegateCommand<VisibilityInformation>(setUserVisibility));
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
                        foreach(var key in keys)
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
                                          Strokes.Clear();
                                          strokes.Clear();
                                          privacyDictionary.Clear();
                                          updateVisibility(info);
                                          var visibleUsers =
                                              userVisibility.Keys.Where(u => userVisibility[u] == true).ToList();
                                          var allVisibleStrokes = new List<MeTLLib.DataTypes.TargettedStroke>();
                                          foreach (var user in visibleUsers.Where(user => userStrokes.ContainsKey(user)))
                                              allVisibleStrokes.AddRange(userStrokes[user]);
                                          ReceiveStrokes(allVisibleStrokes);
                                      });

        }

        private void UpdateCursor(object obj)
        {
            if (obj is Cursor)
            {
                UseCustomCursor = true;
                Cursor = (Cursor)obj;
            }
        }
        private void deleteSelectedItems(object obj)
        {
            deleteSelectedStrokes(null, null);
            ClearAdorners();
        }
        private void MoveTo(object obj)
        {
            privacyDictionary = new Dictionary<double, Stroke>();
            userStrokes = new Dictionary<string, List<MeTLLib.DataTypes.TargettedStroke>>();

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
                    try { UpdateCursor(Commands.UpdateCursor.lastValue()); }
                    catch (Exception) { }
                }
                else 
                {
                    UpdateCursor(CursorExtensions.generateCursor(EditingMode));
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
            Commands.ActualReportDrawingAttributes.Execute(this.DefaultDrawingAttributes);
        }
        private static List<TimeSpan> strokeReceiptDurations = new List<TimeSpan>();
        private static double averageStrokeReceiptDuration()
        {
            return strokeReceiptDurations.Aggregate(0.0, (acc, item) => acc + item.TotalMilliseconds) / strokeReceiptDurations.Count();
        }
        public void ReceiveStrokes(IEnumerable<MeTLLib.DataTypes.TargettedStroke> receivedStrokes)
        {
            if (receivedStrokes.Count() == 0) return;
            if (receivedStrokes.First().slide != currentSlide) return;
            var strokeTarget = target;
            Dispatcher.adoptAsync(
                delegate
                {
                    var start = SandRibbonObjects.DateTimeFactory.Now();
                    var newStrokes = new StrokeCollection(
                        receivedStrokes.Where(ts => ts.target == strokeTarget)
                        .Where(s => s.privacy == "public" || (s.author == Globals.me && me != "projector"))
                        //    when uncommenting line above, remove line below. WENDYS EXPERIMENT!
                        //    .Where(s => s.author == Globals.me)
                        .Select(s => s.stroke)
                        .Where(s => !(this.strokes.Contains(s.sum()))));
                    Strokes.Add(newStrokes);
                    this.strokes.AddRange(newStrokes.Select(s => s.sum()));
                    foreach (var stroke in receivedStrokes)
                    {
                        if (stroke.target == target)
                        {
                            var author= stroke.author==Globals.conversationDetails.Author? "Teacher" : stroke.author;
                            Commands.ReceiveAuthor.Execute(author);
                            if(!userStrokes.ContainsKey(author))
                                userStrokes.Add(author, new List<MeTLLib.DataTypes.TargettedStroke>());
                            if(!userStrokes[author].Contains(stroke))
                                userStrokes[author].Add(stroke);
                            if(!userVisibility.ContainsKey(author))
                                userVisibility.Add(author, true);
                            
                            if (stroke.privacy == "private" && me != "projector")
                                ApplyPrivacyStylingToStroke(stroke.stroke, stroke.privacy);
                        }
                    }
                    var duration = DateTimeFactory.Now() - start;
                    strokeReceiptDurations.Add(duration);
                });
        }
        #region eventHandlers
        private void selectionChanged(object sender, EventArgs e)
        {
            Dispatcher.adoptAsync((Action)addAdorners);
        }
        private void addAdorners()
        {
            ClearAdorners();
            var selectedStrokes = GetSelectedStrokes();
            if (selectedStrokes.Count() == 0) return;
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
            Commands.RequerySuggested(Commands.Undo);
            e.Stroke.startingSum(e.Stroke.sum().checksum);
            doMyStrokeAdded(e.Stroke);
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
                        removeStrokeFromPrivacyDictionary(stroke);
                        Strokes.Remove(stroke);
                        strokes.Remove(stroke.sum());
                        doMyStrokeRemoved(stroke);
                    }
                });
        }

        private void removeStrokeFromPrivacyDictionary(Stroke stroke)
        {
            if (privacyDictionary.Keys.Where(k => k == stroke.sum().checksum).Count() > 0)
                privacyDictionary.Remove(stroke.sum().checksum);
        }

        private void doMyStrokeRemovedExceptHistory(Stroke stroke)
        {
            var sum = stroke.sum().checksum.ToString();
            var bounds = stroke.GetBounds();
            if (stroke != null && stroke is Stroke)
                RemovePrivacyStylingFromStroke(stroke);
            Commands.SendDirtyStroke.Execute(new MeTLLib.DataTypes.TargettedDirtyElement(currentSlide,Globals.me,target,stroke.tag().privacy,sum));
        }
        private void transmitSelectionAltered(object sender, EventArgs e)
        {
            addAdorners();
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
        public void doMyStrokeAdded(Stroke stroke)
        {
            doMyStrokeAdded(stroke, privacy);
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
            if (!strokes.Contains(stroke.sum()))
                strokes.Add(stroke.sum());
            stroke.tag(new MeTLLib.DataTypes.StrokeTag { author = Globals.me, privacy = thisPrivacy, startingColor = stroke.DrawingAttributes.Color.ToString(), isHighlighter = stroke.DrawingAttributes.IsHighlighter });
            SendTargettedStroke(stroke, thisPrivacy);
            ApplyPrivacyStylingToStroke(stroke, thisPrivacy);
        }
        public void SendTargettedStroke(Stroke stroke, string thisPrivacy)
        {
            try
            {
                Commands.ActualReportStrokeAttributes.Execute(stroke.DrawingAttributes);
                Commands.SendStroke.Execute(new MeTLLib.DataTypes.TargettedStroke(currentSlide,Globals.me,target,stroke.tag().privacy,stroke));
            }
            catch (NotSetException e)
            {
            }
        }
        public void FlushStrokes()
        {
            Dispatcher.adoptAsync(delegate { Strokes.Clear(); });
            strokes = new List<MeTLLib.DataTypes.StrokeChecksum>();
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
                stroke.DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(stroke.tag().startingColor);
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
        public void ReceiveDirtyStrokes(IEnumerable<MeTLLib.DataTypes.TargettedDirtyElement> targettedDirtyStrokes)
        {
            if (targettedDirtyStrokes.Count() == 0) return;
            if (!(targettedDirtyStrokes.First().target.Equals(target)) || targettedDirtyStrokes.First().slide != currentSlide) return;
            Dispatcher.adoptAsync(delegate
            {
                var dirtyChecksums = targettedDirtyStrokes.Select(t => t.identifier);
                foreach (var stroke in targettedDirtyStrokes)
                {
                    var author = stroke.author == Globals.conversationDetails.Author ? "Teacher" : stroke.author;
                    if(userStrokes.ContainsKey(author))
                    {
                        var strokeList = userStrokes[author];
                        var dirtyStrokes = strokeList.Where(s => dirtyChecksums.Contains(s.stroke.sum().checksum.ToString())).Select(s => s.stroke.sum().checksum);
                        var newStrokes = strokeList.Where(s => !dirtyStrokes.Contains(s.stroke.sum().checksum)).ToList();
                        userStrokes[author] = newStrokes;
                    }
                }
                var presentDirtyStrokes = Strokes.Where(s => dirtyChecksums.Contains(s.sum().checksum.ToString())).ToList();
                for (int i = 0; i < presentDirtyStrokes.Count(); i++)
                {
                    var stroke = presentDirtyStrokes[i];
                    strokes.Remove(stroke.sum());
                    removeStrokeFromPrivacyDictionary(stroke);
                    Strokes.Remove(stroke);

                }
            });
        }
        private Dictionary<double, Stroke> privacyDictionary = new Dictionary<double, Stroke>();
        private void ApplyPrivacyStylingToStroke(Stroke stroke, string privacy)
        {
            if (!Globals.isAuthor || Globals.conversationDetails.Permissions == MeTLLib.DataTypes.Permissions.LECTURE_PERMISSIONS || target == "notepad") return;
            if (privacy == "private")
                addPrivacyStylingToStroke(stroke);
            else
                RemovePrivacyStylingFromStroke(stroke);
        }
        private void RemovePrivacyStylingFromStroke(Stroke stroke)
        {
            if (!Globals.isAuthor || Globals.conversationDetails.Permissions == MeTLLib.DataTypes.Permissions.LECTURE_PERMISSIONS) return;
            try
            {
                if (stroke.tag().startingColor != null)
                {

                    stroke.DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(stroke.tag().startingColor);
                    if (privacyDictionary.Keys.Where(k => k == stroke.sum().checksum).Count() == 0) return;
                    stroke.DrawingAttributes.IsHighlighter =
                        privacyDictionary[stroke.sum().checksum].tag().isHighlighter;
                    Strokes.Remove(privacyDictionary[stroke.sum().checksum]);
                    privacyDictionary.Remove(stroke.sum().checksum);
                }
            }
            catch (Exception e)
            { }
        }
        private void addPrivacyStylingToStroke(Stroke stroke)
        {
            if (privacyDictionary.Keys.Where(k => k == stroke.sum().checksum).Count() != 0) return;
            var privacyStroke = new Stroke(stroke.StylusPoints);
            privacyStroke.tag(new MeTLLib.DataTypes.StrokeTag { author = "Privacy", privacy = "private", startingColor = stroke.DrawingAttributes.Color.ToString(), isHighlighter = stroke.DrawingAttributes.IsHighlighter });
            privacyStroke.DrawingAttributes = new DrawingAttributes
                                                  {
                                                      Color = (Color)ColorConverter.ConvertFromString(stroke.tag().startingColor),
                                                      Width = stroke.DrawingAttributes.Width * 2,
                                                      Height = stroke.DrawingAttributes.Height * 2,
                                                      IsHighlighter = true
                                                  };
            stroke.DrawingAttributes.Color = Colors.White;
            stroke.DrawingAttributes.IsHighlighter = false;
            privacyStroke.startingSum(privacyStroke.sum().checksum);
            privacyDictionary[stroke.sum().checksum] = (privacyStroke);
            Strokes.Add(privacyStroke);
        }
        public override void showPrivateContent()
        {
            var currentStrokes = Strokes.Clone();
            foreach (Stroke stroke in currentStrokes)
            {
                if (stroke.tag().privacy == "private")
                {
                    Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First().DrawingAttributes.Color = MeTLStanzas.Ink.stringToColor(stroke.tag().startingColor);
                    if (Globals.isAuthor)
                        ApplyPrivacyStylingToStroke(Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First(), stroke.tag().privacy);
                }
            }
        }
        public override void hidePrivateContent()
        {
            var validStrokes = Strokes.Where(s => s.tag().author.ToLower() != "privacy").ToList();
            foreach (Stroke stroke in validStrokes)
            {
                if (stroke.tag().privacy == "private")
                {
                    var selectedStroke = Strokes.Where(s => s.sum().checksum == stroke.sum().checksum).First();
                    RemovePrivacyStylingFromStroke(selectedStroke);
                    selectedStroke.DrawingAttributes.Color = Colors.Transparent;
                }
            }
        }
        private void changeSelectedItemsPrivacy(string newPrivacy)
        {
            if (me == "projector") return;
            foreach (Stroke stroke in GetSelectedStrokes().ToList().Where(i => i is Stroke && i.tag().privacy != newPrivacy))
            {
                var oldTag = ((Stroke)stroke).tag();
                doMyStrokeRemoved(stroke);
                var newStroke = stroke.Clone();
                ((Stroke)newStroke).tag(new MeTLLib.DataTypes.StrokeTag { author = oldTag.author, privacy = newPrivacy, startingColor = oldTag.startingColor, startingSum = oldTag.startingSum });
                doMyStrokeAdded(newStroke, newPrivacy);
            }
            Select(new StrokeCollection());
        }

        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new HandWritingAutomationPeer(this);
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