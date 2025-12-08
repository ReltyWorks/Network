using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace _6_ValueTaskBenchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //| Method             | Mean      | Error     | StdDev    |
            //| ------------------ | ---------:| ---------:| ---------:|
            //| WorkWithTask       | 14.54 ns  | 0.048 ns  | 0.040 ns  |
            //| WorkWithValueTask  | 12.51 ns  | 0.039 ns  | 0.037 ns  |
            BenchmarkRunner.Run<Benchmarks>();

            // ValueTask 가 항상 힙할당을 하게되었을때 :
            //| Method | Mean | Error | StdDev |
            //| ------------------ | --------:| ---------:| ---------:|
            //| WorkWithTask | 1.532 s | 0.0031 s | 0.0028 s |
            //| WorkWithValueTask | 1.530 s | 0.0051 s | 0.0048 s |
        }
    }

    public class Benchmarks
    {
        [Benchmark] public Task<int> WorkWithTask() => Workload.ComputeWithTask();
        [Benchmark] public ValueTask<int> WorkWithValueTask() => Workload.ComputeWithValueTask();
    }

    public static class Workload
    {
        static bool s_isDoneWithTask;
        static bool s_isDoneWithValueTask;
        static int s_resultWithTask;
        static int s_resultWithValueTask;

        public static async Task<int> ComputeWithTask()
        {
            if (s_isDoneWithTask)
                return s_resultWithTask;

            int result = 0;

            for (int i = 1; i < 100; i++)
            {
                result += i;
                await Task.Delay(10);
            }

            s_resultWithTask = result;
            s_isDoneWithTask = true;
            return result;
        }

        public static async ValueTask<int> ComputeWithValueTask()
        {
            if (s_isDoneWithValueTask)
                return s_resultWithValueTask;

            int result = 0;

            for (int i = 1; i < 100; i++)
            {
                result += i;
                await Task.Delay(10);
            }

            s_resultWithValueTask = result;
            s_isDoneWithValueTask = true;
            return result;
        }
    }
}