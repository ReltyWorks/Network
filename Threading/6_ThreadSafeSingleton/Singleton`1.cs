namespace _6_ThreadSafeSingleton
{
    internal class Singleton<T> where T : Singleton<T>
    {
        public static T Instance
        {
            get
            {
                // 컴파일러 최적화에 의해서 이 공유된 변수값을 다른 캐싱된 값으로 치환하고,
                // 다른 쓰레드가 그 값을 읽으면 문제가 될 수 있기 때문에
                // 컴파일러 최적화하지말라고 volatile 붙이고
                // lock 걸기전에 확인하면
                // 할당된 이후에도 매번 lock을 점유하는 로직을 실행할 필요 없다.

                lock(s_gate)
                {
                    if (s_instance == null)
                        s_instance = Activator.CreateInstance<T>();
                }

                return s_instance;
            }
        }
        
        // volatile - 컴파일러에게 최적화하지 말라고 하는 키워드

        private static volatile T s_instance;
        private static readonly object s_gate = new object();
    }
}
