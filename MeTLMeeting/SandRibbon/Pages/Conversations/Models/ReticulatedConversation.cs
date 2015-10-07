using MeTLLib.DataTypes;
using SandRibbon.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SandRibbon.Pages.Conversations.Models
{
    public class VmSlide : DependencyObject
    {
        public Slide Slide { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }


        public int Activity
        {
            get { return (int)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        public static readonly DependencyProperty ActivityProperty =
            DependencyProperty.Register("Activity", typeof(int), typeof(VmSlide), new PropertyMetadata(0));


        public int Voices
        {
            get { return (int)GetValue(VoicesProperty); }
            set { SetValue(VoicesProperty, value); }
        }

        public static readonly DependencyProperty VoicesProperty =
            DependencyProperty.Register("Voices", typeof(int), typeof(VmSlide), new PropertyMetadata(0));


    }
    public class ReticulatedConversation
    {
        public ConversationDetails PresentationPath { get; set; }
        public ConversationDetails AdvancedMaterial { get; set; }
        public ConversationDetails RemedialMaterial { get; set; }
        public List<ConversationDetails> RelatedMaterial { get; set; }
        public List<LearningObjective> Objectives { get; set; }
        private List<ConversationDetails> cds()
        {
            return new[]
                {
                PresentationPath,
                    AdvancedMaterial,
                    RemedialMaterial
                }.Concat(RelatedMaterial?? new List<ConversationDetails>()).ToList();
        }

        internal void CalculateLocations()
        {
            var relevantDetails = cds();
            var locs = new List<VmSlide>();
            for (int row = 0; row < relevantDetails.Count(); row++)
            {
                relevantDetails[row]?.Slides.ForEach(s => locs.Add(new VmSlide { Slide = s, Row = row + 1, Column = s.index }));
            }
            //TODO: Fill assessments            
            Locations.Clear();
            foreach (var loc in locs)
            {
                Locations.Add(loc);
            }
        }
        public ObservableCollection<VmSlide> Locations { get; set; } = new ObservableCollection<VmSlide>();
        public int LongestPathLength => cds().Where(cd => cd != null).Select(d => d.Slides.Count()).Max();
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