namespace _2_Deadlock
{
    internal class Program
    {
        static readonly object key1 = new object();
        static readonly object key2 = new object();

        static void Main(string[] args)
        {
            Thread t1 = new Thread(() => Work(key1, key2));
            Thread t2 = new Thread(() => Work(key2, key1));

            t1.Start();
            t2.Start();

            Thread.Sleep(500);
        }

        static void Work(object gete1, object gete2)
        {
            lock (gete1)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} 게이트1 진입");

                Thread.Sleep(100);

                lock (gete2)
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} 작업 완료");
                }
            }
        }
    }
}
