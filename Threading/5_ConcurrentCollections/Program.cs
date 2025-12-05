using System.Collections.Concurrent;

namespace _5_ConcurrentCollections
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ConcurrentBag
            // 쓰레드별 리스트를 링크드리스트로 관리
            // 쓰레드별 리스트는 4.x (.net framwork) 는 double-linkedlist
            //                 5.x 이상 (.net core) 는 queue
            ConcurrentBag<int> bag = new ConcurrentBag<int>();
            bag.Add(1);
            bag.Add(2);

            // 순서를 보장받지못하는 무작위 아이템 반환
            // TryTake 를 호출한 쓰레드에 대한 노드(리스트) 가 있으면 거기서 가장 최근것을 반환
            // 없으면 다른 쓰레드리스트에서 가져옴
            if (bag.TryTake(out int result))
            {
            }

            // Thread-safe 한 Queue.
            // 생산자-소비자 큐 패턴시 편하게 가져다쓸수있음
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            queue.Enqueue(1);

            if (queue.TryDequeue(out int value))
            {

            }

            ConcurrentDictionary<int, string> dictionary = new ConcurrentDictionary<int, string>();
            dictionary.AddOrUpdate(103230, "Client 1", (key, value) =>
            {
                return "Client 2";
            });

            string name = dictionary[103230];
        }
    }

    public class ConcurrentList<T>
    {
        public class Node
        {
            public string ThreadName;
            public Queue<T> Queue;
            public Node Prev;
            public Node Next;
        }

        T[] _items;
        int _size;
        readonly object s_gate = new object();
        Node _entry;

        public void Add(T item)
        {
            Node node = GetNode();
            node.Queue.Enqueue(item);
        }

        Node GetNode()
        {
            Node tmp = _entry;

            while (tmp != null)
            {
                if (tmp.ThreadName == Thread.CurrentThread.Name)
                    return tmp;

                tmp = tmp.Next;
            }

            lock (s_gate)
            {
                tmp = _entry.Prev = new Node()
                {
                    ThreadName = Thread.CurrentThread.Name,
                    Queue = new Queue<T>(),
                    Next = _entry
                };
            }

            return tmp;
        }


        public void RemoveAt(int index)
        {
            lock (s_gate)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
                _items[_size - 1] = default;
                _size--;
            }
        }

        public int FindIndex(T item)
        {
            Comparer<T> comparer = Comparer<T>.Default;

            lock (s_gate)
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    if (comparer.Compare(_items[i], item) == 0)
                        return i;
                }
            }

            return -1;
        }

        void EnsureCapacity()
        {
            if (_size < _items.Length)
                return;

            T[] newItems = new T[_items.Length * 2];
            Array.Copy(_items, newItems, _size);
            _items = newItems;
        }
    }
}