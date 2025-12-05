namespace Unit1_Thread
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread t1 = new Thread(Sum)
            {
                Name = "Worker - 1",
                IsBackground = true, // 현재 프로세스의 Forground thread count 확인하여 0이면 해제 호출
            };
            t1.Start(1_000_100);

            Thread t2 = new Thread(Sum)
            {
                Name = "Worker - 2",
                IsBackground = true,
            };
            t2.Start(1_000_000);

            t2.Join(); // t1 인스턴스가 나타내는 스레드가 종료될때까지 호출쓰레드 차단
            t1.Join();

            Console.WriteLine($"[Main Thread] Finished");
        }

        static void Sum(object limitObj)
        {
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Start sum");

            int limit = (int)limitObj;
            long result = 0;

            for (int i = 0; i < limit; i++)
            {
                result += i;
            }

            Thread.Sleep(1000);
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Sum 0 ~ {limit} = {result}");
        }
    }
}
