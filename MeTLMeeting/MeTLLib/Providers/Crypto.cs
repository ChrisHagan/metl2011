using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
//using Ninject;

namespace MeTLLib.Providers
{
    class Crypto
    {
        public MetlConfiguration metlServer { get; protected set; }
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly byte[] Key;
        private readonly byte[] IV;

        public Crypto(MetlConfiguration _metlServer)
        {
            metlServer = _metlServer;
            Key = encoding.GetBytes(metlServer.cryptoKey);
            encoding.GetBytes(metlServer.cryptoIV);
        }

        public string decrypt(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";

            var b64string = ((input.Replace("-","+")).Replace("_","="));
            var nonB64bytes = Convert.FromBase64String(b64string);
            var nonB64string = bytestostring(nonB64bytes, encoding);
            var decryptedString = decryptFromByteArray(nonB64bytes, encoding, Key, IV);
            
            var last8Bytes = getLastBytes(nonB64bytes, 8);
            var paddingBytes = decryptFromByteArray(last8Bytes, encoding, Key, IV);
            var paddingLength = Int32.Parse(paddingBytes);
            var decryptedStringFinal = decryptedString.Substring(0, paddingLength);
            return decryptedStringFinal;
        }
        private byte[] getLastBytes(byte[] input, int numberOfBytesToGet)
        {
            var ListOfBytes = new List<byte>();
            for (int i = 0; i < input.Length; i++)
            {
                if (i >= input.Length - numberOfBytesToGet)
                    ListOfBytes.Add(input[i]);
            }
            return ListOfBytes.ToArray();
        }
        public string encrypt(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";
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
        private string decryptFromByteArray(byte[] input, Encoding encoding, byte[] Key, byte[] IV)
        {
            DESCryptoServiceProvider key = new DESCryptoServiceProvider()
            {
                Key = Key,
                IV = IV,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
            CryptoStream stream = new CryptoStream(new MemoryStream(input), key.CreateDecryptor(), CryptoStreamMode.Read);
            var decryptedStream = new byte[input.Length];
            stream.Read(decryptedStream,0,Convert.ToInt32(input.Length));
            var decryptedString = bytestostring(decryptedStream, encoding);
            
            return decryptedString;
        }
        private string bytestostring(byte[] p, Encoding encoding)
        {
            return encoding.GetString(p, 0, p.Length);
        }
    }
}