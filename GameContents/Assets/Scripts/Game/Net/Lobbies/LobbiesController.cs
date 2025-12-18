using Game.Lobbies;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NetworkStatus;

namespace Game.Net.Lobbies
{
    public class LobbiesController : MonoBehaviour
    {
        [SerializeField] LobbiesView _view;
        LobbiesService.LobbiesServiceClient _client;
        AsyncServerStreamingCall<LobbyEvent> _subscribedLobbyCall;

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
            }
        }
    }
}
