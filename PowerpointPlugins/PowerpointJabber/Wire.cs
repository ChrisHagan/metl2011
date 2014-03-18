using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP.Xml.Dom;
using System.Windows.Ink;
using SandRibbonInterop.Stanzas;
using SandRibbonObjects;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace PowerpointJabber
{
    public class Wire
    {
        public string SERVER = "madam.adm.monash.edu.au";
        protected XmppClientConnection conn;
        public Credentials myCredentials;
        private Location myLocation;
        public bool isConnected = false;
        public bool isInConversation = false;
        private Jid jid;
        private const string SYNC_MOVE = "/SYNC_MOVE";

        public Wire()
        {
            Constants.JabberWire.SERVER = ConfigurationProvider.instance.SERVER;
            conn = new XmppClientConnection(SERVER);
        }
        public void Connect()
        {
            var login = new loginDialogBox();
            login.ShowDialog();
            if (login.SuccessfulCompletionOfDialog)
            {
                AttachHandlers(conn);
                conn.Open(myCredentials.name, "examplePassword");
            }
        }
        public void startBroadcasting(string targetConversation)
        {
            //
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
            thisconn.OnLogin += (_) => isConnected = true; 
            thisconn.OnXmppConnectionStateChanged += conn_OnConnectionStateChanged;
        }
        private void conn_OnConnectionStateChanged(object sender, agsXMPP.XmppConnectionState connectionState)
        {
            switch (connectionState)
            {
                case XmppConnectionState.Connected:
                    isConnected = true;
                    break;
                case XmppConnectionState.Disconnected:
                    isConnected = false;
                    break;
                case XmppConnectionState.Connecting:
                    isConnected = false;
                    break;
            }
        }
        private void conn_OnError(object sender, Exception ex)
        {
            //System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
        }
        private void conn_OnWriteXML(object sender, string xml)
        {
            //System.Windows.Forms.MessageBox.Show("OUT: " + xml);
        }
        private void conn_OnReadXML(object sender, string xml)
        {
            //System.Windows.Forms.MessageBox.Show("IN: " + xml);
        }
        public void sendRawStroke(Stroke stroke)
        {

            var tStroke = new TargettedStroke
            {
                author = myCredentials.name,
                privacy = "public",
                slide = myLocation.currentSlide,
                target = "presentationSpace",
                stroke = stroke
            };
            SendStroke(tStroke);
        }
        public void sendRawDirtyStroke(Stroke stroke)
        {
            var element = new TargettedDirtyElement
            {
                author = myCredentials.name,
                privacy = "public",
                slide = myLocation.currentSlide,
                target = "presentationSpace",
                identifier = stroke.sum().checksum.ToString()
            };
            var dStroke = new MeTLStanzas.DirtyInk(element);
            sendDirtyStroke(element);
        }
        public void sendDirtyStroke(TargettedDirtyElement element)
        {
            stanza(new MeTLStanzas.DirtyInk(element));
        }
        public void SendStroke(TargettedStroke stroke)
        {
            stanza(stroke.slide.ToString(), new MeTLStanzas.Ink(stroke));
        }
        public void SendSyncMoveTo(int where)
        {
            command(myLocation.activeConversation, SYNC_MOVE + " " + where);
        }
        private void command(string where, string message)
        {
            send(where, message);
        }
        private void command(string message)
        {
            send("global", message);
        }
        private void joinRooms()
        {
            var rooms = new[]
            {
                Constants.JabberWire.GLOBAL,
                new Jid(myCredentials.name, Constants.JabberWire.MUC, jid.Resource),
                new Jid(myLocation.activeConversation,Constants.JabberWire.MUC,jid.Resource),
                new Jid(myLocation.currentSlide.ToString(), Constants.JabberWire.MUC,jid.Resource),
                new Jid(string.Format("{0}{1}", myLocation.currentSlide, myCredentials.name), Constants.JabberWire.MUC,jid.Resource)
            };
            foreach (var room in rooms.Where(r => r.User != null && r.User != "0"))
            {
                try
                {
                    joinRoom(room);
                }
                catch (Exception e)
                {
                }
            }
        }
        private void joinRoom(Jid room)
        {
            var alias = myCredentials.name + conn.Resource;
            new MucManager(conn).JoinRoom(room, alias, true);
        }
        public void JoinConversation(string room)
        {
            if (myLocation.activeConversation != null)
            {
                var muc = new MucManager(conn);
                muc.LeaveRoom(new Jid(myLocation.activeConversation + "@" + Constants.JabberWire.MUC), myCredentials.name);
            }
            myLocation.activeConversation = room;
            joinRooms();
        }
        public void stanza(string target, Element stanza)
        {
            var message = new agsXMPP.protocol.client.Message();
            string modifiedTarget =
                stanza.GetTag(MeTLStanzas.privacyTag) == "private" ?
                string.Format("{0}{1}", target, stanza.GetTag("author")) : target;
            message.To = new Jid(string.Format("{0}@{1}", modifiedTarget, Constants.JabberWire.MUC));
            message.From = jid;
            message.Type = MessageType.groupchat;
            message.AddChild(stanza);
            send(message);
        }
        public void stanza(Element stanza)
        {
            this.stanza(myLocation.currentSlide.ToString(), stanza);
        }
        private void send(string target, string message)
        {
            try
            {
                if (target.ToLower() == "global")
                    System.Windows.Forms.MessageBox.Show(string.Format("{0} fired on the global thread", message));
                send(new Message(new Jid(target + "@" + Constants.JabberWire.MUC), jid, MessageType.groupchat, message));
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Exception in send: {0}", e.Message));
            }
        }
        protected virtual void send(Message message)
        {
            conn.Send(message);
        }


    }

    public class Credentials
    {
        public string name;
        public string password;
        public List<AuthorizedGroup> authorizedGroups;
    }
    public class AuthorizedGroup
    {
        public AuthorizedGroup() { }
        public AuthorizedGroup(string newGroupKey)
        {
            groupType = "test only";
            groupKey = newGroupKey;
        }
        public AuthorizedGroup(string newGroupKey, string newGroupType)
        {
            groupType = newGroupType;
            groupKey = newGroupKey;
        }
        public string groupType { get; set; }
        public string groupKey { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is AuthorizedGroup)) return false;
            var otherGroup = (AuthorizedGroup)obj;
            return otherGroup.groupKey == this.groupKey;
        }
        public override int GetHashCode()
        {
            return groupKey.GetHashCode();
        }
    }
    public class Location
    {
        public string activeConversation;
        public int currentSlide;
        public List<int> availableSlides = new List<int>();
    }
    public class ConfigurationProvider : HttpResourceProvider
    {
        private static string server;
        private static string stagingServer;
        private static object instanceLock = new object();
        private static ConfigurationProvider instanceProperty;
        public bool isStaging = false;
        public static ConfigurationProvider instance
        {
            get
            {
                lock (instanceLock)
                    if (instanceProperty == null)
                        instanceProperty = new ConfigurationProvider();
                return instanceProperty;
            }
        }
        public string SERVER
        {
            get
            {
                if (isStaging)
                {
                    if (stagingServer == null)
                        stagingServer = XElement.Parse(HttpResourceProvider.insecureGetString(string.Format("http://metl.adm.monash.edu.au/stagingServer.xml"))).Value;
                    server = stagingServer;
                }
                else
                {
                    if (server == null || server == stagingServer)
                        server = XElement.Parse(HttpResourceProvider.insecureGetString(string.Format("http://metl.adm.monash.edu.au/server.xml"))).Value;
                }
                return server;
            }
        }
    }
    public class HttpResourceProvider
    {
        private static WebClient client()
        {
            return new WebClient { Credentials = new NetworkCredential("exampleUsername", "examplePassword") };
        }
        public static string secureGetString(string resource)
        {
            return client().DownloadString(resource);
        }
        public static string insecureGetString(string resource)
        {
            return new WebClient().DownloadString(resource);
        }
        public static string securePutData(string uri, byte[] data)
        {
            return decode(client().UploadData(uri, data));
        }
        public static byte[] secureGetData(string resource)
        {
            return client().DownloadData(resource);
        }
        public static string securePutFile(string uri, string filename)
        {
            return decode(client().UploadFile(uri, filename));
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
    public class AuthorisationProvider : HttpResourceProvider
    {
        public List<AuthorizedGroup> getEligibleGroups(string AuthcateName, string AuthcatePassword)
        {
            var groups = new List<AuthorizedGroup>();
            string encryptedPassword = Crypto.encrypt(AuthcatePassword);
            string sXML = HttpResourceProvider.insecureGetString(String.Format("http://{2}:1188/ldapquery.yaws?username={0}&password={1}", AuthcateName, encryptedPassword, Constants.JabberWire.SERVER));
            var doc = new XmlDocument();
            doc.LoadXml(sXML);
            if (doc.GetElementsByTagName("error").Count == 0)
            {
                groups.Add(new AuthorizedGroup("Unrestricted", ""));
                foreach (XmlElement group in doc.GetElementsByTagName("eligibleGroup"))
                {
                    groups.Add(new AuthorizedGroup(
                        group.InnerText.Replace("\"", ""),
                        group.Attributes["type"].Value));
                }
                groups.Add(new AuthorizedGroup(
                    doc.GetElementsByTagName("user")[0].Attributes["name"].Value,
                    "username"));
            }
            else
                foreach (XmlElement error in doc.GetElementsByTagName("error"))
                    System.Windows.Forms.MessageBox.Show("XmlError node:" + error.InnerText);
            return groups;
        }
    }
    public class Crypto
    {
        public static string encrypt(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";
            var Key = Encoding.UTF8.GetBytes("01234567");
            var IV = Encoding.UTF8.GetBytes("01234567");
            string CountPadding = ((input.Length).ToString()).PadLeft(8);
            var encryptedBytes = encryptToByteArray(input, Encoding.UTF8, Key, IV);
            var paddingBytes = encryptToByteArray(CountPadding, Encoding.UTF8, Key, IV);
            int newSize = encryptedBytes.Length + paddingBytes.Length;
            var ms = new MemoryStream(new byte[newSize], 0, newSize, true, true);
            ms.Write(encryptedBytes, 0, encryptedBytes.Length);
            ms.Write(paddingBytes, 0, paddingBytes.Length);
            byte[] merged = ms.GetBuffer();
            var Base64String = (Convert.ToBase64String(merged, System.Base64FormattingOptions.None));
            var Base64FinalString = ((Base64String.Replace("+", "-")).Replace("=", "_"));
            return Base64FinalString;
        }
        private static byte[] encryptToByteArray(string input, Encoding encoding, byte[] Key, byte[] IV)
        {
            string CountPadding = ((input.Length).ToString()).PadLeft(8);
            DESCryptoServiceProvider key = new DESCryptoServiceProvider()
            {
                Key = Key,
                IV = IV,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
            MemoryStream ms = new MemoryStream();
            ms.Flush();
            CryptoStream stream = new CryptoStream(ms, key.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(input);
            sw.Close();
            ms.Close();
            return ms.ToArray();
        }
        private static string bytestostring(byte[] p, Encoding encoding)
        {
            return encoding.GetString(p, 0, p.Length);
        }
    }
    public struct StrokeTag
    {
        public string author;
        public string privacy;
    }
    public struct StrokeChecksum
    {
        public double checksum;
    }
    public static class StrokeExtensions
    {
        private static Guid STROKE_TAG_GUID = Guid.NewGuid();
        private static Guid STROKE_PRIVACY_GUID = Guid.NewGuid();
        public static StrokeTag tag(this Stroke stroke)
        {
            return new StrokeTag
            {
                author = (string)stroke.GetPropertyData(STROKE_TAG_GUID),
                privacy = (string)stroke.GetPropertyData(STROKE_PRIVACY_GUID)
            };
        }
        public static StrokeTag tag(this Stroke stroke, StrokeTag tag)
        {
            stroke.AddPropertyData(STROKE_TAG_GUID, tag.author);
            stroke.AddPropertyData(STROKE_PRIVACY_GUID, tag.privacy);
            return tag;
        }
        private static Guid CHECKSUM = Guid.NewGuid();
        public static Guid sumId(this Stroke stroke)
        {
            return StrokeExtensions.CHECKSUM;
        }
        public static StrokeChecksum sum(this Stroke stroke)
        {//Removed memoization because it makes moved strokes still think they are in the same place.
            var checksum = new StrokeChecksum
            {
                checksum = stroke.StylusPoints.Aggregate(0.0, (acc, item) =>
                    acc + Math.Round(item.X, POINT_DECIMAL_PLACE_ROUNDING) + Math.Round(item.Y, POINT_DECIMAL_PLACE_ROUNDING))
            };
            return checksum;
        }
        public static int POINT_DECIMAL_PLACE_ROUNDING = 1;
    }
    class Extensions
    {
    }
}
