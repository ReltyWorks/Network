namespace _2_AsyncQuiz
{
    internal class Barista
    {
        public async Task<Coffee> PourCoffee()
        {
            Coffee coffee = new Coffee();

            Console.WriteLine("커피 내리는 중...");

            await Task.Delay(1000);

            Console.WriteLine("커피 준비 완료!");

            return coffee;
        }
    }

    internal class Coffee { }
}
