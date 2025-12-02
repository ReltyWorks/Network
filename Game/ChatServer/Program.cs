using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7386);
            TcpListener listener = new TcpListener(endPoint);
            Socket socket = listener.AcceptSocket();

            ServerSession serverSession = new ServerSession(socket);
            serverSession.Start();
        }
    }
}