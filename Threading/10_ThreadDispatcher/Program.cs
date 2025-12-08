using System.Diagnostics.SymbolStore;

namespace _10_ThreadDispatcher
{
    internal class Program
    {
        static SynchronizationContext _uiContext;

        static void Main(string[] args)
        {
            var dispather = new ThreadDispatcher();

            // engine- loop
            while (true)
            {
                // update~

                // fixedupdate~ 

                dispather.Exec();
            }

            //----------------------------------------------------
            _uiContext = SynchronizationContext.Current;

            Task downloadTask = Task.Run(FakeDownloadAsync);
            downloadTask.Wait();
        }

        static async Task FakeDownloadAsync()
        {
            await Task.Delay(2000);
            _uiContext.Post(_ =>
            {
                PopupAlert("다운로드 완료 !");
            }, null);
        }

        static void PopupAlert(string content)
        {
            Console.WriteLine(content); // UI text 갱신 시뮬레이션
        }
    }
}