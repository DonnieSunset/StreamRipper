using CommandLine;
using CommandLine.Text;

namespace StreamRipperConsoleFW.CLI
{
    internal class Options
    {
        [Option('d', "duration", Required = true, HelpText = "Time in minutes to keep recording.")]
        public int Duration { get; set; }
    }
}
