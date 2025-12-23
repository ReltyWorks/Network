using UnityEngine;
using Grpc.Core;
using Grpc.Net.Client;
using Cysharp.Net.Http;
using System;

namespace Game
{
    public static class GrpcConnection
    {
        public static GrpcChannel channel
        {
            get
            {
                if (!s_isInitialized)
                    InitChannel();

                return s_channel;
            }
        }

        private static GrpcChannel s_channel;
        private static bool s_isInitialized;

        private static void InitChannel()
        {
            var connectionSettings = Resources.Load<ConnectionSettings>("GrpcConnectionSettings");
            string url = $"http://{connectionSettings.ServerIp}:{connectionSettings.ServerPort}";

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var handler = new YetAnotherHttpHandler
            {
                Http2Only = true,
                SkipCertificateVerification = true, // 개발용
            };
            s_channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpHandler = handler,
                Credentials = ChannelCredentials.Insecure, // 개발용 (비암호화 연결)
                DisposeHttpClient = true,
            });

            s_isInitialized = true;
        }
    }
}