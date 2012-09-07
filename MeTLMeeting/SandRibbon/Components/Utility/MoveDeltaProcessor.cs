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
            Canvas = canvas;
            Target = target; 
        }

        abstract public void ReceiveMoveDelta(TargettedMoveDelta moveDelta, string recipient, bool processHistory);
        abstract protected void ContentDelete(TargettedMoveDelta moveDelta);
        abstract protected void ContentPrivacyChange(TargettedMoveDelta moveDelta);
    }
}
