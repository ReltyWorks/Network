/*
Interlocked 클래스
시스템의 원자연산을 매핑한 클래스
하드웨어 수준에서 CPU가 단일 명령으로 실행하는 최소 연산 단위이기 때문에
다른 쓰레드가 연산 중간에 개입 할 수가 없다.

원자연산은 CPU 워드크기 까지만 가능
32 bit system -> 32 bit 까지만 원자연산 됨
*/
namespace _4_Interlock
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Workload workload = new Workload();
            Thread t1 = new Thread(workload.ComputeWithInterlocked);
            Thread t2 = new Thread(workload.ComputeWithInterlocked);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Console.WriteLine($"결과값 : {Workload.Accumulate}, 기댓값 : {Workload.N * 2}");
        }
    }

    internal class Workload
    {
        public const int N = 5_000_000;
        public static long Accumulate = 0;
        

        public void ComputeWithInterlocked()
        {
            for (int i = 0; i < N; i++)
            {
                Interlocked.Increment(ref Accumulate);
            }
        }
    }
}
