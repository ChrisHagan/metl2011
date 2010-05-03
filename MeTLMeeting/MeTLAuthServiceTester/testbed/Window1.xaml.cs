using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;
using System.Net;

namespace testbed
{
    /// <summary>
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            username.Text = "eecrole";
            input.Text = "m0nash2008";
            submit_Click(new object(), new RoutedEventArgs());
        }
        private void submit_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(input.Text))
                return;
            var Key = Encoding.UTF8.GetBytes("01234567");
            var IV = Encoding.UTF8.GetBytes("01234567");
            StringBuilder sb = new StringBuilder();
            sb.Append("Key:");
            foreach (byte b in Key)
            {
                sb.Append(" [" + b.ToString() + "]");
            }
            sb.AppendLine("");
            sb.Append("IV:");
            foreach (byte b in IV)
            {
                sb.Append(" [" + b.ToString() + "]");
            }
            sb.AppendLine("");

            sb.AppendLine("Original String: " + input.Text);
            string CountPadding = ((input.Text.Length).ToString()).PadLeft(8);
            sb.AppendLine("Original String Padded: " + input.Text + CountPadding);
            sb.Append("Original Bytes:");
            foreach (byte b in (System.Text.Encoding.UTF8.GetBytes(input.Text)))
            {
                sb.Append(" [" + b.ToString() + "]");
            }
            sb.AppendLine("");
            sb.Append("Original Bytes with Padding:");
            foreach (byte b in (System.Text.Encoding.UTF8.GetBytes(input.Text)))
            {
                sb.Append(" [" + b.ToString() + "]");
            }
            foreach (byte b in (System.Text.Encoding.UTF8.GetBytes(CountPadding)))
            {
                sb.Append(" [" + b.ToString() + "]");
            }
            sb.AppendLine("");
            sb.Append("Encrypted String: ");
            var encrypted = encryptToString(input.Text, Encoding.UTF8, Key, IV);
            var padding = encryptToString(CountPadding, Encoding.UTF8, Key, IV);
            sb.Append(encrypted);
            sb.AppendLine("");
            sb.Append("Encrypted UTF8:");
            var encryptedBytes = encryptToByteArray(input.Text, Encoding.UTF8, Key, IV);
            var paddingBytes = encryptToByteArray(CountPadding, Encoding.UTF8, Key, IV);
            foreach (byte b in (encryptedBytes))
            {
                sb.Append(" [" + b.ToString() + "]");
            }
            sb.AppendLine("");
            sb.Append("Base64 Encrypted UTF8: ");
            int newSize = encryptedBytes.Length + paddingBytes.Length;
            var ms = new MemoryStream(new byte[newSize], 0, newSize, true, true);
            ms.Write(encryptedBytes, 0, encryptedBytes.Length);
            ms.Write(paddingBytes, 0, paddingBytes.Length);
            byte[] merged = ms.GetBuffer();
            var Base64String = (Convert.ToBase64String(merged, System.Base64FormattingOptions.None));
            sb.AppendLine(Base64String);
            sb.Append("Safe Base64 Encrypted UTF8: ");
            var Base64FinalString = ((Base64String.Replace("+", "-")).Replace("=", "_"));
            sb.AppendLine(Base64FinalString);
            output.Text = sb.ToString();
            if (String.IsNullOrEmpty(username.Text))
                return;
            try
            {
                var request = HttpWebRequest.Create("http://reifier.adm.monash.edu:1188/ldapquery.yaws?username=" + username.Text + "&password=" + Base64FinalString);
                using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    response.Text = "";
                    response.Text = reader.ReadToEnd();
                    encryptedPassword.Text = Base64FinalString;
                }
            }
            catch (Exception ex)
            {
                response.Text = "";
                response.Text = "Exception: " + ex.ToString();
            }
        }
        private byte[] encryptToByteArray(string input, Encoding encoding, byte[] Key, byte[] IV)
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

        private string encryptToString(string input, Encoding encoding, byte[] Key, byte[] IV)
        {
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
            var output = bytestostring(ms.ToArray(), encoding);
            return output;
        }
        private string bytestostring(byte[] p, Encoding encoding)
        {
            return encoding.GetString(p, 0, p.Length);
        }
        private string chartostring(char[] p)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char b in p)
                sb.Append(b.ToString());
            return sb.ToString();
        }
        private string decrypt(string input, Encoding encoding)
        {
            var desanitized = ((input.Replace("-", "+")).Replace("_", "="));
            var de64ed = Convert.FromBase64String(desanitized);
            DESCryptoServiceProvider key = new DESCryptoServiceProvider()
            {
                Key = encoding.GetBytes("01234567"),
                IV = encoding.GetBytes("01234567"),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
            var bytes = de64ed;
            byte[] encodedBytes = new byte[bytes.Length - 8];
            byte[] encodedLength = new byte[8];
            System.Buffer.BlockCopy(bytes, 0, encodedBytes, 0, bytes.Length - 8);
            System.Buffer.BlockCopy(bytes, bytes.Length - 8, encodedLength, 0, 8);
            MemoryStream ms = new MemoryStream(encodedBytes);
            CryptoStream stream = new CryptoStream(ms, key.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(stream);
            string val = (sr.ReadLine().Trim());
            sr.Close();
            ms.Close();
            MemoryStream lms = new MemoryStream(encodedLength);
            CryptoStream lstream = new CryptoStream(lms, key.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader lsr = new StreamReader(lstream);
            string LengthOutput = lsr.ReadLine();
            var lval = Int32.Parse(LengthOutput.Trim());
            lsr.Close();
            lms.Close();
            return chartostring((val.Substring(0, lval)).ToArray());
        }
        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(encryptedPassword.Text))
                return;
            decryptResponse.Text = decrypt(encryptedPassword.Text, Encoding.UTF8);
        }
    }
}
