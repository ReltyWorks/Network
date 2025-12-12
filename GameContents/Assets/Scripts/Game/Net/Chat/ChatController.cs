/*
 * gRPC 생성 후 사용방법:
 * Client:
 * *.*Client 라는 클래스가 생성되므로
 * 이 Client 클래스를 생성하면서 gRPC Channel 을 주입 
 * (Channel 이란 생산자-소비자 큐 모델, 네트워킹에서 Channel 은 클라이언트-서버 간 연결 및 호출 관리 모델)
 * 
 * gRPC call 객체를 통해 gRPC 통신중 이벤트를 처리
 * 
 * 
 * 인터셉터 : 요청/응답을 가로채는 객체
 * 기본구현 요청/응답처리 파이프라인만으로는 특정 로직을 요청/응답처리과정에서 구현할수없으므로 
 * 요청/응답처리 객체의 요청/응답정보를 가져와서 추가로직을 수행할수있도록 구현할수있다.
 */
using UnityEngine;
using System.Text;
using Game.Chat;
using Grpc.Core;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading;

namespace Game.Net.Chat
{
    public class ChatController : MonoBehaviour
    {
        [SerializeField] ChatView _view;
        ChatService.ChatServiceClient _client;
        AsyncDuplexStreamingCall<ChatMessage, ChatMessage> _chatCall;
        CancellationTokenSource _cts;
        StringBuilder _historyBuilder;

        private void Awake()
        {
            _historyBuilder = new StringBuilder(); // TODO : Reserving
            _cts = new CancellationTokenSource(); // 이 객체가 파괴되어도, ReceiveLoop 는 계속 돌기때문에 취소가필요함
            _client = new ChatService.ChatServiceClient(GrpcConnection.channel);

            // gRPC call 을 핸들링하는 객체 구현되어있으므로.. client.{gRPC 이름}() 으로 가져와서 쓸수있다. 
            _chatCall = _client.Chat();
        }

        private void OnEnable()
        {
            _view.onTextSubmit += OnTextSubmit;
            _ = ReceiveMessageLoopAsync(_cts.Token);
        }

        private void OnDisable()
        {
            _view.onTextSubmit -= OnTextSubmit;

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch { }

            try
            {
                _chatCall?.RequestStream?.CompleteAsync();
                _chatCall?.Dispose();
            }
            catch { }
        }

        private void Start()
        {
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            var request = new GetChatHistoryRequest()
            {
                Limit = 100,
                Since = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(5))
            };
            var response = await _client.GetChatHistoryAsync(request);

            foreach (var message in response.History)
            {
                _historyBuilder.AppendLine($"{message.SenderId} : {message.Content}");
            }

            _view.SetHistory(_historyBuilder.ToString());
        }

        public void OnTextSubmit(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            Debug.Log("Sending.." + text);
            _view.ClearText();
            _ = SendMessageAsync(text);
        }

        private async Task SendMessageAsync(string text)
        {
            var message = new ChatMessage
            {
                Content = text,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            try
            {
                await _chatCall.RequestStream.WriteAsync(message);
            }
            catch (RpcException ex)
            {
                Debug.LogError($"전송 실패 : {ex.Status} , {ex.Message}");
            }
        }

        private async Task ReceiveMessageLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (ChatMessage message in _chatCall.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    _historyBuilder.AppendLine($"{message.SenderId} : {message.Content}");
                    _view.SetHistory(_historyBuilder.ToString());
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Debug.Log($"전송 취소 : {ex.Status} , {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}