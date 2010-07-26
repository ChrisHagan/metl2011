using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;

namespace SandRibbon.Utils.Connection
{
    public class PreParser : JabberWire
    {
        public Dictionary<string, TargettedImage> images = new Dictionary<string, TargettedImage>();
        public Dictionary<string, TargettedVideo> videos = new Dictionary<string, TargettedVideo>();
        public Dictionary<string, TargettedAutoShape> autoshapes = new Dictionary<string, TargettedAutoShape>();
        public List<TargettedStroke> ink = new List<TargettedStroke>();
        public List<QuizQuestion> quizzes = new List<QuizQuestion>();
        public List<TargettedSubmission> submissions = new List<TargettedSubmission>();
        public List<QuizAnswer> quizAnswers = new List<QuizAnswer>();
        public List<TargettedBubbleContext> bubbleList = new List<TargettedBubbleContext>();
        public Dictionary<string, TargettedTextBox> text = new Dictionary<string, TargettedTextBox>();
        public Dictionary<string, LiveWindowSetup> liveWindows = new Dictionary<string, LiveWindowSetup>();
        public PreParser(int slide):base()
        {
            if (this.location == null)
                this.location = new Location();
            this.location.currentSlide = slide;
        }
        public InkCanvas ToVisual()
        {
            var canvas = new InkCanvas();
            foreach (var image in images)
                canvas.Children.Add(image.Value.image);
            foreach (var shape in autoshapes)
                canvas.Children.Add(shape.Value.autoshape);
            //Videos currently disabled.
            foreach (var video in videos)
                canvas.Children.Add(video.Value.video);
            foreach (var textbox in text)
                canvas.Children.Add(textbox.Value.box);
            foreach (var stroke in ink)
                canvas.Strokes.Add(stroke.stroke);
            return canvas;
        }
        public T merge<T>(T otherParser) where T : PreParser
        {
            var returnParser = (T)Activator.CreateInstance(typeof(T), location.currentSlide);
            foreach (var parser in new[] { otherParser, this})
            {
                returnParser.ink.AddRange(parser.ink.Where(s => !returnParser.ink.Contains(s)));
                returnParser.quizzes.AddRange(parser.quizzes);
                returnParser.quizAnswers.AddRange(parser.quizAnswers);
                foreach (var kv in parser.text)
                    if (!returnParser.text.ContainsKey(kv.Key))
                        returnParser.text.Add(kv.Key, kv.Value);
                foreach (var kv in parser.images)
                    if(!returnParser.images.ContainsKey(kv.Key))
                        returnParser.images.Add(kv.Key, kv.Value);
                foreach (var kv in parser.autoshapes)
                    if(!returnParser.autoshapes.ContainsKey(kv.Key))
                        returnParser.autoshapes.Add(kv.Key, kv.Value);
                foreach (var kv in parser.liveWindows)
                    if (!returnParser.liveWindows.ContainsKey(kv.Key))
                        returnParser.liveWindows.Add(kv.Key, kv.Value);
                //Videos currently disabled.
                foreach (var kv in parser.videos)
                    if (!returnParser.videos.ContainsKey(kv.Key))
                        returnParser.videos.Add(kv.Key, kv.Value);
            }
            return returnParser;
        }
        public void Regurgitate()
        {
            Commands.ReceiveStrokes.Execute(ink);
            foreach (var autoshape in autoshapes.Values)
                Commands.ReceiveAutoShape.Execute(autoshape);
            if(images.Values.Count > 0)
                Commands.ReceiveImage.Execute(images.Values);
            foreach (var box in text.Values)
                Commands.ReceiveTextBox.Execute(box);
            foreach(var quiz in quizzes)
                Commands.ReceiveQuiz.Execute(quiz);
            foreach(var answer in quizAnswers)
                Commands.ReceiveQuizAnswer.Execute(answer);
            foreach (var window in liveWindows.Values)
                Commands.ReceiveLiveWindow.Execute(window);
            //Videos currently disabled.
            foreach (var video in videos.Values)
                Commands.ReceiveVideo.Execute(video);
            Commands.AllContentSent.Execute(location.currentSlide);
            Logger.Log(string.Format("{1} regurgitate finished {0}", DateTimeFactory.Now(), this.location.currentSlide));
        }
        public override void ReceiveCommand(string message)
        {//Preparsers don't care about commands, they're not a valid part of history.
            return;
        }
        public override void actOnScreenshotSubmission(TargettedSubmission submission)
        {
            submissions.Add(submission);
        }
        public override void actOnDirtyImageReceived(SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyImage image)
        {
            if(images.ContainsKey(image.element.identifier))
                images.Remove(image.element.identifier);
        }
        public override void actOnDirtyAutoshapeReceived(MeTLStanzas.DirtyAutoshape element)
        {
            if (autoshapes.ContainsKey(element.element.identifier))
                autoshapes.Remove(element.element.identifier);
        }
        public override void actOnDirtyTextReceived(MeTLStanzas.DirtyText element)
        {
            if(text.ContainsKey(element.element.identifier))
                text.Remove(element.element.identifier);
        }
        public override void actOnDirtyVideoReceived(MeTLStanzas.DirtyVideo element)
        {
            if (videos.ContainsKey(element.element.identifier))
                videos.Remove(element.element.identifier);
        }
        public override void actOnDirtyStrokeReceived(MeTLStanzas.DirtyInk dirtyInk)
        {
            var strokesToRemove = ink.Where(s => 
                s.stroke.sum().checksum.ToString().Equals(dirtyInk.element.identifier)).ToList();
            foreach(var stroke in strokesToRemove)
                ink.Remove(stroke);
        }
        public override void actOnImageReceived(TargettedImage image)
        {
            images[image.id] = image;
        }
        public override void actOnAutoShapeReceived(TargettedAutoShape autoshape)
        {
            return;
            try
            {
                autoshapes[(string)autoshape.autoshape.Tag] = autoshape;
            }
            catch (NullReferenceException)
            {
                Logger.Log("Null reference in collecting autoshape from preparser");
            }
        }
        public override void actOnStrokeReceived(TargettedStroke stroke)
        {
            ink.Add(stroke);
        }
        public override void actOnQuizReceived(QuizQuestion quizDetails)
        {
            quizzes.Add(quizDetails);
        }
        public override void actOnQuizAnswerReceived(QuizAnswer answer)
        {
            quizAnswers.Add(answer);
        }
        public override void actOnTextReceived(TargettedTextBox box)
        {
            try
            {
                text[box.identity] = box;
            }
            catch (NullReferenceException)
            {
                Logger.Log("Null reference in collecting text from preparser");
            }
        }
        public override void actOnLiveWindowReceived(LiveWindowSetup window)
        {
            liveWindows[window.snapshotAtTimeOfCreation] = window;
        }
        public override void actOnDirtyLiveWindowReceived(TargettedDirtyElement element)
        {
            liveWindows.Remove(element.identifier);
        }
        public override void actOnVideoReceived(TargettedVideo video)
        {
            //Videos currently disabled
            videos[video.id]=video;
        }
        public override void actOnBubbleReceived(TargettedBubbleContext bubble)
        {
            bubbleList.Add(bubble);
        }
        public static int ParentRoom(string room)
        {
            var regex = new Regex(@"(\d+).*");
            var parent = regex.Matches(room)[0].Groups[1].Value;
            return Int32.Parse(parent);
        }
    }
}