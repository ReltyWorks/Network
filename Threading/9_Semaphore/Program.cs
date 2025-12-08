/*
 * Semaphore
 * 얘도 Mutex 처럼 Kernel 레벨 동기화.
 * N 개의 쓰레드가 동시 접근제한.
 * Semaphore 는 점유하지않은 스레드도 해제 요청이 가능하다 (소유권의 개념이 없다)
 */
namespace _9_Semaphore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                new Thread(Work)
                    .Start(i);
            }
        }

        static void Work(object boxedId)
        {
            int id = (int)boxedId;
            using Semaphore semaphore = new Semaphore(2, 2, @"Local\SemaphoreDemo");
            Console.WriteLine($"ID {id} : Semaphore 대기");
            semaphore.WaitOne();
            Console.WriteLine($"ID {id} : Semaphore 점유");
            Thread.Sleep(2000);
            semaphore.Release();
            Console.WriteLine($"ID {id} : Semaphore 해제");
        }
    }
}