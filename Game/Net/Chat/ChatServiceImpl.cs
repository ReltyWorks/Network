/*
 * gRPC 생성후 사용방법 :
 * Server : 
 * *ServcieBase 를 상속받아서 gRPC 매핑 함수를 override 해서 비즈니스 로직을 작성
 */
using Game.Chat;
using Grpc.Core;
using System.Collections.Concurrent;

namespace Game.Net.Chat
{
    public class ChatServiceImpl : ChatService.ChatServiceBase
    {
        const int HISTORY_LIMIT = 100;

        static readonly ConcurrentDictionary<int, IServerStreamWriter<ChatMessage>> s_clients = new();
        static readonly ConcurrentQueue<ChatMessage> s_history = new();

        public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            s_clients[context.GetHashCode()] = responseStream;

            // 클라이언트에게 받은 메세지를 비동기로 소비
            await foreach (ChatMessage message in requestStream.ReadAllAsync())
            {
                AddHistory(message);
                await BroadcastAsync(message);
            }

            s_clients.TryRemove(context.GetHashCode(), out _);
        }

        public override async Task<GetChatHistoryResponse> GetChatHistory(GetChatHistoryRequest request, ServerCallContext context)
        {
            var response = new GetChatHistoryResponse();
            response.History.AddRange(s_history);
            return response;
        }

        private async Task BroadcastAsync(ChatMessage message)
        {
            foreach (var client in s_clients)
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

        private void AddHistory(ChatMessage message)
        {
            if (s_history.Count == HISTORY_LIMIT)
            {
                if (s_history.TryDequeue(out _))
                    s_history.Enqueue(message);
            }
            else
            {
                s_history.Enqueue(message);
            }
        }
    }
}
