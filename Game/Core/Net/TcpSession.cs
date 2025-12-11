using System.Collections.Concurrent;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;

namespace Core.Net
{
    /// <summary>
    /// TCP 통신 소켓의 생명주기 및 소켓을 통한 데이터 송수신 처리
    /// </summary>
    public abstract class TcpSession : IDisposable
    {
        public TcpSession(Socket socket, int recvBufferSize = DEFAULT_RECV_BUFFER)
        {
            Socket = socket;
            _recvBuffer = new byte[recvBufferSize];
            _sendQueue = new ConcurrentQueue<ArraySegment<byte>>(); // TODO : Reserving
            _sendArgs = new SocketAsyncEventArgs();
            _sendArgs.Completed += OnSendCompleted;
        }


        const int KB = 1_024;
        const int HEADER_SIZE = sizeof(ushort);
        const int MAX_PACKET_SIZE = 1 * KB;
        const int DEFAULT_RECV_BUFFER = 4 * KB;

        public bool IsConnected => Socket.Connected;

        protected Socket Socket;

        // Send
        ConcurrentQueue<ArraySegment<byte>> _sendQueue;
        SocketAsyncEventArgs _sendArgs;
        readonly object _sendGate = new object();

        // Receive
        byte[] _recvBuffer;
        int _recvBufferCount;

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;


        public void Start()
        {
            OnConnected?.Invoke();
            _ = RecvLoopAsync();
        }


        public void Send(byte[] body)
        {
            int total = HEADER_SIZE + body.Length;
            byte[] segment = new byte[total];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)body.Length), 0, segment, 0, HEADER_SIZE);
            Buffer.BlockCopy(body, 0, segment, HEADER_SIZE, body.Length);
            _sendQueue.Enqueue(new ArraySegment<byte>(segment, 0, segment.Length));
            Send();
        }

        void Send()
        {
            lock (_sendGate)
            {
                if (_sendQueue.TryDequeue(out var segment) == false)
                    return;

                _ = SendAsync(segment);
            }
        }

        async Task SendAsync(ArraySegment<byte> segment)
        {
            int sentTotal = 0; // 실제 전송된 길이

            // Socket.Send 요청에 넣은 버퍼데이터가 반드시 전송 보장이되는것이 아니기 때문에, 
            // OS 가 실제로 얼마만큼 보냈는지 추적하면서 완전히 전송을 보장해주는 구문을 써야한다.
            while (sentTotal < segment.Count)
            {
                var tcs = new TaskCompletionSource<int>();
                _sendArgs.SetBuffer(segment.Array, sentTotal, segment.Count - sentTotal);
                _sendArgs.UserToken = tcs;
                bool pending = Socket.SendAsync(_sendArgs);

                if (pending == false)
                {
                    OnSendCompleted(this, _sendArgs);
                }

                int sent = await tcs.Task;

                // 전송 실패시 닫음
                if (sent <= 0)
                {
                    CloseSocket();
                    return;
                }

                segment = new ArraySegment<byte>(segment.Array, segment.Offset + sent, segment.Count - sent);
                sentTotal += sent;
            }

            Send();
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            var userToken = (TaskCompletionSource<int>)args.UserToken;

            if (args.SocketError == SocketError.Success)
            {
                userToken.SetResult(args.BytesTransferred); // 실제 전송된 바이트수
            }
            else
            {
                userToken.SetResult(0);
            }
        }

        async Task RecvLoopAsync()
        {
            while (IsConnected)
            {
                ArraySegment<byte> remainBufferSegment = new ArraySegment<byte>(_recvBuffer, _recvBufferCount, _recvBuffer.Length - _recvBufferCount);

                int read = await Socket.ReceiveAsync(remainBufferSegment, SocketFlags.None);

                // 연결에 문제있음
                if (read <= 0)
                {
                    CloseSocket();
                    break;
                }

                _recvBufferCount += read;
                ParsePacket();
            }
        }

        /// <summary>
        /// 현재 RecvBuffer 에 쌓인 모든 패킷을 다 처리 하고
        /// 남은 데이터 앞으로 밀착
        /// </summary>
        void ParsePacket()
        {
            int offset = 0; // RecvBuffer 현재 탐색 인덱스

            while (true)
            {
                // 헤더 길이도 안되는 데이터는 파싱이 안되니까 데이터가 더 쌓일때까지 기다려야함
                if (_recvBufferCount - offset < HEADER_SIZE)
                    break;

                ushort bodyLength = BitConverter.ToUInt16(_recvBuffer, offset);

                // 유효한 데이터인지
                if (bodyLength <= 0 || bodyLength > MAX_PACKET_SIZE)
                    return;

                // body 가 완전히 다 도착했는지
                if (_recvBufferCount - offset - HEADER_SIZE < bodyLength)
                    break;

                byte[] body = new byte[bodyLength];
                Buffer.BlockCopy(_recvBuffer, offset + HEADER_SIZE, body, 0, bodyLength);
                OnPacket(body);
                offset += HEADER_SIZE + bodyLength;
            }

            // 처리하고 남은 데이터 앞으로 옮김
            Buffer.BlockCopy(_recvBuffer, offset, _recvBuffer, 0, _recvBufferCount - offset);
            _recvBufferCount -= offset;
        }

        protected abstract void OnPacket(byte[] body);


        public void CloseSocket()
        {
            try
            {
                Socket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // Nothing to do ...
            }

            try
            {
                Socket?.Close();
            }
            catch
            {
                // Nothing to do ...
            }

            Socket = null;
            OnDisconnected?.Invoke();
        }

        public void Dispose()
        {
            Socket.Dispose();
            OnDisconnected?.Invoke();
        }
    }
}
