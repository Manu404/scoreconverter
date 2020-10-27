using CommandLine;
using System.Collections.Generic;

namespace ScoreConverter
{
    public class Options
    {
        [Option('s', "source", Required = true, HelpText = "Path to your scores.")]
        public IEnumerable<string> Sources { get; set; }

        [Option('d', "destination", Required = false, HelpText = "Path to put the export result in.", Default = "C:\\export")]
        public string Destination { get; set; }

        [Option('m', "ms", Required = false, HelpText = "Path to your musescore executable.", Default = "C:\\Program Files\\MuseScore 3\\bin\\MuseScore3.exe")]
        public string MusescorePath { get; set; }

        [Option('i', "interactive", Required = true, HelpText = "Ask user confirmations.")]
        public bool Interactive { get; set; }

        [Option("silent", Required = false, HelpText = "No console ouput.")]
        public bool Silent { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
}
