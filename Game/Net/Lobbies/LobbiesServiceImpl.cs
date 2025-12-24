using Game.Lobbies;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Net.Lobbies
{
    public class LobbiesServiceImpl : LobbiesService.LobbiesServiceBase
    {
        public LobbiesServiceImpl(ILogger<LobbiesServiceImpl> logger, LobbiesManager manager)
        {
            _logger = logger;
            _manager = manager;
        }

        ILogger<LobbiesServiceImpl> _logger;
        LobbiesManager _manager;

        public override async Task<CreateLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            try
            {
                _logger.Log(LogLevel.Information, "Begin CreateLobby");
                Lobby lobby = _manager.Create(request.UserId, request.MaxClient);
                bool success = lobby != null;

                if (success)
                {
                    _logger.Log(LogLevel.Information, "Succeeded CreateLobby");
                    return new CreateLobbyResponse
                    {
                        Success = success,
                        LobbyInfo = lobby.ToLobbyInfo(),
                        UserInLobbyInfo = lobby.GetUserInfo(request.UserId)
                    };
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Failed CreateLobby");
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
                _logger.Log(LogLevel.Error, "Failed CreateLobby");
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
                _logger.Log(LogLevel.Information, "Begin JoinLobby");
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

                        _logger.Log(LogLevel.Information, "Succeeded JoinLobby");
                        return response;
                    }
                }

                _logger.Log(LogLevel.Information, "Failed JoinLobby");
                return new JoinLobbyResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Failed JoinLobby");
                return new JoinLobbyResponse { Success = false };
            }
        }

        public override async Task<GetLobbyListResponse> GetLobbyList(Empty request, ServerCallContext context)
        {
            try
            {
                _logger.Log(LogLevel.Information, "Begin GetLobbyList");
                return new GetLobbyListResponse
                {
                    LobbyInfos = { _manager.GetLobbyInfos() }
                };
            }
            catch (Exception ex)
            {
                return new GetLobbyListResponse
                {
                    LobbyInfos = { }
                };
            }
            finally
            {
                _logger.Log(LogLevel.Information, "End GetLobbyList");
            }
        }

        public override async Task<LeaveLobbyResponse> LeaveLobby(LeaveLobbyRequest request, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, "Begin LeaveLobby");

            bool success = _manager.LeaveLobby(request.LobbyId, request.UserId);

            try
            {
                if (success)
                {
                    if (_manager.TryGetLobby(request.LobbyId, out Lobby lobby))
                    {
                        _manager.Broadcast(lobby.Id, new LobbyEvent
                        {
                            Type = LobbyEvent.Types.EventType.MemberLeft,
                            LobbyId = lobby.Id,
                            UserId = request.UserId,
                            Kv = { },
                            Ts = Timestamp.FromDateTime(DateTime.UtcNow)
                        });
                    }
                }

                _logger.Log(LogLevel.Information, "End LeaveLobby");

                return new LeaveLobbyResponse
                {
                    Success = success
                };
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Failed LeaveLobby");

                return new LeaveLobbyResponse
                {
                    Success = false
                };
            }
        }

        public override async Task<SetCustomPropertiesResponse> SetCustomProperties(SetCustomPropertiesRequest request, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, "Begin SetCustomProperties");

            try
            {
                if (_manager.TryGetLobby(request.LobbyId, out var lobby))
                {
                    if (lobby.TrySetCustomProperties(request.Kv))
                    {
                        lobby.Broadcast(new LobbyEvent
                        {
                            Type = LobbyEvent.Types.EventType.LobbyCustomPropChanged,
                            LobbyId = lobby.Id,
                            UserId = string.Empty,
                            Kv = { request.Kv },
                            Ts = Timestamp.FromDateTime(DateTime.UtcNow)
                        });

                        _logger.Log(LogLevel.Information, "Succeeded SetCustomProperties");
                        return new SetCustomPropertiesResponse
                        {
                            Success = true
                        };
                    }
                }

                _logger.Log(LogLevel.Information, "Failed SetCustomProperties");
                return new SetCustomPropertiesResponse
                {
                    Success = false
                };
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Failed SetCustomProperties");
                return new SetCustomPropertiesResponse
                {
                    Success = false
                };
            }
        }

        public override async Task<SetUserCustomPropertiesResponse> SetUserCustomProperties(SetUserCustomPropertiesRequest request, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, "Begin SetUserCustomProperties");

            try
            {
                if (_manager.TryGetLobby(request.LobbyId, out var lobby))
                {
                    if (lobby.TrySetMemberCustomProperties(request.UserId, request.Kv))
                    {
                        lobby.Broadcast(new LobbyEvent
                        {
                            Type = LobbyEvent.Types.EventType.UserCustomPropChanged,
                            LobbyId = lobby.Id,
                            UserId = request.UserId,
                            Kv = { request.Kv },
                            Ts = Timestamp.FromDateTime(DateTime.UtcNow)
                        });

                        _logger.Log(LogLevel.Information, "Succeeded SetUserCustomProperties");
                        return new SetUserCustomPropertiesResponse
                        {
                            Success = true
                        };
                    }
                }

                _logger.Log(LogLevel.Information, "Failed SetUserCustomProperties");
                return new SetUserCustomPropertiesResponse
                {
                    Success = false
                };
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Failed SetUserCustomProperties");
                return new SetUserCustomPropertiesResponse
                {
                    Success = false
                };
            }
        }

        public override async Task SubscribeLobby(SubscribeLobbyRequest request, IServerStreamWriter<LobbyEvent> responseStream, ServerCallContext context)
        {
            Lobby.Subscriber subscriber = null;

            try
            {
                _logger.Log(LogLevel.Information, "Begin SubscribeLobby");
                subscriber = _manager.Subscribe(request.LobbyId, responseStream);
                await subscriber.Completion;
            }
            catch (Exception ex)
            {
                if (subscriber != null)
                {
                    await subscriber.DisposeAsync();
                }
            }
        }
    }
}
