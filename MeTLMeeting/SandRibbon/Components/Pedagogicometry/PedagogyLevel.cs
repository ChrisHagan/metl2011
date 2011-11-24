namespace SandRibbon.Components.Pedagogicometry
{
    // NB: Do not change the order of the enum entries, only add new codes at the bottom
    public enum PedagogyCode
    {
            Whiteboard = 0,
            SurveyRespondent,
            ResponsivePresentation,
            CollaborativePresentation,
            CrowdsourcedConversation
    }

    public class PedagogyLevel
    {
        public PedagogyCode code;
        public string label { get; set; }
    }
}