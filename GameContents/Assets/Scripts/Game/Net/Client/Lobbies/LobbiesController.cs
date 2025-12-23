using Game.Lobbies;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NetworkStatus;
using Google.Protobuf.WellKnownTypes;

namespace Game.Net.Client.Lobbies
{
    public class LobbiesController : MonoBehaviour
    {
        [SerializeField] LobbiesView _view;
        LobbiesService.LobbiesServiceClient _client;
        AsyncServerStreamingCall<LobbyEvent> _subscribedLobbyCall;

        LobbyInfo _cachedLobbyInfo;
        List<UserInLobbyInfo> _cachedUserInfos = new();

        private void Awake()
        {
            _client = new LobbiesService.LobbiesServiceClient(GrpcConnection.channel);
        }

        private void OnEnable()
        {
            /*
             * 주의할점 : 비동기작업을 수행할시, View 를 사용자가 조작하지못하도록 대기동안 막는 로직 필요함.
             */
            _view.onCreateLobbyConfirm += CreateLobbyAsync;
            _view.onJoinButtonClicked += JoinLobbyAsync;
            _view.onLeaveClicked += LeaveLobby; // 스레드잠깐 블록 되도 괜찮거나/ async-await 가 좀 어렵다면 이런식으로 해도됨.
            _view.onRefreshButtonClicked += GetLobbyList;
            _view.onReadyClicked += SetReady;
            _view.onStartClicked += TryStart;
        }

        public void SetReady()
        {
            SetUserInLobbyCustomProperties(NetworkBlackboard.userId, new Dictionary<string, string>
            {
                { "IsReady" , "true" }
            });
        }

        public void TryStart()
        {
            bool allReady = true;

            foreach (var userInfo in _cachedUserInfos)
            {
                if (userInfo.CustomProperties.TryGetValue("IsReady", out string value) &&
                    Convert.ToBoolean(value) == false)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady == false)
                return; // TODO : 준비를 하지않은 멤버가 있습니다 알림

            // TODO : 서버에 게임시작 요청 호출
        }

        public async void CreateLobbyAsync(string title, int maxUser)
        {
            var request = new CreateLobbyRequest
            {
                UserId = NetworkBlackboard.userId,
                MaxClient = maxUser
            };

            try
            {
                var response = await _client.CreateLobbyAsync(request);

                if (response.Success)
                {
                    SubscribeLobby(response.LobbyInfo.LobbyId);
                    SetLobbyCustomProperties(new Dictionary<string, string>
                    {
                        { "Title" , title }
                    });
                    SetUserInLobbyCustomProperties(NetworkBlackboard.userId, new Dictionary<string, string>
                    {
                        { "IsReady" , "true" }
                    });

                    _cachedLobbyInfo = response.LobbyInfo;
                    _cachedUserInfos.Add(response.UserInLobbyInfo);
                    _view.RefreshUserSlots(_cachedUserInfos);
                    _view.SetBeforeJoinLobbyCanvas(false);
                    _view.SetAfterJoinLobbyCanvas(true);                    
                }
                else
                {
                    // TODO : 로비생성 실패에대한 알림 팝업
                }
            }
            catch (Exception ex)
            {
                // TODO : 연결 문제에 대한 알림 팝업
                throw ex;
            }
        }

        public async void JoinLobbyAsync(int lobbyId)
        {
            var request = new JoinLobbyRequest
            {
                LobbyId = lobbyId,
                UserId = NetworkBlackboard.userId
            };
            try
            {
                var response = await _client.JoinLobbyAsync(request);

                if (response.Success)
                {
                    SubscribeLobby(response.LobbyInfo.LobbyId);
                    SetUserInLobbyCustomProperties(NetworkBlackboard.userId, new Dictionary<string, string>
                    {
                        { "IsReady" , "false" }
                    });
                    _cachedLobbyInfo = response.LobbyInfo;
                    _cachedUserInfos = new List<UserInLobbyInfo>(response.UserInLobbyInfos);
                    _view.RefreshUserSlots(_cachedUserInfos);
                    _view.SetBeforeJoinLobbyCanvas(false);
                    _view.SetAfterJoinLobbyCanvas(true);
                }
                else
                {
                    // TODO : 로비진입 실패에대한 알림 팝업
                }
            }
            catch (Exception ex)
            {
                // TODO : 연결 문제에 대한 알림 팝업
                throw ex;
            }
        }

        public void LeaveLobby()
        {
            if (_cachedLobbyInfo == null)
                return; // TODO : 이미 나가고 없다는 예외알림 팝업

            try
            {
                var request = new LeaveLobbyRequest
                {
                    UserId = NetworkBlackboard.userId,
                    LobbyId = _cachedLobbyInfo.LobbyId
                };

                var response = _client.LeaveLobby(request);

                if (response.Success)
                {
                    _cachedLobbyInfo = null;
                    _view.SetBeforeJoinLobbyCanvas(true);
                    _view.SetAfterJoinLobbyCanvas(false);
                }
                else
                {
                    // TODO : 실패 알림 
                }
            }
            catch (Exception ex) 
            {
                // TODO : 예외 알림
                throw ex;
            }
        }

        public void GetLobbyList()
        {
            try
            {
                var response = _client.GetLobbyList(new Empty());
                _view.OnGetLobbyList(response.LobbyInfos);
            }
            catch (Exception ex)
            {
                // TODO : 예외 알림
                throw ex;
            }
        }

        public void SetLobbyCustomProperties(IDictionary<string, string> customProperties)
        {
            if (_cachedLobbyInfo == null)
                return;

            try
            {
                var request = new SetCustomPropertiesRequest
                {
                    LobbyId = _cachedLobbyInfo.LobbyId,
                    Kv = { customProperties }
                };

                var response = _client.SetCustomProperties(request);
            }
            catch (Exception ex)
            {
                // TODO : 예외 알림
                throw ex;
            }
        }

        public void SetUserInLobbyCustomProperties(string userId, IDictionary<string, string> customProperties)
        {
            if (_cachedLobbyInfo == null)
                return;

            try
            {
                var request = new SetUserCustomPropertiesRequest
                {
                    LobbyId = _cachedLobbyInfo.LobbyId,
                    UserId = userId,
                    Kv = { customProperties }
                };

                var response = _client.SetUserCustomProperties(request);
            }
            catch (Exception ex)
            {
                // TODO : 예외 알림
                throw ex;
            }
        }

        public void SubscribeLobby(int lobbyId)
        {
            var request = new SubscribeLobbyRequest
            {
                LobbyId = lobbyId,
                UserId = NetworkBlackboard.userId
            };

            _subscribedLobbyCall = _client.SubscribeLobby(request);

            // Task.Run 하게되면 메인이 아닌 다른 쓰레드를 가져다 쓰게 될것이므로
            // 엔진 이벤트와 동기화가 되려면 메인 쓰레드 동기화가 필요하다
            Task.Run(async () =>
            {
                await foreach (var lobbyEvent in _subscribedLobbyCall.ResponseStream.ReadAllAsync())
                    await HandleLobbyEvent(lobbyEvent);
            });
        }

        async Task HandleLobbyEvent(LobbyEvent e)
        {
            await Awaitable.MainThreadAsync();

            switch (e.Type)
            {
                case LobbyEvent.Types.EventType.MemberJoin:
                    {
                        _cachedUserInfos.Add(new UserInLobbyInfo
                        {
                            UserId = e.UserId,
                            CustomProperties = { e.Kv }
                        });
                        _view.RefreshUserSlots(_cachedUserInfos);
                    }
                    break;
                case LobbyEvent.Types.EventType.MemberLeft:
                    {
                        int index = _cachedUserInfos.FindIndex(user => user.UserId == e.UserId);

                        if (index < 0)
                            return;

                        _cachedUserInfos.RemoveAt(index);
                        _view.RefreshUserSlots(_cachedUserInfos);
                    }
                    break;
                case LobbyEvent.Types.EventType.LobbyCustomPropChanged:
                    {
                        // 알아서 해보시길.. 
                    }
                    break;
                case LobbyEvent.Types.EventType.UserCustomPropChanged:
                    {
                        int index = _cachedUserInfos.FindIndex(user => user.UserId == e.UserId);

                        if (index < 0)
                            return;

                        // 이렇게 gRPC 메세지를 직접 캐싱 하는거보다 캐싱용 구조체 따로 만들어서 값만 캐싱하는게 낫다..
                        // 시간나면 로비, 유저 데이터 캐싱 부분 바꿔봐라..
                        _cachedUserInfos[index] = new UserInLobbyInfo
                        {
                            UserId = e.UserId,
                            CustomProperties = { e.Kv }
                        };
                        _view.RefreshUserSlots(_cachedUserInfos);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
