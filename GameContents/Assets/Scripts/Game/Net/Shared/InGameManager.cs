/*
 * Unity.NetCode.NetworkBehaviour :
 * 
 * 네트워크 동기화되어야하는 GameObject 에는 netcode for gameobject 에서
 * NetworkObject 컴포넌트를 가져야함.
 * 
 * NetworkObject 에 대한 행동을 정의하는 베이스클래스
 */
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer;
using System.Collections.Generic;
using Unity.Collections;

namespace Game.Net.Shared
{
    public enum InGameState
    {
        None,
        WaitUntilAllClientsAreConntected,
        StartContent,
        WaitUntilContentFinished,
    }

    /// <summary>
    /// 기본 C# Unmanaged primitive 들과 Unity 에서 제공해주는 Unmanaged built-in 타입들 외에 
    /// 나만의 동기화 데이터 형태가 필요하면 INetworkSerializable 을 상속받아서 만들어주면된다.
    /// TODO : LobbyInfo 메세지와 동기화해야함.
    /// </summary>
    public struct MatchInfo : INetworkSerializable
    {
        public int clientCount;
        public FixedString64Bytes title; // 문자열 동기화 필요하면 고정크기배열 문자열써야함.

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientCount);
            serializer.SerializeValue(ref title);
        }
    }


    public class InGameManager : NetworkBehaviour
    {
        public static InGameManager singleton;

        public NetworkVariable<InGameState> state = new NetworkVariable<InGameState>(
                value: InGameState.None,
                readPerm: NetworkVariableReadPermission.Everyone,
                writePerm: NetworkVariableWritePermission.Server
            );

        public NetworkVariable<MatchInfo> matchInfo = new NetworkVariable<MatchInfo>(
                value: default,
                readPerm: NetworkVariableReadPermission.Everyone,
                writePerm: NetworkVariableWritePermission.Server
            );

        public NetworkList<ulong> connectedClientIds = new NetworkList<ulong>(
                values: null,
                readPerm: NetworkVariableReadPermission.Everyone,
                writePerm: NetworkVariableWritePermission.Server
            );


        private void Awake()
        {
            singleton = this;
        }
        /*
         * MultiplayerRoleFlag.Client / Server 는 컴파일할때 초기화값 결정해놓는것임
         * NetworkBehaviour 의 IsServer ,IsClient 는 NetworkManager.StartServer() / StartClient() 호출 이후 (네트워크동기화 이후) 결정되는것임
         */
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log("Spawned InGameManager.");

            if (IsServer)
            {
                matchInfo.Value = new MatchInfo
                {
                    clientCount = 2,
                    title = "Noobs only"
                };
                state.Value = InGameState.WaitUntilAllClientsAreConntected;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        void OnClientConnected(ulong clientId)
        {
            connectedClientIds.Add(clientId);

            if (state.Value == InGameState.WaitUntilAllClientsAreConntected &&
                connectedClientIds.Count == matchInfo.Value.clientCount)
            {
                state.Value = InGameState.StartContent;
            }
        }

        void OnClientDisconnected(ulong clientId)
        {
            connectedClientIds.Remove(clientId);
        }
    }
}