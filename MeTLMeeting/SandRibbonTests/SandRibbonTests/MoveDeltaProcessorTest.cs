// -----------------------------------------------------------------------
// <copyright file="MoveDeltaProcessorTests.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SandRibbonTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using SandRibbon.Components.Utility;

    using NUnit.Framework;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class MoveDeltaProcessorTest
    {
        [Test]
        public void ReceiveMoveDelta()
        {
            var canvas = new InkCanvas();
            var contentBuffer = new ContentBuffer();

            var moveDeltaProcessor = new StackMoveDeltaProcessor(canvas, contentBuffer, "presentationSpace");
        }
    }
}
