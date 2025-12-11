using Core.Net;
using Game.Core.Net;
using Game.NetworkContracts;
using System;
using UnityEngine;

namespace Game.Net.Chat 
{
    public class ChatClient
    {
        public ChatClient(TcpClientSession session)
        {
            _session = session;
            _session.OnPacketReceived += OnRecvChatMessage;
        }

        TcpClientSession _session;

        public event Action<(int senderId, string message)> onRecvChatMessage;

        public void SendChatMessage(string message)
        {
            _session.Send(new C_ChatSend()
            {
                Text = message
            });
        }

        private void OnRecvChatMessage(IPacket packet)
        {
            if (packet is not S_ChatSend parsed)
                return;

            onRecvChatMessage?.Invoke((parsed.SenderId, parsed.Text));
        }
    }
}