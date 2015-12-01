using System;
using System.Windows;
using System.Windows.Data;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Collaboration;

namespace SandRibbon.Components
{
    public partial class ContentVisibility
    {
        public ObservableCollection<ContentVisibilityDefinition> visibilities = new ObservableCollection<ContentVisibilityDefinition>();
        public RibbonCollaborationPage rootPage { get; protected set; }
        public ContentVisibility()
        {
            InitializeComponent();
            contentVisibilitySelectors.ItemsSource = visibilities;
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdateConversationDetails);
            var updateContentVisibilityCommand = new DelegateCommand<List<ContentVisibilityDefinition>>((_unused) => potentiallyRefresh());
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as RibbonCollaborationPage;
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.UpdateContentVisibility.RegisterCommandToDispatcher(updateContentVisibilityCommand); 
            };
            Unloaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.UpdateContentVisibility.UnregisterCommand(updateContentVisibilityCommand);
            };
            Commands.SetContentVisibility.DefaultValue = ContentFilterVisibility.defaultVisibilities;
            DataContext = this;
        }

        protected int slide = -1;
        protected ConversationDetails conversation = ConversationDetails.Empty;
        protected List<GroupSet> groupSets = new List<GroupSet>();
        protected void UpdateConversationDetails(ConversationDetails cd)
        {
            conversation = cd;
            potentiallyRefresh();
        }

        protected void potentiallyRefresh()
        {
            var thisSlide = conversation.Slides.Find(s => s.id == slide);
            if (thisSlide != default(Slide) && thisSlide.type == Slide.TYPE.GROUPSLIDE)
            {
                var oldGroupSets = groupSets;
                var currentState = new Dictionary<string, bool>();
                foreach (var vis in visibilities)
                {
                    if (vis.GroupId != "")
                    {
                        currentState.Add(vis.GroupId, vis.Subscribed);
                    }
                }
                var newSlide = conversation.Slides.Find(s => s.id == slide);
                if (newSlide != null)
                {
                    groupSets = newSlide.GroupSets;
                    var newGroupDefs = new List<ContentVisibilityDefinition>();
                    groupSets.ForEach(gs =>
                    {
                        var oldGroupSet = oldGroupSets.Find(oldGroup => oldGroup.id == gs.id);
                        gs.Groups.ForEach(g =>
                        {
                            var oldGroup = oldGroupSet.Groups.Find(ogr => ogr.id == g.id);
                            var wasSubscribed = currentState[g.id];
                            if (rootPage.details.isAuthor(rootPage.networkController.credentials.name) || g.GroupMembers.Contains(rootPage.networkController.credentials.name))
                            {
                                var groupDescription = rootPage.details.isAuthor(rootPage.getNetworkController().credentials.name) ? String.Format("Group {0}: {1}", g.id, g.GroupMembers.Aggregate("", (acc, item) => acc + " " + item)) : String.Format("Group {0}", g.id);
                                newGroupDefs.Add(
                                    new ContentVisibilityDefinition("Group " + g.id, groupDescription, g.id, wasSubscribed, (sap,a, p, c, s) => g.GroupMembers.Contains(a))
                                );
                            }
                        });
                    });
                    Dispatcher.adopt(delegate
                    {
                        visibilities.Clear();
                        foreach (var nv in newGroupDefs.Concat(ContentFilterVisibility.defaultGroupVisibilities))
                        {
                            visibilities.Add(nv);
                        }
                    });
                }
            }
            else
            {
                Dispatcher.adopt(delegate
                {
                    visibilities.Clear();
                    foreach (var nv in ContentFilterVisibility.defaultVisibilities)
                    {
                        visibilities.Add(nv);
                    }
                }); 

            }

        }
        private void OnVisibilityChanged(object sender, DataTransferEventArgs args)
        {
            Commands.SetContentVisibility.Execute(visibilities);
        }
    }
}