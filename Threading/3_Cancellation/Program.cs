using System;

namespace _3_Cancellation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // CancellationTokenSource : 작업관리자
            // CancellationTokenSource.CancellationToken : 작업관리자의 신호를 받는 무전기

            // CancellationTokenSource 를 생성시에 TimeoutDelay 를 설정하여 직접 Cancel 호출하지않아도 타이머로 호출되게 할수있다.
            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                    if (Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("다운로드 취소 요청 감지 : 작업을 취소합니다..");
                        cts.Cancel();
                    }
            });

            try
            {
                Task<bool> downloadTask = FakeDownloadAsync(cts.Token);
                Task timeout = Task.Delay(5000);

                // Task.WhenXXX 는 비동기용
                // Task.WaitXXX 는 동기용
                int index = Task.WaitAny(new Task[] { downloadTask, timeout }, cts.Token);

                if (index == 0)
                {
                    if (downloadTask.IsCanceled == false)
                        Console.WriteLine("다운로드 완료");
                }
                else if (index == 1)
                {
                    Console.WriteLine("다운로드 실패. 응답 시간 초과");
                }
                else
                {
                    throw new NotImplementedException();
                }

                downloadTask.Wait();
            }
            catch (AggregateException agrregateEx)
            {
                if (agrregateEx.InnerException is TaskCanceledException)
                {
                    Console.WriteLine("다운로드 취소됨");
                }
            }
        }

        static async Task<bool> FakeDownloadAsync(CancellationToken cancellationToken)
        {
            int resourceCount = 24;
            Random random = new Random();

            for (int i = 0; i < resourceCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DrawProgressBar(i / (float)resourceCount);
                await Task.Delay(random.Next(250, 1000));
            }

            Console.WriteLine($"리소스 다운로드 중 ... {100} %");

            return true;
        }

        static void DrawProgressBar(float progress)
        {
            Console.Clear();
            int resolution = 10;

            for (int i = 0; i < resolution; i++)
            {
                char c = i <= progress * resolution ? '■' : '□';
                Console.Write(c);
            }
            Console.WriteLine();
            Console.WriteLine($"리소스 다운로드 중 ... {progress * 100} %");
        }
    }
}