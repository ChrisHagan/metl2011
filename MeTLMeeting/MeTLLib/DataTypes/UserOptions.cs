using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace MeTLLib.DataTypes
{
    public class UserOptions
    {
        public int pedagogyLevel { get; set; }
        public int powerpointImportScale { get; set; }
        public string logLevel { get; set; }
        public static UserOptions DEFAULT = new UserOptions { 
            logLevel = "ERROR",
            pedagogyLevel = 2,
            powerpointImportScale = 1
        };
        public static UserOptions ReadXml(string xml) {
            return (UserOptions) new XmlSerializer(typeof(UserOptions)).Deserialize(new StringReader(xml));
        } 
        public static string WriteXml(UserOptions options) {
            var stream = new MemoryStream();
            new XmlSerializer(typeof(UserOptions)).Serialize(stream, options);
            return Encoding.UTF8.GetString(stream.ToArray());
        } 
    }
}
