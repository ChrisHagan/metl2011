using System.Net.Sockets;
using System.Text;

namespace HeadfulClassRoom
{
    public class Telnet
    {
        public static TcpClient client;
        public static UTF8Encoding Encoding = new UTF8Encoding();
        public static void Send(string message)
        {
            if (client == null)
            {
                client = new TcpClient("localhost", 23);
            }
            client.GetStream().Write(Encoding.GetBytes(message), 0, message.Length);
        }
    }
}
