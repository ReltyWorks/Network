using Core.Net;
using Game.NetworkContracts;
using System;
using System.Net.Sockets;
using UnityEngine;

namespace Game.Core.Net
{
    public class TcpClientSession : TcpSession
    {
        public event Action<IPacket> OnPacketReceived;

        public TcpClientSession(Socket socket, int recvBufferSize = 4096) : base(socket, recvBufferSize)
        {
        }

        protected override void OnPacket(byte[] body)
        {
            IPacket packet = PacketFactory.FromBytes(body);
            OnPacketReceived?.Invoke(packet);
        }

        public void Send(IPacket packet)
        {
            Send(PacketFactory.ToBytes(packet));
        }
    }
}