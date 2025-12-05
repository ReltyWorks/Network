namespace _3_MonitorSignal
{
    internal class Program
    {
        static readonly Queue<int> s_item = new Queue<int>();
        static readonly object s_gate = new object();
        static bool s_isSending;

        static void Main(string[] args)
        {
            s_isSending = true;
            Thread[] sends = new Thread[10];

            for(int i = 0; i < 10; i++)
            {
                sends[i] = new Thread(Send);
                sends[i].Start(i);
            }

            for(int i = 0;i < 10; i++)
            {
                sends[i].Join();
            }

            s_isSending = false;

            lock(s_gate)
            {
                Monitor.PulseAll(s_gate); // Wait로 대기중인 모든 쓰레드 깨움
            }
        }

        static void Send(object itemObject)
        {
            int item = (int)itemObject;

            lock (s_gate)
            {
                s_item.Enqueue(item);
                Console.WriteLine($"Send : {item}");
                Monitor.Pulse(s_gate); // Wait로 대기중인 쓰레드 한개 깨움
                // CriticalSection 처리했으니까 임시 대기해줬던 Recv 쓰레드에게 자원 다시 써도 된다고 알려줌
            }
        }

        static void Recv()
        {
            while (true)
            {
                lock (s_gate)
                {
                    // 송신 작업 중인데 아직 아이템 송신 안됨
                    if (s_item.Count == 0 && s_isSending == true)
                        Monitor.Wait(s_gate); // 이 쓰레드가 s_gate 자원을 대기하는 쓰레드가 됨

                    // 수신 작업 완료
                    if (s_item.Count == 0 && s_isSending == false)
                    break;

                    if (s_item.Count == 0)
                        continue;

                    int item = s_item.Dequeue();
                    Console.WriteLine($"Recv : {item}");
                }
            }
        }
    }
}
