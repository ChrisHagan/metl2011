using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP.Xml.Dom;
using System.Windows.Forms;

namespace PowerpointJabber
{
    public class Wire
    {
        public string SERVER = "madam.adm.monash.edu.au";
        protected XmppClientConnection conn;
        public Wire()
        {
            conn = new XmppClientConnection(SERVER);
        }
        public void Connect(string username, string password)
        {
            AttachHandlers(conn);
            conn.Open(username, password);
        }
        public void Disconnect()
        {
            DetachHandlers(conn);
            conn.Close();
        }
        private void DetachHandlers(XmppClientConnection thisconn)
        {
            thisconn.OnError -= conn_OnError;
            thisconn.OnReadXml -= conn_OnReadXML;
            thisconn.OnWriteXml -= conn_OnWriteXML;
        }
        private void AttachHandlers(XmppClientConnection thisconn)
        {
            thisconn.OnError += conn_OnError;
            thisconn.OnReadXml += conn_OnReadXML;
            thisconn.OnWriteXml += conn_OnWriteXML;
        }
        private void conn_OnError(object sender, Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
        private void conn_OnWriteXML(object sender, string xml)
        {
            MessageBox.Show("OUT: " + xml);
        }
        private void conn_OnReadXML(object sender, string xml)
        {
            MessageBox.Show("IN: " + xml);
        }
    }
}
