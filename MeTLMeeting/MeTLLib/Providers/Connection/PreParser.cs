using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using MeTLLib.DataTypes;
using System.Diagnostics;
using agsXMPP.Xml.Dom;
//using Ninject;

namespace MeTLLib.Providers.Connection
{
    public class PreParser : JabberWire
    {
        public Dictionary<string, TargettedImage> images = new Dictionary<string, TargettedImage>();
        public List<TargettedStroke> ink = new List<TargettedStroke>();
        public List<Attendance> attendances = new List<Attendance>();
        public List<QuizQuestion> quizzes = new List<QuizQuestion>();
        public List<TargettedFile> files = new List<TargettedFile>();
        public List<TargettedSubmission> submissions = new List<TargettedSubmission>();
        public List<TargettedMoveDelta> moveDeltas = new List<TargettedMoveDelta>();
        public List<QuizAnswer> quizAnswers = new List<QuizAnswer>();
        public List<MeTLStanzas.DirtyInk> dirtyInk = new List<MeTLStanzas.DirtyInk>();
        public List<MeTLStanzas.DirtyText> dirtyText = new List<MeTLStanzas.DirtyText>();
        public List<MeTLStanzas.DirtyImage> dirtyImage = new List<MeTLStanzas.DirtyImage>();
        public Dictionary<string, TargettedTextBox> text = new Dictionary<string, TargettedTextBox>();
        public Dictionary<string, LiveWindowSetup> liveWindows = new Dictionary<string, LiveWindowSetup>();
        public PreParser(Credentials credentials, int room, Structure.IConversationDetailsProvider conversationDetailsProvider, HttpHistoryProvider historyProvider, CachedHistoryProvider cachedHistoryProvider, MetlConfiguration metlServerAddress, ResourceCache cache, IReceiveEvents receiveEvents, IWebClientFactory webClientFactory, HttpResourceProvider resourceProvider, IAuditor _auditor)
            : base(credentials, conversationDetailsProvider, historyProvider, cachedHistoryProvider, metlServerAddress, cache, receiveEvents, webClientFactory, resourceProvider, false, _auditor)
        {
            if (this.location == null)
                this.location = new Location("0", 1, new List<int> { 1 });
            this.location.currentSlide = room;
            this.receiveEvents = receiveEvents;
        }
        public T merge<T>(T otherParser) where T : PreParser
        {
            return auditor.wrapFunction((a =>
            {
                var returnParser = (T)Activator.CreateInstance(typeof(T),
                    credentials,
                    location.currentSlide,
                    conversationDetailsProvider,
                    historyProvider,
                    cachedHistoryProvider,
                    metlServerAddress,
                    cache,
                    receiveEvents,
                    webClientFactory,
                    resourceProvider,
                    auditor
                    );
                foreach (var parser in new[] { otherParser, this })
                {
                    a(GaugeStatus.InProgress, 10);
                    foreach (var attendance in parser.attendances.ToList())
                        returnParser.actOnAttendance(new MeTLStanzas.Attendance(attendance));
                    foreach (var moveDelta in parser.moveDeltas)
                        returnParser.actOnMoveDelta(new MeTLStanzas.MoveDeltaStanza(moveDelta));
                    //returnParser.moveDeltas.Add(moveDelta);
                    a(GaugeStatus.InProgress, 20);
                    foreach (var i in parser.dirtyImage.ToList())
                        returnParser.actOnDirtyImageReceived(i);
                    a(GaugeStatus.InProgress, 30);
                    foreach (var i in parser.dirtyText.ToList())
                        returnParser.actOnDirtyTextReceived(i);
                    a(GaugeStatus.InProgress, 40);
                    foreach (var i in parser.dirtyInk.ToList())
                        returnParser.actOnDirtyStrokeReceived(i);
                    a(GaugeStatus.InProgress, 50);
                    foreach (var i in parser.ink.ToList())
                        returnParser.actOnStrokeReceived(i);
                    //returnParser.ink.AddRange(parser.ink.Where(s => !returnParser.ink.Contains(s)));
                    a(GaugeStatus.InProgress, 60);
                    returnParser.quizzes.AddRange(parser.quizzes.ToList());
                    a(GaugeStatus.InProgress, 70);
                    returnParser.quizAnswers.AddRange(parser.quizAnswers.ToList());
                    //returnParser.dirtyImage.AddRange(parser.dirtyImage);
                    //returnParser.dirtyInk.AddRange(parser.dirtyInk);
                    //returnParser.dirtyText.AddRange(parser.dirtyText);
                    a(GaugeStatus.InProgress, 80);
                    foreach (var kv in parser.text.ToList())
                        returnParser.actOnTextReceived(kv.Value);
                    /*if (!returnParser.text.ContainsKey(kv.Key))
                        returnParser.text.Add(kv.Key, kv.Value);*/
                    a(GaugeStatus.InProgress, 90);
                    foreach (var kv in parser.images.ToList())
                        returnParser.actOnImageReceived(kv.Value);
                    /*if(!returnParser.images.ContainsKey(kv.Key))
                        returnParser.images.Add(kv.Key, kv.Value);*/

                    a(GaugeStatus.InProgress, 95);
                    foreach (var kv in parser.liveWindows.ToList())
                        if (!returnParser.liveWindows.ContainsKey(kv.Key))
                            returnParser.liveWindows.Add(kv.Key, kv.Value);
                }
                return returnParser;

            }), "mergeParser", "preParser");
        }
        public void Regurgitate()
        {
            receiveEvents.receiveStrokes(ink.ToArray());
            if (images.Values.Count > 0)
                receiveEvents.receiveImages(images.Values.ToArray());
            foreach (var box in text.Values)
                receiveEvents.receiveTextBox(box);
            foreach (var moveDelta in moveDeltas)
                receiveEvents.receiveMoveDelta(moveDelta);
            foreach (var dirty in dirtyInk)
                receiveEvents.receiveDirtyStroke(dirty.element);
            foreach (var dirty in dirtyImage)
                receiveEvents.receiveDirtyImage(dirty.element);
            foreach (var dirty in dirtyText)
                receiveEvents.receiveDirtyTextBox(dirty.element);
            foreach (var quiz in quizzes)
                receiveEvents.receiveQuiz(quiz);
            foreach (var answer in quizAnswers)
                receiveEvents.receiveQuizAnswer(answer);
            foreach (var window in liveWindows.Values)
                receiveEvents.receiveLiveWindow(window);
            foreach (var file in files)
                receiveEvents.receiveFileResource(file);
            foreach (var attendance in attendances)
                receiveEvents.attendanceReceived(attendance);
            receiveEvents.allContentSent(location.currentSlide);
            Trace.TraceInformation(string.Format("{1} regurgitate finished {0}", DateTimeFactory.Now(), this.location.currentSlide));
        }
        public override void actOnStatusRecieved(MeTLStanzas.TeacherStatusStanza status)
        {
            return; //do nothing
        }
        public override void actOnFileResource(MeTLStanzas.FileResource resource)
        {
            files.Add(resource.fileResource);
        }
        public override void ReceiveCommand(Element el)
        {//Preparsers don't care about commands, they're not a valid part of history.
            return;
        }
        public override void actOnAttendance(MeTLStanzas.Attendance attendance)
        {
            attendances.Add(attendance.attendance);
        }
        public override void actOnMoveDelta(MeTLStanzas.MoveDeltaStanza moveDelta)
        {
            var mdp = moveDelta.parameters;
            var inksToRemove = new List<TargettedStroke>();
            var inksToAdd = new List<TargettedStroke>();
            var textToRemove = new Dictionary<string, TargettedTextBox>();
            var textToAdd = new Dictionary<string, TargettedTextBox>();
            var imagesToRemove = new Dictionary<string, TargettedImage>();
            var imagesToAdd = new Dictionary<string, TargettedImage>();

            var relevantStrokes = ink.Where(i => dirtiesThis(moveDelta, i));
            var relevantImages = images.Values.Where(i => dirtiesThis(moveDelta, i));
            var relevantTexts = text.Values.Where(i => dirtiesThis(moveDelta, i));

            double top = 0.0;
            double left = 0.0;
            bool firstItem = true;
            if (Double.IsNaN(mdp.xOrigin) || Double.IsNaN(mdp.yOrigin))
            {
                Func<double, double, bool> updateRect = (l, t) =>
                {
                    bool changed = false;
                    if (firstItem)
                    {
                        top = t;
                        left = l;
                        firstItem = false;
                        changed = true;
                    }
                    else
                    {
                        if (t < top)
                        {
                            top = t;
                            changed = true;
                        }
                        if (l < left)
                        {
                            left = l;
                            changed = true;
                        }
                    }
                    return changed;
                };

                foreach (var s in relevantStrokes)
                {
                    var strokeBounds = s.stroke.GetBounds();
                    updateRect(strokeBounds.Left, strokeBounds.Top);
                }
                foreach (var t in relevantTexts)
                {
                    var tSpec = t.boxSpecification;
                    updateRect(tSpec.x, tSpec.y);
                }
                foreach (var i in relevantImages)
                {
                    var iSpec = i.imageSpecification;
                    updateRect(iSpec.x, iSpec.y);
                }
            }
            else
            {
                left = mdp.xOrigin;
                top = mdp.yOrigin;
            }
            foreach (var aInk in relevantStrokes)
            {
                if (mdp.privacy == aInk.privacy && mdp.timestamp > aInk.timestamp)
                {
                    inksToRemove.Add(aInk);
                    if (!mdp.isDeleted)
                    {
                        var sBounds = aInk.stroke.GetBounds();
                        var internalX = sBounds.Left - left;
                        var internalY = sBounds.Top - top;
                        var offsetX = -(internalX - (internalX * mdp.xScale));
                        var offsetY = -(internalY - (internalY * mdp.yScale));
                        inksToAdd.Add(aInk.AdjustVisual(mdp.xTranslate + offsetX, mdp.yTranslate + offsetY, mdp.xScale, mdp.yScale).AlterPrivacy(mdp.newPrivacy));
                    }
                }
            }
            foreach (var aText in relevantTexts)
            {
                if (mdp.privacy == aText.privacy && mdp.timestamp > aText.timestamp)
                {
                    textToRemove.Add(aText.identity, aText);
                    if (!mdp.isDeleted)
                    {
                        var tSpec = aText.boxSpecification;
                        var internalX = tSpec.x - left;
                        var internalY = tSpec.y - top;
                        var offsetX = -(internalX - (internalX * mdp.xScale));
                        var offsetY = -(internalY - (internalY * mdp.yScale));
                        var targettedText = aText.AdjustVisual(mdp.xTranslate + offsetX, mdp.yTranslate + offsetY, mdp.xScale, mdp.yScale).AlterPrivacy(mdp.newPrivacy);
                        textToAdd.Add(targettedText.identity, targettedText);
                    }
                }
            }
            foreach (var aImage in relevantImages)
            {
                if (mdp.privacy == aImage.privacy && mdp.timestamp > aImage.timestamp)
                {
                    imagesToRemove.Add(aImage.identity, aImage);
                    if (!mdp.isDeleted)
                    {
                        var iSpec = aImage.imageSpecification;
                        var internalX = iSpec.x - left;
                        var internalY = iSpec.y - top;
                        var offsetX = -(internalX - (internalX * mdp.xScale));
                        var offsetY = -(internalY - (internalY * mdp.yScale));
                        var tImage = aImage.AdjustVisual(mdp.xTranslate + offsetX, mdp.yTranslate + offsetY, mdp.xScale, mdp.yScale).AlterPrivacy(mdp.newPrivacy);
                        imagesToAdd.Add(tImage.identity, tImage);
                    }
                }
            }
            foreach (var i in inksToRemove)
                ink.Remove(i);
            foreach (var i in inksToAdd)
                ink.Add(i);
            foreach (var i in textToRemove)
                text.Remove(i.Key);
            foreach (var i in textToAdd)
                text.Add(i.Key, i.Value);
            foreach (var i in imagesToRemove)
                images.Remove(i.Key);
            foreach (var i in imagesToAdd)
                images.Add(i.Key, i.Value);
            // preparsers need to apply the move deltas in timestamp order
            moveDeltas.Add(moveDelta.parameters);
        }

        public override void actOnScreenshotSubmission(TargettedSubmission submission)
        {
            submissions.Add(submission);
        }
        public override void actOnDirtyImageReceived(MeTLStanzas.DirtyImage image)
        {
            if (images.ContainsKey(image.element.identity))
            {
                var possiblyRemovedImage = images[image.element.identity];
                if ((possiblyRemovedImage.privacy == image.element.privacy) && (possiblyRemovedImage.timestamp <= image.element.timestamp))
                {
                    images.Remove(image.element.identity);
                }
            }
        }
        public override void actOnDirtyTextReceived(MeTLStanzas.DirtyText element)
        {
            if (text.ContainsKey(element.element.identity))
            {
                var possibleRemovedText = text[element.element.identity];
                if ((possibleRemovedText.privacy == element.element.privacy) && (possibleRemovedText.timestamp <= element.element.timestamp))
                    text.Remove(element.element.identity);
            }
        }
        public override void actOnDirtyStrokeReceived(MeTLStanzas.DirtyInk dirtyInk)
        {
            var strokesToRemove = ink.Where(s => s.HasSameIdentity(dirtyInk.element.identity)).ToList();
            foreach (var stroke in strokesToRemove)
            {
                if (dirtiesThis(dirtyInk, stroke))
                    ink.Remove(stroke);
            }
        }
        public override void actOnImageReceived(TargettedImage image)
        {
            if (!dirtyImage.Any(di => dirtiesThis(di, image)) && !moveDeltas.Where(md => md.isDeleted && md.privacy == image.privacy && md.timestamp > image.timestamp).Any(md => dirtiesThis(md, image)))
            {
                var newImage = moveDeltas.Where(md => dirtiesThis(md, image)).OrderBy(md => md.timestamp).Aggregate(image, (tempImage, md) =>
                {
                    //return tempImage.AdjustVisual(md.xTranslate, md.yTranslate, md.xScale, md.yScale).AlterPrivacy(md.newPrivacy);
                    return tempImage.AlterPrivacy(md.newPrivacy);
                });
                images[image.identity] = image;
            }
            /*if (!dirtyImage.Any(di => dirtiesThis(di, image)) && !moveDeltas.Any(md => dirtiesThis(md, image)))
                images[image.identity] = image;*/
        }

        private bool dirtiesThis(TargettedMoveDelta moveDelta, TargettedElement elem)
        {
            if (elem is TargettedImage)
            {
                return moveDelta.imageIds.Any(i =>
                {
                    return elem.identity == i.Identity && elem.privacy == moveDelta.privacy && elem.timestamp < moveDelta.timestamp;
                });
            }
            else if (elem is TargettedStroke)
            {
                return moveDelta.inkIds.Any(i =>
                {
                    return elem.identity == i.Identity && elem.privacy == moveDelta.privacy && elem.timestamp < moveDelta.timestamp;
                });
            }
            else if (elem is TargettedTextBox)
            {
                return moveDelta.textIds.Any(i =>
                {
                    return elem.identity == i.Identity && elem.privacy == moveDelta.privacy && elem.timestamp < moveDelta.timestamp;
                });
            }
            else return false;
        }

        private bool dirtiesThis(MeTLStanzas.MoveDeltaStanza moveDelta, TargettedElement elem)
        {
            return dirtiesThis(moveDelta.parameters, elem);
        }

        private bool dirtiesThis(MeTLStanzas.DirtyElement dirty, TargettedElement elem)
        {
            return elem.identity == dirty.element.identity && elem.privacy == dirty.element.privacy && elem.timestamp < dirty.element.timestamp;
        }

        private bool dirtiesThis(TargettedDirtyElement dirty, TargettedElement elem)
        {
            return elem.identity == dirty.identity && elem.privacy == dirty.privacy && elem.timestamp < dirty.timestamp;
        }

        public override void actOnStrokeReceived(TargettedStroke stroke)
        {
            if (!dirtyInk.Any(di => dirtiesThis(di, stroke)) && !(moveDeltas.Where(md => md.isDeleted && md.privacy == stroke.privacy && md.timestamp > stroke.timestamp).Any(md => dirtiesThis(md, stroke))))
            {
                var newStroke = moveDeltas.Where(md => dirtiesThis(md, stroke)).OrderBy(md => md.timestamp).Aggregate(stroke, (tempStroke, md) =>
                {
                    //return tempStroke.AdjustVisual(md.xTranslate, md.yTranslate, md.xScale, md.yScale).AlterPrivacy(md.newPrivacy);                    
                    return tempStroke.AlterPrivacy(md.newPrivacy);
                });
                ink.Add(newStroke);
            }
        }
        public override void actOnQuizReceived(QuizQuestion details)
        {
            quizzes.Add(details);
        }
        public override void actOnQuizAnswerReceived(QuizAnswer quizAnswer)
        {
            quizAnswers.Add(quizAnswer);
        }
        public override void actOnTextReceived(TargettedTextBox box)
        {
            try
            {

                if (!dirtyText.Any(di => dirtiesThis(di, box)) && !moveDeltas.Where(md => md.isDeleted && md.privacy == box.privacy && md.timestamp > box.timestamp).Any(md => dirtiesThis(md, box)))
                {
                    var newTextBox = moveDeltas.Where(md => dirtiesThis(md, box)).OrderBy(md => md.timestamp).Aggregate(box, (tempBox, md) =>
                    {
                        //return tempBox.AdjustVisual(md.xTranslate, md.yTranslate, md.xScale, md.yScale).AlterPrivacy(md.newPrivacy);
                        return tempBox.AlterPrivacy(md.newPrivacy);
                    });
                    text[box.identity] = box;
                }
                /*if (!dirtyText.Any(di => dirtiesThis(di, box)) && !moveDeltas.Any(md => dirtiesThis(md, box)))                   
                    text[box.identity] = box;*/
            }
            catch (NullReferenceException)
            {
                Trace.TraceError("Null reference in collecting text from preparser");
            }
        }
        public override void actOnLiveWindowReceived(LiveWindowSetup window)
        {
            liveWindows[window.snapshotAtTimeOfCreation] = window;
        }
        public override void actOnDirtyLiveWindowReceived(TargettedDirtyElement element)
        {
            liveWindows.Remove(element.identity);
        }
        public static int ParentRoom(string room)
        {
            return Int32.Parse(room.Split('/').Last());
            /*
            var regex = new Regex(@"(\d+).*");
            var parent = regex.Matches(room)[0].Groups[1].Value;
            return Int32.Parse(parent);
             */
        }
    }
}