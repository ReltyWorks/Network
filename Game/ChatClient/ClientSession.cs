using Core.Net;
using System.Net.Sockets;

namespace ChatClient
{
    internal class ClientSession : TcpSession
    {
        public int ClientId { get; private set; }
        public event Action<IPacket> OnPacketReceived;

        public ClientSession(int clientId, Socket socket, int recvBufferSize = 4096) : base(socket, recvBufferSize)
        {
            ClientId = clientId;
        }

        protected override void OnPacket(byte[] body)
        {
            IPacket packet = PacketFactory.FromBytes(body);
            OnPacketReceived?.Invoke(packet);
        }

        public void Send(IPacket packet)
        {
            Send(PacketFactory.ToByte(packet));
        }
    }
}
