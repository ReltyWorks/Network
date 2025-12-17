using Game.Lobbies;
using Google.Protobuf.Collections;
using Grpc.Core;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Net.Lobbies
{
    public class Lobby
    {
        public int Id { get; init; }
        public int MaxUser { get; set; }
        public string MasterUserId { get; set; }

        private HashSet<string> _members { get; } = new();

        private List<Subscriber> _subscribers = new();

        public class Subscriber : IAsyncDisposable
        {
            public Task Completion => _writeLoop;

            private Task _writeLoop;
            private readonly Channel<LobbyEvent> _channel;
            const int STREAM_BUFFER = 128;

            public Subscriber(IServerStreamWriter<LobbyEvent> writer)
            {
                _channel = Channel.CreateBounded<LobbyEvent>(new BoundedChannelOptions(STREAM_BUFFER)
                {
                    SingleReader = true,
                    SingleWriter = false, // broadcast 같은 호출을 여러 클라이언트가 동시다발적으로 호출가능(멀티스레딩)
                    FullMode = BoundedChannelFullMode.Wait // 만약 이벤트 종류별로 Channel 을 따로 쓴다면 DropOldest 고려하면 좋다
                });
                _writeLoop = LobbyEventWriteLoop(writer);
            }

            public async ValueTask ProduceEventAsync(LobbyEvent e)
            {
                await _channel.Writer.WriteAsync(e);
            }

            private async Task LobbyEventWriteLoop(IServerStreamWriter<LobbyEvent> writer)
            {
                // ConfigureAwait(false) 해준이유 : 이 작업이 굳이 호출쓰레드 복귀를 하지않고 작업만 하면 되기때문에
                await foreach (var e in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
                {
                    await writer.WriteAsync(e).ConfigureAwait(false);
                }
            }

            public async ValueTask DisposeAsync()
            {
                _channel.Writer.TryComplete();
                await _writeLoop.ConfigureAwait(false);
            }
        }

        private readonly object _gate = new();

        /// <summary>
        /// 로비의 속성
        /// ex) "방제목","초고수만1:1"  "맵종류","사막"
        /// </summary>
        private Dictionary<string, string> _customProperties { get; } = new();

        /// <summary>
        /// user_id 별 kv.
        /// "IsReady", "True"
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _memberCustomProperties { get; } = new();


        public bool TryJoin(string userId)
        {
            lock (_gate)
            {
                if (_members.Count >= MaxUser)
                    return false;

                if (_members.Add(userId) == false)
                    return false;

                _memberCustomProperties[userId] = new();
                return true;
            }
        }

        public bool TryLeave(string userId, out int remainMemberCount)
        {
            lock (_gate)
            {
                remainMemberCount = _members.Count;

                if (_members.Remove(userId) == false)
                    return false;

                remainMemberCount--;
                _memberCustomProperties.Remove(userId);
                return true;
            }
        }

        public bool TrySetCustomProperties(IDictionary<string, string> kv)
        {
            lock (_gate)
            {
                foreach (var (k, v) in kv)
                    _customProperties[k] = v;

                return true;
            }
        }

        public bool TrySetMemberCustomProperties(string userId, IDictionary<string, string> kv)
        {
            lock (_gate)
            {
                if (_memberCustomProperties.TryGetValue(userId, out var customProperties) == false)
                    return false;

                foreach (var (k, v) in kv)
                    customProperties[k] = v;

                return true;
            }
        }

        public Subscriber Subscribe(IServerStreamWriter<LobbyEvent> writer)
        {
            var subscriber = new Subscriber(writer);

            lock (_gate)
            {
                _subscribers.Add(subscriber);
            }

            // 구독자의 구독이 끝나면 구독자목록에서 제거
            subscriber.Completion.ContinueWith(_ =>
            {
                lock (_gate)
                {
                    _subscribers.Remove(subscriber);
                }
            });

            return subscriber;
        }

        public void Broadcast(LobbyEvent e)
        {
            lock (_gate)
            {
                foreach (var subscriber in _subscribers)
                {
                    _ = subscriber.ProduceEventAsync(e);
                }
            }
        }

        public LobbyInfo ToLobbyInfo()
        {
            lock (_gate)
            {
                return new LobbyInfo
                {
                    LobbyId = Id,
                    MaxClient = MaxUser,
                    NumClient = _members.Count,
                    MasterUserId = MasterUserId,
                    CustomProperties = { _customProperties }
                };
            }
        }

        public UserInLobbyInfo GetUserInfo(string userId)
        {
            lock (_gate)
            {
                if (!_memberCustomProperties.TryGetValue(userId, out var properties))
                    return null;

                return new UserInLobbyInfo
                {
                    UserId = userId,
                    CustomProperties = { properties }
                };
            }
        }

        public RepeatedField<UserInLobbyInfo> GetUserInfos()
        {
            lock (_gate)
            {
                IEnumerable<UserInLobbyInfo> enumerable = 
                    _memberCustomProperties.Select(x =>
                    {
                        return new UserInLobbyInfo
                        {
                            UserId = x.Key,
                            CustomProperties = { x.Value }
                        };
                    });

                var repeatedField = new RepeatedField<UserInLobbyInfo>();
                repeatedField.AddRange(enumerable);
                return repeatedField;
            }
        }
    }
}
