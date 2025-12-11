using Game.Chat;
using Grpc.Core;
using System.Collections.Concurrent;

namespace Game.Net.Chat
{

    public class ChatServiceImpl : ChatService.ChatServiceBase
    {
        private ConcurrentDictionary<int, IServerStreamWriter<ChatMessage>> _clients = new();

        public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            // 클라이언트에게 받은 메세지를 비동기로 소비
            await foreach (ChatMessage message in requestStream.ReadAllAsync())
            {
                _clients[context.GetHashCode()] = responseStream;
                await BroadcastAsync(message);
            }

            _clients.TryRemove(context.GetHashCode(), out _);
        }

        private async Task BroadcastAsync(ChatMessage message)
        {
            foreach (var client in _clients)
            {
                try
                {
                    await client.Value.WriteAsync(message);
                }
                catch
                {
                    // 메세지 전송 실패시 예외
                }
            }
        }
    }
}
