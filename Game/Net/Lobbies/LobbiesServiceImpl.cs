using Game.Lobbies;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Net.Lobbies
{
    public class LobbiesServiceImpl : LobbiesService.LobbiesServiceBase
    {
        public LobbiesServiceImpl(LobbiesManager manager)
        {
            _manager = manager;
        }

        LobbiesManager _manager;

        public override async Task<CreateLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            try
            {
                Lobby lobby = _manager.Create(request.UserId, request.MaxClient);
                bool success = lobby != null;

                if (success)
                {
                    return new CreateLobbyResponse
                    {
                        Success = success,
                        LobbyInfo = lobby.ToLobbyInfo(),
                        UserInLobbyInfo = lobby.GetUserInfo(request.UserId)
                    };
                }
                else
                {
                    return new CreateLobbyResponse
                    {
                        Success = success,
                        LobbyInfo = new LobbyInfo
                        {
                            LobbyId = -1,
                            MaxClient = -1,
                            NumClient = -1,
                            MasterUserId = "Failed",
                            CustomProperties = { }
                        },
                        UserInLobbyInfo = { }
                    };
                }
            }
            catch (Exception ex)
            {
                return new CreateLobbyResponse
                {
                    Success = false,
                    LobbyInfo = new LobbyInfo
                    {
                        LobbyId = -1,
                        MaxClient = -1,
                        NumClient = -1,
                        MasterUserId = "Server internal error",
                        CustomProperties = { }
                    },
                    UserInLobbyInfo = { }
                };
            }
        }

        public override async Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            try
            {
                bool success = _manager.JoinLobby(request.LobbyId, request.UserId);

                if (success)
                {
                    if (_manager.TryGetLobby(request.LobbyId, out var lobby))
                    {
                        LobbyInfo lobbyInfo = lobby.ToLobbyInfo();
                        UserInLobbyInfo userInfo = lobby.GetUserInfo(request.UserId);
                        RepeatedField<UserInLobbyInfo> userInfos = lobby.GetUserInfos();

                        _manager.Broadcast(lobby.Id, new LobbyEvent
                        {
                            Type = LobbyEvent.Types.EventType.MemberJoin,
                            LobbyId = lobbyInfo.LobbyId,
                            UserId = userInfo.UserId,
                            Kv = { userInfo.CustomProperties },
                            Ts = Timestamp.FromDateTime(DateTime.UtcNow)
                        });

                        var response = new JoinLobbyResponse
                        {
                            Success = success,
                            LobbyInfo = lobbyInfo,
                            UserInLobbyInfos = { userInfos }
                        };

                        return response;
                    }
                }

                return new JoinLobbyResponse { Success = false };
            }
            catch (Exception ex)
            {
                return new JoinLobbyResponse { Success = false };
            }
        }
    }
}
