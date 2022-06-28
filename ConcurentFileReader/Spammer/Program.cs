using System;
using System.IO;
using System.Threading;

namespace Spammer
{
    class Program
    {
        private static string inputFolder = @"D:\Projects\FileReader\InputFolder";
        private static int testDuration = 60000 * 10;

        static void Main()
        {
            var random = new Random();

            while (testDuration >= 0)
            {
                var fileName = Guid.NewGuid();
                
                File.WriteAllText($@"{inputFolder}\{fileName}.txt", "Testing");
                var randomSleepDuration = random.Next(100,500);
                testDuration -= randomSleepDuration;
                Thread.Sleep(randomSleepDuration);
            }

            Console.WriteLine("Finished");
        }
    }
}