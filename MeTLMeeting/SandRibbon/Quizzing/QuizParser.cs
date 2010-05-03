using SandRibbon.Utils.Connection;
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public class QuizParser : PreParser
    {
        public QuizParser(int location)
            : base(location)
        {
        }
        public override void actOnQuizAnswerReceived(SandRibbonInterop.QuizAnswer answer)
        {
            base.actOnQuizAnswerReceived(answer);
        }
        public override void actOnQuizStatus(QuizStatusDetails status)
        {
            base.actOnQuizStatus(status);
        }
        public override void actOnQuizReceived(SandRibbonInterop.QuizDetails quizDetails)
        {
            base.actOnQuizReceived(quizDetails);
        }
        /*All noop overrides below this line*/
        public override void actOnAutoShapeReceived(SandRibbonInterop.MeTLStanzas.TargettedAutoShape autoshape)
        {
        }
        public override void actOnDirtyAutoshapeReceived(SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyAutoshape element)
        {
        }
        public override void actOnDirtyImageReceived(SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyImage image)
        {
        }
        public override void actOnDirtyLiveWindowReceived(SandRibbonInterop.MeTLStanzas.TargettedDirtyElement element)
        {
        }
        public override void actOnDirtyStrokeReceived(SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyInk dirtyInk)
        {
        }
        public override void actOnDirtyTextReceived(SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyText element)
        {
        }
        public override void actOnImageReceived(SandRibbonInterop.MeTLStanzas.TargettedImage image)
        {
        }
        public override void actOnLiveWindowReceived(SandRibbonObjects.LiveWindowSetup window)
        {
        }
        public override void actOnStrokeReceived(SandRibbonInterop.MeTLStanzas.TargettedStroke stroke)
        {
        }
        public override void actOnTextReceived(SandRibbonInterop.MeTLStanzas.TargettedTextBox box)
        {
        }
    }
}
