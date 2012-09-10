namespace SandRibbon.Components.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MeTLLib.DataTypes;
    using System.Windows.Controls;

    public abstract class MoveDeltaProcessor
    {
        public InkCanvas Canvas { get; private set; }

        public string Target { get; private set; }

        protected MoveDeltaProcessor(InkCanvas canvas, string target)
        {
            this.Canvas = canvas;
            this.Target = target; 
        }

        public abstract void ReceiveMoveDelta(TargettedMoveDelta moveDelta, string recipient, bool processHistory);
        protected abstract void ContentDelete(TargettedMoveDelta moveDelta);
        protected abstract void ContentPrivacyChange(TargettedMoveDelta moveDelta);
    }
}
