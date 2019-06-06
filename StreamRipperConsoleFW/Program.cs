using CommandLine;
using StreamRipperConsoleFW.CLI;
using StromReisser3000.Domain;
using StromReisser3000.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace StreamRipperConsole
{
    class Program
    {
        private static IStromReisser myRipper;
        private static IStromAufzeichner myRecorder;
        private static StreamSource myStreamSource;
        private static Options options;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => options = opts)
                .WithNotParsed<Options>(errs => System.Environment.Exit(-1));

            myStreamSource = new StreamSource
            {
                DisplayName = "Radio Energy Nuremberg",
                FilePrefix = "NRJ_Nuremberg",
                StreamUrl = "http://energyradio.de/nuernberg"
            };

            myRipper = new Mp3StromReisser();
            myRipper.StartRip(myStreamSource);

            var now = DateTime.Now;
            var filePrefix = "Energy_Mastermix";
            var fileName = $"{filePrefix}_{now.Day:00}-{now.Month:00}-{now.Year}_{now.Hour:00}-{now.Minute:00}-{now.Second:00}.mp3";
            var fileLocation = Path.Combine(@"P:\tmp", fileName);

            myRecorder = new Mp3StromAufzeichner((Mp3StromReisser)myRipper, fileLocation);
            myRecorder.StartRecord();

            var sw = new Stopwatch();
            sw.Start();
            do
            {
                Thread.Sleep(1000);
            }
            while (sw.Elapsed.TotalMinutes < options.Duration);

            myRecorder.StopRecord();
            myRipper.StopRip();

            Console.WriteLine("End World!");
        }
    }
}
