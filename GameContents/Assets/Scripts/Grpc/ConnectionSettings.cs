using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Grpc/ConnectionSettings")]
    public class ConnectionSettings : ScriptableObject
    {
        [field: SerializeField] public string ServerIp { get; private set; } = "127.0.0.1";
        [field: SerializeField] public int ServerPort { get; private set; } = 7777;
    }
}
