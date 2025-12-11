using Core.Net;
using Game.Net.Chat;
using Game.NetworkContracts;
using System.Net;
using System.Net.Sockets;

namespace Game.Net
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(ListenClients)
                .Wait();
        }

        static async Task ListenClients()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();
            ClientIdGenerator idGenerator = new ClientIdGenerator();
            TcpServerSessionHub hub = new TcpServerSessionHub(100);
            ChatService chatService = new ChatService(hub);

            hub.OnPacketReceived += (senderId, packet) => Console.WriteLine($"클라이언트 {senderId} 에게서 패킷 {packet.PacketId} 받음 ");

            while (true)
            {
                Socket socket = await listener.AcceptSocketAsync();
                Console.WriteLine($"클라이언트 접속함 {socket.RemoteEndPoint}");
                int clientId = idGenerator.AssignClientId();
                TcpServerSession session = new TcpServerSession(clientId, socket);

                // 수용량 초과
                if (clientId < 0)
                {
                    session.Send(new S_ConnectionFailure()
                    {
                        Reason = "Server is fulled."
                    });

                    // TODO : Send 가 정상적으로 완료될때까지 기다렸다가 Session 을 닫아야함..
                    await Task.Delay(1000);
                    session.CloseSocket();
                    continue;
                }

                session.Send(new S_ConnectionSuccess()
                {
                    AssignedClientId = clientId,
                });

                session.Start();
                hub.Add(session);
            }
        }
    }
}
