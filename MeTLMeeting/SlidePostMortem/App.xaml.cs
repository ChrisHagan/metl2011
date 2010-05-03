using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SlidePostMortem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //This is to ensure that all the static constructors are called.
            base.OnStartup(e);
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.Ink();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.Quiz();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.Image();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.Answer();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.TextBox();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyInk();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyText();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.AutoShape();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyImage();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.QuizStatus();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.LiveWindow();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyElement();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyAutoshape();
            new SandRibbonInterop.MeTLStanzas.MeTLStanzas.DirtyLiveWindow();
        }
    }
}
