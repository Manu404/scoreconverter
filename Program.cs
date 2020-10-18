using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScoreConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       IConsole console = new ConsoleWrapper(o.Silent);
                       try
                       {
                           ConversionJob job = new ConversionJob(o.MusescorePath, o.Sources, o.Destination);
                           job.Do();
                       }
                       catch(Exception e)
                       {
                           Console.WriteLine(e.Message);
                       }
                   });
        }
    }
}
