// Channel : 생산자의 생산속도와 소비자의 소비속도 차이에 의한 병목현상을 핸들링하는 객체
namespace _4_ChannelPipeline
{
    internal class Program
    {
        const int TOTAL = 200; // 처리해야하는 아이템 갯수
        static readonly int s_bufferCapacity = 20;
        static Queue<int> s_buffer = new(s_bufferCapacity);
        static readonly object s_bufferGate = new object();
        static bool s_isProduceCompleted;

        static void Main(string[] args)
        {
            Task produce = Task.Run(ProduceAsync);
            Task consume1 = Task.Run(ConsumeAsync);
            Task consume2 = Task.Run(ConsumeAsync);
            Task.WaitAll(produce, consume1, consume2);
        }

        static async Task ProduceAsync()
        {
            for (int i = 0; i < TOTAL; i++)
            {
                await WaitUntilBufferHasSpaceAsync();

                lock (s_bufferGate)
                {
                    s_buffer.Enqueue(i);
                }

                Console.WriteLine($"[생산자] : 아이템 {i} 생산");
            }

            lock (s_bufferGate)
            {
                s_isProduceCompleted = true;
            }
        }

        static async Task ConsumeAsync()
        {
            while (true)
            {
                lock (s_bufferGate)
                {
                    if (s_buffer.Count > 0)
                    {
                        int item = s_buffer.Dequeue();
                        Console.WriteLine($"[소비자 {Thread.CurrentThread.Name}] : 아이템 {item} 소비 시작");
                        //await Task.Delay(100);
                        Console.WriteLine($"[소비자 {Thread.CurrentThread.Name}] : 아이템 {item} 소비 완료");
                    }
                    // 모든 아이템 소모 완료
                    else if (s_isProduceCompleted)
                    {
                        break;
                    }
                }
                //await Task.Delay(10);
            }
        }

        static async Task WaitUntilBufferHasSpaceAsync()
        {
            while (true)
            {
                lock(s_bufferGate)
                {
                    if (s_buffer.Count < s_bufferCapacity)
                        return;
                }

                //await Task.Delay(10);
            }
        }
    }
}