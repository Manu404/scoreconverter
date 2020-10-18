﻿using Newtonsoft.Json;
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
        private string _binPath;
        private string[] _desiredOutput = { "png", "spos", "mpos", "svg", "ogg", "metajson" };
        private IConsole _console;

        public string JobFilePath { get; set; }
        public string WorkingDir { get; set; }
        public string Destination { get; set; }
        public List<ConversionJobFile> Files { get; set; }

        public ConversionJob(string binPath, IEnumerable<string> files, string destination, IConsole console)
        {
            _binPath = binPath;
            _console = console;
            Destination = destination;
            BuildFileList(files);            
        }

        private void BuildFileList(IEnumerable<string> files)
        {
            Files = new List<ConversionJobFile>();
            foreach (var source in files)
            {
                if (File.Exists(source))
                    Files.Add(new ConversionJobFile()
                    {
                        Name = Path.GetFileName(source),
                        NameWithoutExtension = Path.GetFileNameWithoutExtension(source),
                        FullPath = Path.GetFullPath(source),
                        Task = new List<ConversionJobTask>()
                    });
            }
        }
        public void Do()
        {
            CreateTempWorkingDir();
            PrepareJsonJobFile();
            WriteJsonJob();
            if (ExecuteJob() && EnsureJobDone())
            {
                CreateArchives();
            }
            Clean();
        }

        private void CreateTempWorkingDir()
        {
            WorkingDir = Path.Combine(Path.GetTempPath(), $"job_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            if (Directory.Exists(WorkingDir))
                Directory.Delete(WorkingDir, true);
            else
                Directory.CreateDirectory(WorkingDir);
        }

        private void Clean()
        {
            if(File.Exists(JobFilePath))
                File.Delete(JobFilePath);
            foreach (var file in Files)
                if(File.Exists(file.WorkingDir))
                Directory.Delete(file.WorkingDir, true);
            _console.WriteLine("We cleaned everything.");
        }

        private void CreateArchives()
        {
            _console.WriteLine("Your files will now be packed in zip by the elves, it's a fine art...");
            foreach (var file in Files)
            {
                string zipPath = Path.Combine(this.Destination, Path.GetFileName(Path.ChangeExtension(file.FullPath, ".zip")));
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(file.WorkingDir, zipPath);
            }
            _console.WriteLine("They did it !");
        }

        private bool ExecuteJob()
        {
            if(!File.Exists(_binPath))
            {
                _console.WriteLine("We couldn't find the goblin team :( Make sure Musescore is installed and/org you provided a valid path to the binary file");
                return false;
            }
            ProcessStartInfo startInfo = new ProcessStartInfo(_binPath);
            startInfo.Arguments = $"-j {JobFilePath}";
            var p = Process.Start(startInfo);
            _console.WriteLine("Your files had been sent to the goblins team, they're doing their best...");
            int i = 0;
            while (!p.HasExited)
            {
                System.Threading.Thread.Sleep(1000); // fine, as waiting for external process
                _console.Write(".");
                i++;
            }
            _console.WriteLine($"\n{i} goblins died...");
            return true;
        }

        private bool EnsureJobDone()
        {
            string missingOutput = String.Empty;
            foreach(var file in this.Files)
            {
                foreach(var task in file.Task)
                {
                    if (!File.Exists(task.Out))
                        missingOutput += task.Out + '\n';
                }
            }
            if(String.IsNullOrEmpty(missingOutput))
            {
                _console.WriteLine("The following files couldn't be generated by the goblins team: ");
                _console.WriteLine(missingOutput);
                _console.WriteLine("Please fix your job request.");
                return false;
            }
            return true;
        }


        private void WriteJsonJob()
        {
            var tasks = Files.SelectMany(f => f.Task);
            var jsonFileName = $"{Path.GetDirectoryName(WorkingDir)}.json";
            string json = JsonConvert.SerializeObject(tasks);
            System.IO.File.WriteAllText(jsonFileName, json);
            JobFilePath = Path.GetFullPath(jsonFileName);
        }

        private void PrepareJsonJobFile()
        {
            Files.ForEach((f) => PrepareSingleFileForJob(f));
        }

        private void PrepareSingleFileForJob(ConversionJobFile file)
        {
            file.WorkingDir = Path.Combine(Path.GetDirectoryName(file.FullPath), file.NameWithoutExtension);
            Directory.CreateDirectory(file.WorkingDir);

            foreach (var extension in _desiredOutput)
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
