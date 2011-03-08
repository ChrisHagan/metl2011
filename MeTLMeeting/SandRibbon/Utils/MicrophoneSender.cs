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
                socket.Send(Encoding.UTF8.GetBytes("icy-name:Unnamed Server\r\nicy-genre:Unknown Genre\r\nicy-pub:1\r\nicy-br:160\r\nicy-url:http://www.shoutcast.com\r\nicy-irc:%23shoutcast\r\nicy-icq:0\r\nicy-aim:N%2FA\r\n\r\n"));
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
            return buffer;
        }
    }
}
