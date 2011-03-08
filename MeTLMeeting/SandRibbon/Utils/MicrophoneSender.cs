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
        public static void startSending()
        {
            if (microphoneDevice == null)
                establishMicrophone();
            if (sendShoutcastHeaders())
                microphoneDevice.Start();
        }
        public static void stopSending()
        {
            if (microphoneDevice != null)
                microphoneDevice.Stop();
            if (socket != null && socket.Connected)
            {
                socket.Disconnect(false);
                socket.Dispose();
                socket = null;
            }
        }
        private static void establishMicrophone()
        {
            microphoneDevice = new WaveIn(WaveIn.Devices.Where(s => s.Name.Contains("Mic")).FirstOrDefault(), 44100, 16, 1, 98200);
            microphoneDevice.BufferFull += sendBuffer;
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
                socket.Send(Encoding.UTF8.GetBytes(string.Format("icy-name:{0}\r\nicy-description:{4}\r\nicy-genre:{1}\r\nicy-pub:0\r\nicy-br:{2}\r\nicy-url:{3}\r\nicy-irc:%23shoutcast\r\nicy-icq:0\r\nicy-aim:N%2FA\r\n\r\n",MP3Name,MP3Genre,MP3Bitrate,MP3Url,MP3Description)));
                return true;
            }
            else return false;
        }
        private static void sendBuffer(byte[] buffer)
        {
            if (socket != null && socket.Connected)
                socket.Send(transcode(buffer));
        }
        private static byte[] transcode(byte[] buffer)
        {
            //so, here the wav-encoded data needs to be transcoded into 160kbps Mp3.  In fact, we can drop considerably from 160, but I figure, why not start high?
            return buffer;
        }
    }
}
