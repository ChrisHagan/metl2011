using MeTLLib.DataTypes;
using SandRibbon.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SandRibbon.Pages.Conversations.Models
{
    public class VmSlide
    {
        public Slide Slide { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
    public class ReticulatedConversation
    {
        public ConversationDetails PresentationPath { get; set; }
        public ConversationDetails AdvancedMaterial { get; set; }
        public ConversationDetails RemedialMaterial { get; set; }
        public List<LearningObjective> Objectives { get; set; }
        private List<ConversationDetails> cds()
        {
            return new[]
                {
                PresentationPath,
                    AdvancedMaterial,
                    RemedialMaterial
                }.ToList();
        }
        public int LongestPathLength => cds().Where(cd => cd != null).Select(d => d.Slides.Count()).Max();         
        public List<VmSlide> Locations
        {
            get
            {
                var relevantDetails = cds();
                var locs = new List<VmSlide>();
                for (int row = 0; row < relevantDetails.Count(); row++)
                {
                    relevantDetails[row]?.Slides.ForEach(s => locs.Add(new VmSlide { Slide = s, Row = row + 1, Column = s.index }));                    
                }
                //TODO: Fill assessments            
                return locs;
            }
        }
    }

    public class ReticulatedNode
    {
        public List<ReticulatedNode> Outputs { get; set; }
        public SlideRestriction Restriction { get; set; }
        public string Narration { get; set; }
        public Slide Slide { get; set; }

        public ReticulatedNode()
        {
            Outputs = new List<ReticulatedNode>();
        }
    }

    public class SlideRestriction
    {
        public static SlideRestriction Unrestricted = new SlideRestriction { predicate = p => true };
        public Func<MeTLUser, bool> predicate { get; set; }
    }
}