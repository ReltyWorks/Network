namespace Unit1_Thread
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread t1 = new Thread(Sum) { Name = "Worker1", IsBackground = true };
            t1.Start(10_000_000);

            Thread t2 = new Thread(Sum) { Name = "Worker2", IsBackground = true };
            t2.Start(1_000);

            t2.Join(); // t1 인스턴스가 나타내는 스레드가 종료될때까지 호출 쓰레드 차단

            Console.WriteLine("완전 작업 끝남!");
        }

        static void Sum(object limitObj)
        {
            Console.WriteLine($"[{Thread.CurrentThread.Name}] 히히 작업 시작!");

            int limit = (int)limitObj;

            long result = 0;

            for (int i = 0; i < limit; i++)
                result += i;

            Thread.Sleep(1000); // 멈춤

            Console.WriteLine($"Sum 0 ~ {limit} = {result}");
            Console.WriteLine($"[{Thread.CurrentThread.Name}] 히히 작업 끝남!");
        }
    }
}
