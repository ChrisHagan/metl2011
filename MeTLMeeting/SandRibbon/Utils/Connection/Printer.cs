using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using PrintDialog = System.Windows.Controls.PrintDialog;
using MeTLLib.DataTypes;
using MeTLLib;
using MeTLLib.Providers;
using MeTLLib.Providers.Connection;
using HttpResourceProvider = MeTLLib.Providers.Connection.HttpResourceProvider;
using System.Windows.Ink;

namespace SandRibbon.Utils.Connection
{
    public class PrintParser : MeTLLib.Providers.Connection.PreParser
    {
        public List<object> history = new List<object>();
        private PrinterMoveDeltaProcessor moveDeltaProcessor;
        public PrintParser(
            Credentials credentials,
            int room,
            MeTLLib.Providers.Structure.IConversationDetailsProvider conversationDetailsProvider,
            HttpHistoryProvider historyProvider,
            CachedHistoryProvider cachedHistoryProvider,
            MetlConfiguration metlServerAddress,
            ResourceCache cache,
            IReceiveEvents receiveEvents,
            IWebClientFactory webClientFactory,
            HttpResourceProvider httpResourceProvider,
            IAuditor _auditor)
            : base(credentials, room, conversationDetailsProvider, historyProvider, cachedHistoryProvider, metlServerAddress, cache, receiveEvents, webClientFactory, httpResourceProvider,_auditor)
        {
        }
        //Please not that notepad is current disabled. the code has been left in as it does not interfere with the execution.
        public IEnumerable<MeTLInkCanvas> ToVisualWithNotes()
        {
            return createVisual();
        }
        public IEnumerable<MeTLInkCanvas> ToVisualWithoutNotes()
        {
            var canvases = createVisual("presentationSpace", true);
            return new[] { canvases.FirstOrDefault() };
        }
        
        private void ResizeCanvas(MeTLInkCanvas canvas, ContentBuffer contentBuffer)
        {
            contentBuffer.AdjustContent();
            ReAddFilteredContent(canvas, contentBuffer, ContentVisibilityEnum.AllVisible);
        }

        private void ReAddFilteredContent(MeTLInkCanvas canvas, ContentBuffer contentBuffer, ContentVisibilityEnum contentVisibility)
        {
            canvas.Strokes.Clear();
            canvas.Strokes.Add(new StrokeCollection(contentBuffer.FilteredStrokes(contentVisibility).Select(s => s as Stroke)));

            canvas.Children.Clear();
            foreach (var child in contentBuffer.FilteredTextBoxes(contentVisibility))
                canvas.Children.Add(child);
            foreach (var child in contentBuffer.FilteredImages(contentVisibility))
                canvas.Children.Add(child);
        }

        private IEnumerable<MeTLInkCanvas> createVisual(string target, bool includePublic)
        {
            var canvas = new MeTLInkCanvas();
            var contentBuffer = new ContentBuffer();
            moveDeltaProcessor = new PrinterMoveDeltaProcessor(canvas, target,contentBuffer);
            foreach (var stroke in ink)
            {
                if ((includePublic && stroke.privacy == Privacy.Public) || stroke.target == target)
                    contentBuffer.AddStroke(new PrivateAwareStroke(stroke.stroke, target), s => canvas.Strokes.Add(s));
            }
            foreach (var image in images)
            {
                var imageToAdd = image.Value.imageSpecification.forceEvaluationForPrinting();
                imageToAdd.tag(image.Value.image.tag());
                if ((includePublic && image.Value.privacy == Privacy.Public) || image.Value.target == target)
                {
                    Panel.SetZIndex(imageToAdd, image.Value.privacy == Privacy.Public ? 1 : 2);
                    contentBuffer.AddImage(imageToAdd, i => canvas.Children.Add(i));
                }
            }
            foreach (var box in text)
            {
                var textbox = box.Value.box;
                textbox.BorderThickness = new Thickness(0);
                textbox.BorderBrush = new SolidColorBrush(Colors.Transparent);
                textbox.Background = new SolidColorBrush(Colors.Transparent);
                if ((includePublic && box.Value.privacy == Privacy.Public) || box.Value.target == target)
                {
                    // positive Z is out of the screen
                    Panel.SetZIndex(textbox, 3);
                    contentBuffer.AddTextBox(textbox.toMeTLTextBox(), t => canvas.Children.Add(t));
                }
            }

            ResizeCanvas(canvas, contentBuffer);
            //foreach (var moveDelta in moveDeltas)
            //    moveDeltaProcessor.ReceiveMoveDelta(moveDelta, SandRibbon.Providers.Globals.me, true);

            if (canvas.Children.Count == 0 && canvas.Strokes.Count == 0)
                return new List<MeTLInkCanvas>();

            var tempPrinter = new PrintDialog();
            var size = new Size(tempPrinter.PrintableAreaWidth, tempPrinter.PrintableAreaHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(new Point(0,0), size));

            return new[] { canvas };
        }
        private IEnumerable<MeTLInkCanvas> createVisual()
        {
            var canvasList = new List<MeTLInkCanvas>();

            var presentationVisual = createVisual("presentationSpace", includePublic: true);
            if (presentationVisual != null && presentationVisual.Count() > 0)
                canvasList.AddRange(presentationVisual);

            var notesVisual = createVisual("notepad", includePublic: false);
            if (notesVisual != null && notesVisual.Count() > 0)
                canvasList.AddRange(notesVisual);

            return canvasList;
        }
        public override void actOnQuizReceived(MeTLLib.DataTypes.QuizQuestion details)
        {
            //Nothing.  Printer doesn't care about quiz
        }
    }
    public class Printer
    {
        private static int targetPageCount;
        private static int targetParserCount;
        public PrinterInformation PrinterInfo = new PrinterInformation();

        public class PrinterInformation
        {
            public List<Slide> slides;
            public string title;
            public Dictionary<string, PrintParser> parsers = new Dictionary<string, PrintParser>();
        }
        public void PrintHandout(string jid, string user)
        {
            var printDocument = new Action<IEnumerable<PrintParser>>(ShowPrintDialogWithoutNotes);
            var conversation = App.controller.client.DetailsOf(jid);
            targetPageCount = conversation.Slides.Where(s => s.type == MeTLLib.DataTypes.Slide.TYPE.SLIDE).Count();
            targetParserCount = targetPageCount;
            PrinterInfo = new PrinterInformation
                              {
                                  parsers = new Dictionary<string, PrintParser>(),
                                  title = jid,
                                  slides = conversation.Slides
                              };
            foreach (var slide in conversation.Slides.Where(s => s.type == MeTLLib.DataTypes.Slide.TYPE.SLIDE).OrderBy(s => s.index))
            {
                var room = slide.id;
                App.controller.client.historyProvider.Retrieve<PrintParser>(
                                null,
                                null,
                                (parser) => ReceiveParser(parser, printDocument, room),
                                room.ToString());
            }
        }
        public void PrintPrivate(string jid, string user)
        {
            var printDocument = new Action<IEnumerable<PrintParser>>(ShowPrintDialogWithNotes);
            var conversation = App.controller.client.DetailsOf(jid);
            targetPageCount = conversation.Slides.Where(s => s.type == Slide.TYPE.SLIDE).Count();
            targetParserCount = targetPageCount * 2;
            PrinterInfo = new PrinterInformation
                              {
                                  parsers = new Dictionary<string, PrintParser>(),
                                  title = jid,
                                  slides = conversation.Slides
                              };
            foreach (var slide in conversation.Slides.Where(s => s.type == MeTLLib.DataTypes.Slide.TYPE.SLIDE).OrderBy(s => s.index))
            {
                var room = slide.id;
                var parsers = new List<PrintParser>();
                App.controller.client.historyProvider.Retrieve<PrintParser>(
                                null,
                                null,
                                (parser) => ReceiveParser(parser, printDocument, room),
                                room.ToString());
                App.controller.client.historyProvider.RetrievePrivateContent<PrintParser>(
                                null,
                                null,
                                (parser) => ReceiveParser(parser, printDocument, room),
                                user,
                                room.ToString());
            }
        }
        int parsers = 0;
        private void ReceiveParser(PrintParser parser, Action<IEnumerable<PrintParser>> ShowPrintDialog, int room)
        {
            parsers++;
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.PRINTING, parsers, targetParserCount));
            if (PrinterInfo.parsers.ContainsKey(room.ToString()))
            {
                var Merged = PrinterInfo.parsers[room.ToString()].merge(parser);
                Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.PRINTING, parsers, targetParserCount));
                PrinterInfo.parsers[room.ToString()] = Merged;
            }
            else
            {
                Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.PRINTING, parsers, targetParserCount));
                PrinterInfo.parsers.Add(room.ToString(), parser);
            }
            if (PrinterInfo.parsers.Count() == targetPageCount && parsers == targetParserCount)
            {
                var indicesByJid = PrinterInfo.slides.Aggregate(new Dictionary<string, int>(),
                    (acc, item) =>
                    {
                        acc.Add(item.id.ToString(), item.index);
                        return acc;
                    });
                ShowPrintDialog(from p in PrinterInfo.parsers orderby indicesByJid[p.Key] select p.Value);

            }
        }
        private void ShowPrintDialogWithNotes(IEnumerable<PrintParser> parsers)
        {
            var visuals = parsers.Select(p => p.ToVisualWithNotes())
                                 .Aggregate(new List<MeTLInkCanvas>(),
                                                           (acc, item) =>
                                                           {
                                                               acc.AddRange(item);
                                                               return acc;
                                                           });
            HandlePrint(visuals.Where(v => v != null));
        }
        private void ShowPrintDialogWithoutNotes(IEnumerable<PrintParser> parsers)
        {
            var visuals = parsers.Select(p => p.ToVisualWithoutNotes())
                                 .Aggregate(new List<MeTLInkCanvas>(),
                                                           (acc, item) =>
                                                           {
                                                               acc.AddRange(item);
                                                               return acc;
                                                           });
            HandlePrint(visuals.Where(v => v != null));
        }
        private void HandlePrint(IEnumerable<MeTLInkCanvas> visuals)
        {
            Application.Current.Dispatcher.adoptAsync((System.Action)delegate
              {
                  var printer = new PrintDialog { PageRangeSelection = PageRangeSelection.AllPages };
                  var result = printer.ShowDialog();
                  if ((bool)result)
                  {
                      var myDocument = new FixedDocument();
                      foreach (var visual in visuals)
                      {
                          var pageContent = new PageContent();
                          var page = new FixedPage
                                         {
                                             Width = printer.PrintableAreaWidth,
                                             Height = printer.PrintableAreaHeight
                                         };
                          var viewbox = new Viewbox();
                          viewbox.Width = page.Width;
                          viewbox.Height = page.Height;
                          viewbox.Child = (UIElement)visuallyAdjustedVisual(visual);
                          page.Children.Add(viewbox);
                          ((IAddChild)pageContent).AddChild(page);
                          myDocument.Pages.Add(pageContent);
                      }
                      printer.PrintDocument(myDocument.DocumentPaginator, "A document");
                  }
                  Commands.HideProgressBlocker.Execute(null);
              });
        }
        private MeTLInkCanvas visuallyAdjustedVisual(MeTLInkCanvas visual)
        {
            foreach (var box in visual.TextChildren())
            {
                var textbox = (TextBox)box;
                var testSize = new Size(textbox.ActualWidth, textbox.ActualHeight);
                textbox.Measure(testSize);
                textbox.Arrange(new Rect(testSize));
                var lastCharRect = textbox.GetRectFromCharacterIndex(textbox.Text.Count());
                if (textbox.Height < lastCharRect.Bottom)
                    textbox.Height = lastCharRect.Bottom;
            }
            return visual;
        }
        private class ThumbBox : Viewbox
        {
            public static int THUMBNAIL_WIDTH = 96;
            public static int THUMBNAIL_HEIGHT = 96;
            private string filename;
            public ThumbBox(UIElement child, string filename)
                : base()
            {
                this.Child = child;
                this.filename = filename;
                this.Width = THUMBNAIL_WIDTH;
                this.Height = THUMBNAIL_HEIGHT;
            }
            public void Save()
            {
                var descendantRect = VisualTreeHelper.GetDescendantBounds(this);
                this.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(this);
                BitmapFrame frame = BitmapFrame.Create(bitmap);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);
                using (var stream = File.Create(filename))
                    encoder.Save(stream);
            }
        }
    }
}
