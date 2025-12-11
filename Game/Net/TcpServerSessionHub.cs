using Core.Net;
using Game.NetworkContracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Net
{
    public class TcpServerSessionHub
    {
        public TcpServerSessionHub(int capacity)
        {
            _sessions = new ConcurrentDictionary<int, TcpServerSession>(Environment.ProcessorCount, capacity);
        }


        public IEnumerable<TcpServerSession> All => _sessions.Values;

        ConcurrentDictionary<int, TcpServerSession> _sessions;

        public event Action<int, IPacket> OnPacketReceived;

        public void Add(TcpServerSession session)
        {
            int clientId = session.ClientId;

            if (_sessions.TryAdd(clientId, session))
            {
                session.OnPacketReceived += OnPacketReceived;
                session.OnDisconnected += () => Remove(session.ClientId);
                Console.WriteLine($"클라이언트 {clientId} 관리 세션 등록됨");
            }
            else
            {
                throw new Exception($"ClientId 가 중복 발급됨..");
            }
        }

        public void Remove(int clientId)
        {
            if (_sessions.TryRemove(clientId, out _))
            {
                // Nothing to do...
                Console.WriteLine($"클라이언트 {clientId} 관리 세션 제거됨");
            };
        }

        public void Broadcast(IPacket packet)
        {
            foreach (var session in _sessions.Values)
                session.Send(packet);
        }

        public void BroadcastToOthers(IPacket packet, int clientIdExclusive)
        {
            foreach (var session in _sessions)
                if (session.Key != clientIdExclusive)
                    session.Value.Send(packet);
        }
    }
}
