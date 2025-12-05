namespace _1_Lock
{
    internal class Counter
    {
        public int Value => _value;

        int _value;

        readonly object _gate = new object();

        public void Increment_ThreadUnsafe()
        {
            _value++;
        }

        public void Increment_ThreadSafe()
        {
            // lock : Application 내에서 쓰레드 간 동기화를 하기위한 키워드
            // <Critical Section>
            lock (_gate)
            {
                _value++;
            }
            // </Critical Section>

/*
* while 루프로 대기 (Spinlock) : busy-wait, 현재 쓰레드를 CPU가 계속 바쁘게 연산하면서 기다림
* lock : lock object를 확인해서 경쟁상태가 발생하지 않으면 그냥 패스,
* 스케쥴링의 우선순위 뒤로 밀어내기위해 기다려야하는 쓰레드로 분류하도록 한다.
*/
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            int n = 100_000;
            Counter counter_threadUnsafe = new Counter();
            Thread t1 = new Thread(() =>
            {
                for (int i = 0; i < n; i++)
                    counter_threadUnsafe.Increment_ThreadUnsafe();
            });
            Thread t2 = new Thread(() =>
            {
                for (int i = 0; i < n; i++)
                    counter_threadUnsafe.Increment_ThreadUnsafe();
            });

            Counter counter_threadSafe = new Counter();
            Thread t3 = new Thread(() =>
            {
                for (int i = 0; i < n; i++)
                    counter_threadSafe.Increment_ThreadSafe();
            });
            Thread t4 = new Thread(() =>
            {
                for (int i = 0; i < n; i++)
                    counter_threadSafe.Increment_ThreadSafe();
            });

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();

            Console.WriteLine($"Increment_threadUnsafe 결과는 : {counter_threadUnsafe.Value}, 기댓값 : {n * 2}");
            Console.WriteLine($"Increment_threadSafe 결과는 : {counter_threadSafe.Value}, 기댓값 : {n * 2}");
        }
    }
}
