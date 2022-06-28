using System;
using System.IO;
using System.Linq;
using System.Threading;
using Quartz;
using Serilog;
using Serilog.Core;

namespace ConcurentFileReader
{
    class Program
    {
        private static Guid instanceId = Guid.NewGuid();
        private static Logger logger;
        private static string inputFolder = @"D:\Projects\FileReader\InputFolder";
        private static string outputFolder = @"D:\Projects\FileReader\OutputFolder";
        private static DateTime previousFire = DateTime.Now.ToUniversalTime();
        private static DateTime thisFire = DateTime.Now.ToUniversalTime();
        private static int sleepPeriodSeconds = 60;
        private static string exportedFilesSuffix = "exported";
        private static DirectoryInfo inputDir;
        private static string expression = "0 0/2 * 1/1 * ? *";
        private static CronExpression cron = new CronExpression(expression);

        static void Main()
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateLogger();

            logger.Information("Starting the worker {0}", instanceId);
            logger.Information("Cron expression : {0}", expression);
            
            logger.Information("Input folder set to path {0}", inputFolder);
            if (!Directory.Exists(inputFolder))
            {
                logger.Fatal("Input folder not found. Terminating...");
                return;
            }
            logger.Information("Successfully found input folder");
            inputDir = new DirectoryInfo(inputFolder);
            
            logger.Information("Output folder set to path {0}", outputFolder);
            if (!Directory.Exists(outputFolder))
            {
                logger.Fatal("Output folder not found. Terminating...");
                return;
            }
            logger.Information("Successfully found output folder");
            
            DoWork();
            
            logger.Information("Finished process.");
        }

        static void DoWork()
        {
            while (true)
            {
                previousFire = thisFire.ToLocalTime();
                thisFire = cron.GetNextValidTimeAfter(previousFire)!.Value.DateTime.ToLocalTime();
                var difference = (thisFire - previousFire).TotalSeconds;
                logger.Information("Next fire at {0}", thisFire.ToLocalTime().ToString("g"));
                Thread.Sleep((int) difference * 1000);
                
                CheckFiles();
            }
        }

        static void CheckFiles()
        {
            logger.Information("-------- {0} --------", thisFire.ToLocalTime().ToString("G"));
            var files = inputDir.EnumerateFiles()
                .Where(x => previousFire <= x.CreationTime && x.CreationTime < thisFire && !x.Name.EndsWith(exportedFilesSuffix));
            int counter = 0;
            foreach (var file in files)
            {
                if (!file.Exists)
                    continue;
                try
                {
                    using (var fileStream =
                           new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        if (fileStream.Length == 0)
                        {
                            fileStream.Close();
                            file.Delete();
                            continue;
                        }

                        logger.Information("Working with file : {0}", file.FullName);

                        var bytes = new byte[fileStream.Length];

                        var lenght = (int)fileStream.Length;
                        int count;
                        int sum = 0;
                        while ((count = fileStream.Read(bytes, sum, lenght - sum)) > 0)
                            sum += count;

                        var outputFileStream = new FileStream($@"{outputFolder}\{file.Name}-{instanceId}",
                            FileMode.OpenOrCreate, FileAccess.Write);

                        outputFileStream.Write(bytes);
                        outputFileStream.Flush();
                        outputFileStream.Close();

                        outputFileStream = new FileStream($@"{inputFolder}\{file.Name}-{exportedFilesSuffix}",
                            FileMode.CreateNew, FileAccess.Write);
                        outputFileStream.Write(bytes);
                        outputFileStream.Flush();
                        outputFileStream.Close();

                        fileStream.SetLength(0);
                        fileStream.Close();
                        file.Delete();
                        counter++;
                    }
                }
                catch (Exception e)
                { }
            }
            
            logger.Information("--- This iteration counter : {0}", counter);
        }
    }
}