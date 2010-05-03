using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using SandRibbon.Utils.Connection;

namespace SandRibbon.Utils
{
    class WebServiceUtilities
    {
        /*

        [Obsolete("Now overloaded")]
        public static void fillSubjects(ComboBox cbo)
        {
            try
            {
                //var MeTLAuthClient = new MeTLAuthService.MeTLAuthServiceSoapClient();
                string userName = "eecrole";
                string password = "m0nash2008";
                string sOutput = ""; //= MeTLAuthClient.eligibleConversationGroups(userName, password).ToString();
                Dictionary<string, string> d = convertXMLToDictionary(sOutput);
                foreach (string s in d.Keys)
                {
                    var oNewItem = new ComboBoxItem();
                    oNewItem.Content = s;
                    cbo.Items.Add(oNewItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void setSubject(ComboBox cbo, string selection)
        {
            ComboBoxItem c = (ComboBoxItem)cbo.FindName(selection);
            cbo.SelectedItem = c;
        }

        public static void setSubject(ComboBox cbo, int id)
        {
            cbo.SelectedIndex = id;
        }

        private static Dictionary<string, string> convertXMLToDictionary(string sInput)
        {
            Dictionary<string, string> dLocal = new Dictionary<string, string>();
            XmlDocument x = new System.Xml.XmlDocument();
            x.InnerXml = sInput;
            foreach (XmlElement xE in x.FirstChild.FirstChild)
            {
                string key = xE.Name;
                string value = xE.Value;
                if (!dLocal.ContainsKey(key))
                    dLocal.Add(key, value);
            }
            return dLocal;
        }
*/
    }
}
