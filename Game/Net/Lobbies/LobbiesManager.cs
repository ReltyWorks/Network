using Game.Lobbies;
using Google.Protobuf.Collections;
using Grpc.Core;
using Net.Utils;
using System.Collections.Concurrent;

namespace Net.Lobbies
{
    public class LobbiesManager
    {
        private IdGenerator _idGenerator;
        readonly ConcurrentDictionary<int, Lobby> _lobbies = new();

        /// <summary>
        /// 로비 생성
        /// </summary>
        /// <param name="userId"> 생성하려는 자 </param>
        /// <param name="maxUser"> 최대 유저 허용수 </param>
        /// <returns></returns>
        public Lobby Create(string userId, int maxUser)
        {
            int lobbyId = _idGenerator.AssignId();

            if (lobbyId < 0)
                return null;

            Lobby lobby = new Lobby
            {
                Id = lobbyId,
                MasterUserId = userId,
                MaxUser = maxUser,
            };

            _ = lobby.TryJoin(userId);
            _lobbies[lobbyId] = lobby;
            return lobby;
        }

        public bool TryGetLobby(int lobbyId, out Lobby lobby)
        {
            return _lobbies.TryGetValue(lobbyId, out lobby);
        }

        /// <summary>
        /// 로비 참여
        /// </summary>
        /// <param name="lobbyId"> 참여할 로비 </param>
        /// <param name="userId"> 참여할 유저</param>
        /// <returns> success </returns>
        public bool JoinLobby(int lobbyId, string userId)
        {
            if (!TryGetLobby(lobbyId, out Lobby lobby))
                return false;

            if (!lobby.TryJoin(userId))
                return false;

            return true;
        }

        /// <summary>
        /// 로비 떠남
        /// </summary>
        /// <param name="lobbyId"> 떠나려는 로비 </param>
        /// <param name="userId"> 떠나려는 유저 </param>
        /// <returns> success </returns>
        public bool LeaveLobby(int lobbyId, string userId)
        {
            // 떠나려했으나 이미 로비 사라지고 없음
            if (!TryGetLobby(lobbyId, out Lobby lobby))
                return false;

            if (!lobby.TryLeave(userId, out int remainMemberCount))
                return false;

            // 떠나고나니 남은 유저가 없다면 로비제거
            if (remainMemberCount == 0)
            {
                if (_lobbies.Remove(lobbyId, out _))
                    _idGenerator.ReleaseId(lobbyId);
            }

            return true;
        }

        public bool SetCustomProperties(int lobbyId, IDictionary<string, string> kv)
        {
            if (!_lobbies.TryGetValue(lobbyId, out Lobby lobby))
                return false;

            if (!lobby.TrySetCustomProperties(kv))
                return false;

            return true;
        }

        public bool SetUserCustomProperties(int lobbyId, string userId, IDictionary<string, string> kv)
        {
            if (!_lobbies.TryGetValue(lobbyId, out Lobby lobby))
                return false;

            if (!lobby.TrySetMemberCustomProperties(userId, kv))
                return false;

            return true;
        }

        public Lobby.Subscriber Subscribe(int lobbyId, IServerStreamWriter<LobbyEvent> serverStreamWriter)
        {
            if (!_lobbies.TryGetValue(lobbyId, out Lobby lobby))
                return null;

            return lobby.Subscribe(serverStreamWriter);
        }

        /// <summary>
        /// 로비의 모든 유저에게 이벤트를 통지
        /// </summary>
        public void Broadcast(int lobbyId, LobbyEvent e)
        {
            if (!_lobbies.TryGetValue(lobbyId, out Lobby lobby))
                return;

            lobby.Broadcast(e);
        }

        public RepeatedField<LobbyInfo> GetLobbyInfos()
        {
            return new RepeatedField<LobbyInfo> { _lobbies.Values.Select(lobby => lobby.ToLobbyInfo()) };
        }
    }
}
