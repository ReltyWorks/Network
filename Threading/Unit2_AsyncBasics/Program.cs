namespace Unit2_AsyncBasics
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Action<bool> downloadCompletion = (bool success) =>
            {
                Console.WriteLine($"Success : {success}");
            };

            Thread thread = new Thread(() =>
            {
                bool success = FakeDownload("File A", 1500);
                downloadCompletion(success);
            })
            {
                Name = "File A Download thread"
            };

            thread.Start();
            thread.Join();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                FakeDownload("File A", 1500);
            });

            Task task = new Task(() =>
            {
                FakeDownload("File A", 1500);
            });
            task.Start();

            Task.Factory.StartNew(() => FakeDownload("File A", 1500));
            Task taskRun = Task.Run(() => FakeDownload("File A", 1500));
            taskRun.ContinueWith(task => FakeDownload("File B", 1500))
                .Wait();



            bool success = await FakeDownloadAsync("File B", 800);

            /*
            var machine = new FakeDownloadAsyncMachine();
            machine.MoveNext();
            */
        }

        static bool FakeDownload(string resourceName, int simulationTimeMS)
        {
            Console.WriteLine($"{resourceName} 다운로드 시작...");
            Thread.Sleep(simulationTimeMS);
            Console.WriteLine($"{resourceName} 다운로드 완료.");
            return true;
        }

        static async Task<bool> FakeDownloadAsync(string resourceName, int simulationTimeMS)
        {
            Console.WriteLine($"{resourceName} 다운로드 시작...");
            await Task.Delay(simulationTimeMS);
            Console.WriteLine($"{resourceName} 다운로드 완료.");
            return true;
        }

        /*
        class FakeDownloadAsyncMachine : IAsyncStateMachine
        {
            int _step;

            public void MoveNext()
            {
                if (_step ~~~)
                        return;

                switch (_step)
                {
                    case 0:
                        Console.WriteLine($"{resourceName} 다운로드 시작...");
                        Task.Delay(simulationTimeMS);
                        break;
                    case 1:
                        Console.WriteLine($"{resourceName} 다운로드 완료.");
                        break;
                    default:
                        break;
                }

                _step++;
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                throw new NotImplementedException();
            }
        }
        */

        /*
        static IEnumerable Enumerable()
        {
            yield return null;
        }

        static IEnumerator<int> Coroutine()
        {
            yield return 3;
            Console.WriteLine("Hi");
            yield return 4;
            Console.WriteLine("Hello");
            yield return 5;
            Console.WriteLine("How R U");
        }

        struct CoroutineEnumerator : IEnumerator<int>
        {
            public int Current => _current;

            object IEnumerator.Current => Current;
            int _current;
            int _step;

            public bool MoveNext()
            {
                if (_step~)
                        return false;

                switch (_step)
                {
                    case 0:
                        _current = 3;
                        break;
                    case 1:
                        Console.WriteLine("Hi");
                        _current = 4;
                        break;
                    case 2:
                        Console.WriteLine("Hello");
                        _current = 5;
                        break;
                    case 3:
                        Console.WriteLine("How R U");
                        break;
                    default:
                        break;
                }

                _step++;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }
        }
        */
    }
}