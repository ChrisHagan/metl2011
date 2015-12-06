using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Newtonsoft.Json.Linq;
using SandRibbon.Components;
using SandRibbon.Pages.Collaboration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace SandRibbon.Pages.Conversations.Models
{
    public class Participation : DependencyObject
    {
        public ObservableCollection<LocatedActivity> Activity { get; set; } = new ObservableCollection<LocatedActivity>();        
        public ILookup<string, LocatedActivity> Participants { get; internal set; }
    }
    public class VmSlide : DependencyObject
    {
        public ConversationRelevance Relevance { get; set; }
        public Slide Slide { get; set; }
        public ConversationDetails Details { get; set; }


        public LocatedActivity Aggregate
        {
            get { return (LocatedActivity)GetValue(AggregateProperty); }
            set { SetValue(AggregateProperty, value); }
        }        
        public static readonly DependencyProperty AggregateProperty =
            DependencyProperty.Register("Aggregate", typeof(LocatedActivity), typeof(VmSlide), new PropertyMetadata(null));

        public Participation Participation
        {
            get { return (Participation)GetValue(ParticipationProperty); }
            set { SetValue(ParticipationProperty, value); }
        }
        public static readonly DependencyProperty ParticipationProperty =
            DependencyProperty.Register("Participation", typeof(Participation), typeof(VmSlide), new PropertyMetadata(null));
    }
    public enum ConversationRelevance
    {
        PRESENTATION_PATH,
        ADVANCED_MATERIAL,
        REMEDIAL_MATERIAL,
        RELATED_MATERIAL
    }
    public class ReticulatedConversation : DependencyObject
    {
        public NetworkController networkController { get; set; }
        public ConversationDetails PresentationPath { get; set; }
        public ConversationDetails AdvancedMaterial { get; set; }
        public ConversationDetails RemedialMaterial { get; set; }
        public List<ConversationDetails> RelatedMaterial { get; set; } = new List<ConversationDetails>();
        public List<LearningObjective> Objectives { get; set; } = new List<LearningObjective>();
        private List<ConversationDetails> cds()
        {
            return new[]
                {
                PresentationPath,
                    AdvancedMaterial,
                    RemedialMaterial
                }.Concat(RelatedMaterial ?? new List<ConversationDetails>()).ToList();
        }
        public ObservableCollection<VmSlide> Locations { get; set; } = new ObservableCollection<VmSlide>();


        public ObservableCollection<LocatedActivity> Participation
        {
            get { return (ObservableCollection<LocatedActivity>)GetValue(ParticipationProperty); }
            set { SetValue(ParticipationProperty, value); }
        }
        public static readonly DependencyProperty ParticipationProperty =
            DependencyProperty.Register("Participation", typeof(ObservableCollection<LocatedActivity>), typeof(ReticulatedConversation), new PropertyMetadata(new ObservableCollection<LocatedActivity>()));


        public int LongestPathLength => cds().Where(cd => cd != null).Select(d => d.Slides.Count()).Max();
        public int PathCount => cds().Count();

        public delegate void LocationAnalysis();
        public event LocationAnalysis LocationAnalyzed;

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