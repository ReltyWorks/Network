using System.Threading.Channels;

namespace _5_Channel
{
    internal class Program
    {
        const int TOTAL = 20;

        static void Main(string[] args)
        {
            Channel<int> channel = Channel.CreateBounded<int>(10);

            // 생산자 작업
            Task producer = Task.Run(async () =>
            {
                for (int i = 0; i < TOTAL; i++)
                {
                    await channel.Writer.WriteAsync(i);
                    channel.Writer.WriteAsync(i);
                }

                channel.Writer.Complete();
                Console.WriteLine($"[생산자] : 작업완료");
            });

            // 소비자 작업
            Task cosumer = Task.Run(async () =>
            {
                await foreach (int item in channel.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"[소비자] : {item} 소비시작");
                    await Task.Delay(100);
                    Console.WriteLine($"[소비자] : {item} 소비완료");
                }

                Console.WriteLine($"[소비자] : 작업완료");
            });

            Task.WaitAll(producer, cosumer);
        }
    }
}
