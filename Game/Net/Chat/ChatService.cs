using Game.NetworkContracts;

namespace Game.Net.Chat
{
    public class ChatService : IDisposable
    {
        public ChatService(TcpServerSessionHub hub)
        {
            _hub = hub;
            _hub.OnPacketReceived += OnRecvChatMessage;
        }

        public void Dispose()
        {
            _hub.OnPacketReceived -= OnRecvChatMessage;
        }

        TcpServerSessionHub _hub;

        void OnRecvChatMessage(int senderId, IPacket packet)
        {
            if (packet is not C_ChatSend parsed)
                return;

            BroadCastChatMessage(senderId, parsed.Text);
        }

        void BroadCastChatMessage(int senderId, string text)
        {
            _hub.Broadcast(new S_ChatSend()
            {
                SenderId = senderId,
                Text = text
            });
        }
    }
}
