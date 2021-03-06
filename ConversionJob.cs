﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScoreConverter
{
    public class ConversionJob
    {
        private readonly string[] _desiredOutput = { "png", "spos", "mpos", "metajson" };
        private string _binPath { get; }
        private IConsole _console { get; }

        private string _jsonJobFilePath { get; set; }
        private string _workingDir { get; }
        private string _tempDir { get; }
        private string _destination { get; }
        private string _jobName { get; }
        private List<ConversionJobFile> _files { get; }

        public ConversionJob(string binPath, IEnumerable<string> files, string destination, IConsole console)
        {
            _binPath = binPath;
            _console = console;
            _jobName = $"job_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            _tempDir = Path.Combine(Path.GetTempPath(), "ScoreConverter");
            _workingDir = Path.Combine(_tempDir, _jobName);
            _jsonJobFilePath = Path.Combine(_workingDir, $"{_jobName}.json");

            _destination = destination;
            _files = new List<ConversionJobFile>();
            BuildFileList(files);            
        }

        private void BuildFileList(IEnumerable<string> files)
        {            
            foreach (var source in files)
            {
                if (File.Exists(source))
                    _files.Add(new ConversionJobFile()
                    {
                        Name = Path.GetFileName(source),
                        NameWithoutExtension = Path.GetFileNameWithoutExtension(source),
                        FullPath = Path.GetFullPath(source),
                        Tasks = new List<ConversionJobTask>()
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
            if (Directory.Exists(_workingDir))
                Directory.Delete(_workingDir, true);
            else
                Directory.CreateDirectory(_workingDir);
        }

        private void Clean()
        {
            if(Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
            _console.WriteLine("We cleaned everything, including previous failed jobs.");
        }

        private void CreateArchives()
        {
            _console.WriteLine("Your files will now be packed in zip by the elves, it's a fine art...");
            foreach (var file in _files)
            {
                string zipPath = Path.Combine(this._destination, Path.GetFileName(Path.ChangeExtension(file.FullPath, ".zip")));
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
            startInfo.Arguments = $"-j {_jsonJobFilePath}";
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
            foreach(var file in this._files)
            {
                foreach(var task in file.Tasks.SelectMany(t => t.Out))
                {
                    if (!File.Exists(task))
                        missingOutput += task + '\n';
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
            var tasks = _files.SelectMany(f => f.Tasks);
            var parts = _files.SelectMany(f => f.Parts);
            string jsonTasks = JsonConvert.SerializeObject(tasks);
            string jsonParts = JsonConvert.SerializeObject(parts);
            var result = String.Join(',', new [] {jsonTasks.Remove(jsonTasks.Length - 1), jsonParts.Remove(0, 1)});

             System.IO.File.WriteAllText(_jsonJobFilePath, result);
        }

        private void PrepareJsonJobFile()
        {
            _files.ForEach((f) => PrepareSingleFileForJob(f));
        }

        private void PrepareSingleFileForJob(ConversionJobFile file)
        {
            file.WorkingDir = Path.Combine(_workingDir, file.NameWithoutExtension);
            Directory.CreateDirectory(file.WorkingDir);

            var output = _desiredOutput.ToList().ConvertAll(o => Path.Combine(file.WorkingDir, Path.ChangeExtension(file.Name, o)));

            file.Tasks.Add(new ConversionJobTask()
            {
                In = file.FullPath,
                Out = output.ToArray()
            });

            var parts = _desiredOutput.ToList().ConvertAll(o => new string[] { $"{file.WorkingDir}\\{Path.GetFileNameWithoutExtension(file.Name)} (part for ", $").{o}" } ).ToArray();

            file.Parts.Add(new ConversionPartJobTask()
            {
                In = file.FullPath,
                Out = parts
            });
        }
    }
}
