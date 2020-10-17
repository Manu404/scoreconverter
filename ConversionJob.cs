using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ScoreConverter
{
    public class ConversionJob
    {
        private string[] desiredOutput = { ".png", ".spos", ".mpos", ".svg", ".ogg", "metajson" };

        public string JobFilePath { get; set; }
        public List<ConversionJobFile> Files { get; set; }

        public void Clean()
        {
            File.Delete(JobFilePath);
            foreach (var file in Files)
                Directory.Delete(file.WorkingDir, true);
            Console.WriteLine("We cleaned the... goblin thing.");
        }

        public void CreateArchives()
        {
            Console.WriteLine("Your files will now be packed in zip by the elves, it's a fine art...");
            foreach (var file in Files)
            {
                string zipPath = Path.ChangeExtension(file.FullPath, ".zip");
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(file.WorkingDir, zipPath);
            }
            Console.WriteLine("They did it !");
        }

        public void ExecuteJob(string binPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(binPath);
            startInfo.Arguments = $"-j {JobFilePath}";
            var p = Process.Start(startInfo);
            Console.WriteLine("Your files had been sent to the goblins team, they're doing their best...");
            int i = 0;
            while (!p.HasExited)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write(".");
                i++;
            }
            Console.WriteLine($"\n{i} goblins died...");
        }

        public void WriteJsonJob()
        {
            var tasks = Files.SelectMany(f => f.Task);
            var jsonFileName = $"job_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.json";
            string json = JsonConvert.SerializeObject(tasks);
            System.IO.File.WriteAllText(jsonFileName, json);
            JobFilePath = Path.GetFullPath(jsonFileName);
        }

        public void PrepareJsonJobFile()
        {
            Files.ForEach((f) => PrepareSingleFileForJob(f));
        }

        public void PrepareSingleFileForJob(ConversionJobFile file)
        {
            file.WorkingDir = Path.Combine(Path.GetDirectoryName(file.FullPath), file.NameWithoutExtension);
            Directory.CreateDirectory(file.WorkingDir);

            foreach (var extension in desiredOutput)
            {
                var output = Path.Combine(file.WorkingDir, Path.ChangeExtension(file.Name, extension));
                file.Task.Add(new ConversionJobTask()
                {
                    In = file.FullPath,
                    Out = output
                });                
            }
        }
    }
}
