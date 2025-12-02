using System.Net.Sockets;

namespace ChatServer
{
    public class ServerSession : TcpSession
    {
        public ServerSession(Socket socket, int recvBufferSize = 4096) : base(socket, recvBufferSize)
        {
        }

        protected override void OnPacket(byte[] body)
        {
            throw new NotImplementedException();
        }
    }
}
