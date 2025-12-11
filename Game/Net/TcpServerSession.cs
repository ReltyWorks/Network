using System.Net.Sockets;
using Core.Net;
using Game.NetworkContracts;

namespace Game.Net
{
    public class TcpServerSession : TcpSession
    {
        public int ClientId { get; private set; }

        /// <summary>
        /// client id (sender id) , packet
        /// </summary>
        public event Action<int, IPacket> OnPacketReceived;


        public TcpServerSession(int clientId, Socket socket, int recvBufferSize = 4096) : base(socket, recvBufferSize)
        {
            ClientId = clientId;
        }

        protected override void OnPacket(byte[] body)
        {
            IPacket packet = PacketFactory.FromBytes(body);
            OnPacketReceived?.Invoke(ClientId, packet);
        }

        public void Send(IPacket packet)
        {
            Send(PacketFactory.ToBytes(packet));
        }
    }
}
