/*
 * Mutex 는 자원을 점유할 수 있는 쓰레드가 1개.
 * Mutex 는 자원을 점유한 쓰레드만 해제가 가능함.
 * 
 * lock 같은경우에는 공유자원을 힙에할당 (new object() .. ) 하고 스레드 간에 이 객체를 참조하여 동기화
 * Mutex 는 프로세스에 상관없이 동기화를 하는게 목적이기때문에 Kernel 레벨에 자원 관리 요청을하고 그 자원으로 동기화
 */
namespace _8_Mutex
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string id = args.Length > 0 ? args[0] : string.Empty;

            // Local : 현재 세션에서만 프로세스간 동기화
            // Global : 전체 세션에서만 프로세스간 동기화
            using Mutex mutex = new Mutex(false, @"Global\NamedMutex");
            Console.WriteLine($"ID {id} : 뮤텍스 요청");
            mutex.WaitOne();
            Console.WriteLine($"ID {id} : 뮤텍스 점유. 해제하려면 아무 키나 누르세요.");
            Console.ReadKey(intercept: true);
            mutex.ReleaseMutex();
            Console.WriteLine($"ID {id} : 뮤텍스 해제");
        }
    }
}