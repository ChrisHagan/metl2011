namespace SandRibbon.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MeTLLib.DataTypes;
    using System.Xml.Linq;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConversationDescriptor
    {
        private readonly ConversationDetails convDetails;
        private readonly XElement xml;

        public ConversationDetails Details
        {
            get
            {
                return convDetails;
            }
        }

        public XElement Xml
        {
            get
            {
                return xml;
            }
        }

        public bool HasPrivateContent { get; set; }

        public ConversationDescriptor(ConversationDetails conversationDetails, XElement xmlRoot)
        {
            convDetails = conversationDetails;
            xml = xmlRoot;
        }
    }
}
