using System.Text;

namespace _2_AsyncQuiz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Barista barista = new Barista();
            Cook cook = new Cook();

            var task1 = barista.PourCoffee();
            Coffee morningCoffee = task1.Result;

            var task2 = cook.FryEgg();
            var task3 = cook.FryBacon();
            var task4 = cook.MakeToast();
            var task5 = cook.JamOnToast(task4.Result);

            EggFried morningEgg = task2.Result;
            BaconFried morningBacon = task3.Result;
            Toast morningToast = task5.Result;

            Console.WriteLine("식사 준비 완료!");
        }
    }
}