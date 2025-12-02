namespace Unit2_AsyncBasics
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread thread = new Thread(() =>
            {
                FakeDownload("File A", 2500);
            })
            {
                Name = "File A Download thread"
            };

            thread.Start();
            thread.Join();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                FakeDownload("File B", 2500);
            });

            Task task = new Task(() =>
            {
                FakeDownload("File C", 2500);
            });
            task.Start();

            Task taskRun = Task.Run(() => FakeDownload("File D", 2500));

            taskRun.Wait();
        }

        static void FakeDownload(string resourceName, int simulationTimeMS)
        {
            Console.WriteLine($"{resourceName} 다운로드 시작...");

            Thread.Sleep(simulationTimeMS);

            Console.WriteLine($"{resourceName} 다운로드 완료.");
        }
    }
}
