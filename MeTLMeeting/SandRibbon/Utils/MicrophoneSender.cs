using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech;
using SpeechLib;
using System.IO;
using System.Net;
using System.Windows;
using SandRibbon.Utils.Microphone;
using System.Net.Sockets;
using Yeti.Lame;

namespace SandRibbon.Utils
{
    class MicrophoneSender
    {
        private static Socket socket;
        private static WaveIn microphoneDevice;
        private static readonly string MP3Name = "MeTL Lecturer";
        private static readonly string MP3Genre = "Unicast Lecture Content";
        private static readonly string MP3Description = "This is a live stream of whatever";
        private static readonly int MP3Bitrate = 160;
        private static readonly string MP3Url = "http://metl.adm.monash.edu.au/MeTL2011";
        private static readonly string IcecastMountPoint = "MeTLStream1";
        private static TcpClient client;
        private static readonly string SocketHost = "radar.adm.monash.edu.au";
        private static readonly string IcecastRequestType = String.Format("SOURCE /{0} ICE/1.0",IcecastMountPoint);
        private static readonly string IceCastHeaders = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{0}",
            "\r\n",
            IcecastRequestType,
            "content-Type: audio/mpeg",
            "Authorization: Basic c291cmNlOnNvdXJjZVBhc3N3b3Jk",
            String.Format("ice-name: {0}", MP3Name),
            String.Format("ice-url: {0}", MP3Url),
            String.Format("ice-genre: {0}", MP3Genre),
            String.Format("ice-bitrate: {0}", MP3Bitrate),
            "ice-private: 1",
            "ice-public: 0",
            String.Format("ice-description: {0}", MP3Description),
            String.Format("ice-audio-info: ice-samplerate=44100;ice-bitrate={0};ice-channels=2", MP3Bitrate)
            );
        private static NetworkStream icecastStream;
        private enum Protocols { Shoutcast, Icecast };
        private static Protocols Protocol = Protocols.Icecast;
        public static bool startSending()
        {
            if (microphoneDevice == null)
                establishMicrophone();
            if (connectionIsValid())
            {
                microphoneDevice.Start();
                return true;
            }
            else return false;
        }
        public static void stopSending()
        {
            if (microphoneDevice != null)
                microphoneDevice.Stop();

            switch (Protocol)
            {
                case Protocols.Shoutcast:
                    if (socket != null && socket.Connected)
                    {
                        socket.Disconnect(false);
                        socket.Dispose();
                        socket = null;
                    }
                    break;
                case Protocols.Icecast:
                    break;
            }

        }
        private static void establishMicrophone()
        {
            microphoneDevice = new WaveIn(WaveIn.Devices.Where(s => s.Name.Contains("Mic")).FirstOrDefault(), 44100, 16, 2, 98200);
            microphoneDevice.BufferFull += sendBuffer;
        }
        private static bool connectionIsValid()
        {
            bool res = false;
            switch (Protocol)
            {
                case Protocols.Shoutcast:
                    res = sendShoutcastHeaders();
                    break;
                case Protocols.Icecast:
                    res = establishIcecastConnection();
                    break;
            }
            return res;
        }
        private static bool sendShoutcastHeaders()
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string headers = "sourcePassword\r\n";
            if (!socket.Connected)
                socket.Connect("radar.adm.monash.edu.au", 8501);
            byte[] responseBuffer = new byte[20];
            socket.ReceiveTimeout = 20000;
            socket.Send(Encoding.UTF8.GetBytes(headers));
            socket.Receive(responseBuffer);
            if (Encoding.UTF8.GetString(responseBuffer) == "OK2\r\nicy-caps:11\r\n\r\n")
            {
                socket.Send(Encoding.UTF8.GetBytes(string.Format("icy-name:{0}\r\nicy-description:{4}\r\nicy-genre:{1}\r\nicy-pub:0\r\nicy-br:{2}\r\nicy-url:{3}\r\nicy-irc:%23shoutcast\r\nicy-icq:0\r\nicy-aim:N%2FA\r\n\r\n", MP3Name, MP3Genre, MP3Bitrate, MP3Url, MP3Description)));
                return true;
            }
            else return false;
        }
        private static bool establishIcecastConnection()
        {
            bool res = false;
            client = new TcpClient();
            client.Connect(SocketHost, 8500);
            icecastStream = client.GetStream();
            var bytes = Encoding.UTF8.GetBytes(IceCastHeaders);
            icecastStream.Write(bytes,0,bytes.Length);
            icecastStream.Flush();
            res = true;
            return res;
        }
        private static void sendBuffer(byte[] buffer)
        {
            byte[] data = transcode(buffer);
            switch (Protocol)
            {
                case Protocols.Shoutcast:
                    if (socket == null || !socket.Connected)
                        sendShoutcastHeaders();
                    if (socket != null && socket.Connected)
                        socket.Send(data);
                    break;
                case Protocols.Icecast:
                    if (icecastStream != null && client != null)
                    {
                        icecastStream.Write(data, 0, data.Length);
                        icecastStream.Flush();
                    }   
                    break;
            }

        }
        private static byte[] transcode(byte[] buffer)
        {
            MemoryStream outputStream = new MemoryStream();
            try
            {
                var writer = new Yeti.MMedia.Mp3.Mp3Writer(outputStream, new WaveLib.WaveFormat(44100, 16, 2));
                writer.WriterConfig.Format.cbSize = 160;
                writer.Write(buffer);
                writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Mp3 Conversion :" + e.Message);
            }
            return outputStream.ToArray();
        }
    }
}
