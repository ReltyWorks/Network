using Unity.Multiplayer;
using Unity.Netcode;
using UnityEngine;

namespace Game.Net.Shared
{
    public class InGameBootstrap : MonoBehaviour
    {
        NetworkManager _networkManager;


        private void Start()
        {
            _networkManager = NetworkManager.Singleton;
            bool isClient = MultiplayerRolesManager.ActiveMultiplayerRoleMask.HasFlag(MultiplayerRoleFlags.Client);
            bool isServer = MultiplayerRolesManager.ActiveMultiplayerRoleMask.HasFlag(MultiplayerRoleFlags.Server);

            if (isClient)
            {
                _networkManager.StartClient();
            }
            else if (isServer)
            {
                _networkManager.StartServer();
            }
            else
            {
                throw new System.ArgumentException(MultiplayerRolesManager.ActiveMultiplayerRoleMask.ToString());
            }
        }
    }
}