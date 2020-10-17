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
                       ConversionJob job = new ConversionJob()
                       {
                           Files = new List<ConversionJobFile>()
                       };

                       foreach(var source in o.Sources)
                       {
                           if (File.Exists(source))
                               job.Files.Add(new ConversionJobFile()
                               {
                                   Name = Path.GetFileName(source),
                                   NameWithoutExtension = Path.GetFileNameWithoutExtension(source),
                                   FullPath = Path.GetFullPath(source),
                                   Task = new List<ConversionJobTask>()
                               });
                       }

                       job.PrepareJsonJobFile();
                       job.WriteJsonJob();
                       job.ExecuteJob(o.MusescorePath);
                       job.CreateArchives();
                       job.Clean();

                       Console.ReadKey();
                   });
        }
    }
}
