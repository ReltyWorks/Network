using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Diagnostics;

namespace Unit1_ThreadPool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
            return;

            // workerThreads : CPU-Bound thread 개수
            // completionPortThreads : I/O-Bound thread 개수
            ThreadPool.SetMinThreads(4, 4);
            ThreadPool.SetMaxThreads(4, 8);

            // ThreadPool 의 이용가능한 스레드가 확보될때
            // 해당 쓰레드에 작업을 할당할 대기열에 작업을 등록
            ThreadPool.QueueUserWorkItem(_ =>
            {

            });
        }

        static void Measure(string label, Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Stopwatch sw = Stopwatch.StartNew();
            action.Invoke();
            sw.Stop();

            Console.WriteLine($"{label} : {sw.ElapsedMilliseconds}");
        }
    }

    public static class Workload
    {
        public static void Compute()
        {
            int limit = 100_000;
            long result = 0;

            for (int i = 1; i < limit; i++)
            {
                result += i;
            }
        }
    }

    [SimpleJob]
    public class Benchmarks
    {
        [Benchmark] public void Workload_Thread() => Workload.Compute();
        [Benchmark] public void Workload_ThreadPool() => ThreadPool.QueueUserWorkItem(_ => Workload.Compute());
    }
}
