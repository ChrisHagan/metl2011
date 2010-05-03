using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SandRibbon.Utils
{
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
}
