using CommandLine;
using StreamRipperConsoleFW.CLI;
using StreamRipper.Domain;
using StreamRipper.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace StreamRipperConsole
{
    class Program
    {
        private static IStreamRipper myRipper;
        private static IStreamRecorder myRecorder;
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

            myRipper = new Mp3StreamRipper();
            myRipper.StartRip(myStreamSource);

            var fileLocation = GetFileLocation();
            Console.WriteLine($"Writing to {fileLocation}");

            myRecorder = new Mp3StreamRecorder((Mp3StreamRipper)myRipper, fileLocation);
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

        private static string GetFileLocation()
        {
            var now = DateTime.Now;
            var filePrefix = "Energy_Mastermix";
            var pathPrefix = @"P:\Downloads\Mastermix";
            var dateStr = $"{now.Year}-{now.Month:00}-{now.Day:00}";
            var timeStr = $"{now.Hour:00}-{now.Minute:00}-{now.Second:00}";
            var fileName = $"{filePrefix}_{dateStr}_{timeStr}.mp3";
            var directoryLocation = Path.Combine(pathPrefix, dateStr);
            var fileLocation = Path.Combine(directoryLocation, fileName);

            if (!Directory.Exists(directoryLocation))
                Directory.CreateDirectory(directoryLocation);

            return fileLocation;
        }

    }
}
