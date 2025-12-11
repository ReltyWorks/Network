using System.Net.Sockets;
using UnityEngine;
using Core.Net;
using Game.Core.Net;
using System.Text;
using System.Collections.Generic;

namespace Game.Net.Chat
{
    public class ChatController : MonoBehaviour
    {
        [SerializeField] ChatView _view;
        ChatClient _client;
        StringBuilder _historyBuilder;

        Socket socket;
        TcpClientSession session;

        private void Awake()
        {
            // TODO : Scoping..
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ConnectionSettings.ServerIp, ConnectionSettings.ServerPort);
            session = new TcpClientSession(socket);
            session.Start();

            _historyBuilder = new StringBuilder(); // TODO : Reserving
            _client = new ChatClient(session);
            _client.onRecvChatMessage += OnRecvMessage;
        }

        private void OnEnable()
        {
            _view.onTextSubmit += SendMessage;
        }

        private void OnDisable()
        {
            _view.onTextSubmit -= SendMessage;
        }

        private void OnDestroy()
        {
            session.CloseSocket();
        }

        public void SendMessage(string text)
        {
            Debug.Log("Sending.." + text);
            _client.SendChatMessage(text);
            _view.ClearText();
        }

        public void OnRecvMessage((int senderId, string text) message)
        {
            _historyBuilder.AppendLine($"{message.senderId} : {message.text}");
            _view.SetHistory(_historyBuilder.ToString());
        }
    }
}