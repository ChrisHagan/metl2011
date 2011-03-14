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
        //This is where we decide upon the microphone.  We may need to find a better way of finding a microphone than just choosing the first to contain "Mic".
        private static WaveIn microphoneDevice;
        private static readonly Microphone.Native.WavInDevice RawMicrophoneDevice = WaveIn.Devices.Where(s => s.Name.Contains("Mic")).FirstOrDefault();
        
        //We support streaming over two different protocols.  We prefer icecast - it allows us to dynamically set mountpoints.  If we have to use shoutcast, we'll have to build a mount-pool, and perform pool-management, on the server side.
        private enum Protocols { Shoutcast, Icecast };
        private static Protocols Protocol = Protocols.Icecast;

        //Socket is necessary for shoutcast.
        private static Socket socket;
        //Client is necessary for icecast.
        private static TcpClient client;
        private static NetworkStream icecastStream;
        //SocketHost is used by both icecast and shoutcast.  This is the address and port of the server.
        private static readonly string SocketHost = "radar.adm.monash.edu.au";
        private static readonly int SocketPort = 8500;

        //These fields determine the input format of the microphone, and the output format of the MP3 encoder.
        private static readonly WaveLib.WaveFormat inputFormat = new WaveLib.WaveFormat(48000, 16, 2);
        private static readonly BE_CONFIG mp3Format = new BE_CONFIG(inputFormat, 160);

        //These fields should be modified as need be.  These are purely cosmetic and informational.
        private static readonly string MP3Name = "MeTL Lecturer";
        private static readonly string MP3Genre = "Unicast Lecture Content";
        private static readonly string MP3Description = "This is a live stream of whatever";
        private static readonly string MP3Url = "http://metl.adm.monash.edu.au/MeTL2011";

        //This field is the name fo the stream - clients can connect to "http://{SocketHost}:{SocketPort}/{IcecastMountPoint}.m3u"
        private static readonly string IcecastMountPoint = "MeTLStream1";

        //Do not modify these fields.  These are necessary for the underlying socket negotiation.
        private static readonly string IcecastRequestType = String.Format("SOURCE /{0} ICE/1.0", IcecastMountPoint);
        private static readonly int MP3Bitrate = mp3Format.format.mp3.bOriginal;
        private static readonly string HeaderSeparator = "\r\n";
        private static readonly string IceCastHeaders = string.Join(HeaderSeparator,
            //string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{0}",
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
            String.Format("ice-audio-info: ice-samplerate={0};ice-bitrate={1};ice-channels={2}", inputFormat.nSamplesPerSec, MP3Bitrate, inputFormat.nChannels),
            HeaderSeparator
            );
        private static readonly string ShoutcastPasswordAttempt = "sourcePassword\r\n";
        private static readonly string ShoutcastPasswordAccepted = "OK2\r\nicy-caps:11\r\n\r\n";
        private static readonly string ShoutcastHeaders = string.Format("icy-name:{0}\r\nicy-description:{4}\r\nicy-genre:{1}\r\nicy-pub:0\r\nicy-br:{2}\r\nicy-url:{3}\r\nicy-irc:%23shoutcast\r\nicy-icq:0\r\nicy-aim:N%2FA\r\n\r\n", MP3Name, MP3Genre, MP3Bitrate, MP3Url, MP3Description);

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
            {
                microphoneDevice.Stop();
                microphoneDevice = null;
            }
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
                    if (icecastStream != null)
                    {
                        icecastStream.Close(0);
                        icecastStream.Dispose();
                        icecastStream = null;
                    }
                    if (client.Connected)
                        client.Close();
                    if (client != null)
                        client = null;
                    break;
            }
        }
        private static void establishMicrophone()
        {
            microphoneDevice = new WaveIn(RawMicrophoneDevice, inputFormat.nSamplesPerSec, inputFormat.wBitsPerSample, inputFormat.nChannels, inputFormat.nSamplesPerSec * (inputFormat.wBitsPerSample / 8) * inputFormat.nChannels);
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
            if (!socket.Connected)
                socket.Connect(SocketHost, SocketPort + 1);
            byte[] responseBuffer = new byte[20];
            socket.ReceiveTimeout = 5000;
            socket.Send(Encoding.UTF8.GetBytes(ShoutcastPasswordAttempt));
            socket.Receive(responseBuffer);
            if (Encoding.UTF8.GetString(responseBuffer) == ShoutcastPasswordAccepted)
            {
                socket.Send(Encoding.UTF8.GetBytes(ShoutcastHeaders));
                return true;
            }
            else return false;
        }
        private static bool establishIcecastConnection()
        {
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(SocketHost, SocketPort);
            icecastStream = client.GetStream();
            var bytes = Encoding.UTF8.GetBytes(IceCastHeaders);
            icecastStream.Write(bytes, 0, bytes.Length);
            icecastStream.Flush();
            return client.Connected;
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
                    bool res = false;
                    if (icecastStream == null || client == null || !client.Connected)
                        res = establishIcecastConnection();
                    else res = true;
                    if (res)
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
            var writer = new Yeti.MMedia.Mp3.Mp3Writer(outputStream, inputFormat, mp3Format);
            writer.Write(buffer);
            writer.Flush();
            return outputStream.ToArray();
        }
    }
}
