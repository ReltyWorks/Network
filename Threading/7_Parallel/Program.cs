/*
 * for/ foreach 같은 반복 작업을 여러 쓰레드로 나누어서 병렬처리하기위한 클래스
 */
namespace _7_Parallel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int N = 10_000_000;
            long sum1 = 0;

            // for
            //-------------------------------------
            for (int i = 0; i < N; i++)
            {
                sum1 += i;
            }

            object gate = new object();

            Parallel.For(0, N, i =>
            {
                lock (gate)
                {
                    sum1 += i;
                }
            });

            // foreach
            //-------------------------------------
            List<int> list = new List<int>()
            {
                5, 2, 4, 1, 3
            };

            foreach (var item in list)
            {

            }

            Parallel.ForEach(list, item =>
            {

            });

            // invoke
            //-------------------------------------

            Parallel.Invoke(
                () =>
                {
                    // Action 1
                },
                () =>
                {
                    // Action 2
                },
                () =>
                {
                    // Action 3
                }
                );
        }
    }
}