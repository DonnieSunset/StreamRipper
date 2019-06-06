using StromReisser3000.Domain;
using StromReisser3000.Interfaces;
using System;
using System.IO;
using System.Threading;

namespace StreamRipperConsole
{
    class Program
    {
        private static IStromReisser myRipper;
        private static IStromAufzeichner myRecorder;
        private static StreamSource myStreamSource;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            myStreamSource = new StreamSource
            {
                DisplayName = "Radio Energy Nuremberg",
                FilePrefix = "NRJ_Nuremberg",
                StreamUrl = "http://energyradio.de/nuernberg"
            };

            myRipper = new Mp3StromReisser();
            myRipper.StartRip(myStreamSource);

            var filePath = Path.Combine(@"P:\tmp", "bla.mp3");
            myRecorder = new Mp3StromAufzeichner((Mp3StromReisser)myRipper, filePath);
            myRecorder.StartRecord();

            Thread.Sleep(5000);

            myRecorder.StopRecord();
            myRipper.StopRip();

            Console.WriteLine("End World!");
        }
    }
}
