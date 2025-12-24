using Game.Net.Chat;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Threading.Tasks;
using Net.Lobbies;

namespace Game.Net
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Kestrel : 
            builder.WebHost.ConfigureKestrel(option =>
            {
                // gRPC 는 Http2 프로토콜만지원
                option.ListenAnyIP(7777, listenOption => listenOption.Protocols = HttpProtocols.Http2);
            });

            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true; // 개발 디버깅용
            }); // Grpc server 사용
            builder.Services.AddGrpcReflection();
            builder.Services.AddSingleton<LobbiesManager>();
            var app = builder.Build();

            app.MapGrpcService<ChatServiceImpl>(); // 구현된 Grpc 서비스 등록 (Scope : Transient (요청시마다 인스턴스생성))
            app.MapGrpcService<LobbiesServiceImpl>();
            app.MapGrpcReflectionService();
            await app.RunAsync();
        }
    }
}
