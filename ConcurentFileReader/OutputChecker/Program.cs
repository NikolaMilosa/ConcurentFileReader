using System;
using System.Collections.Generic;
using System.IO;

namespace OutputChecker
{
    class Program
    {
        private static string outputFolder = @"D:\Projects\FileReader\OutputFolder";
        static void Main(string[] args)
        {
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Output dir not found");
                return;
            }

            var dictionary = new Dictionary<string, int>();
            var dirInfo = new DirectoryInfo(outputFolder);

            var files = dirInfo.GetFiles();
            foreach (var file in files)
            {
                var actualFile = file.Name.Split(".txt")[0];
                if (dictionary.ContainsKey(actualFile))
                {
                    dictionary[actualFile]++;
                }
                else
                {
                    dictionary[actualFile] = 1;
                }
            }

            foreach (var file in dictionary)
            {
                if (file.Value > 1)
                {
                    Console.WriteLine($"File with name {file.Key} was exported {file.Value} times");
                }
            }
        }
    }
}