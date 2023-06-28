﻿using System.Diagnostics;
using myoddweb.directorywatcher;
using myoddweb.directorywatcher.interfaces;

namespace AutoRar
{
    class Program
    {
        static public void Main(string[] args)
        {
            string _outputFolder = "Output";
            string _inputFolder = "Input";
            string _password = args[0];

            string _path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            _outputFolder = _path + "/" + _outputFolder;
            _inputFolder = _path + "/" + _inputFolder;

            if (!Path.Exists(_outputFolder) || !Path.Exists(_inputFolder))
            {
                Console.WriteLine("ERROR: No INPUT OR OUTPUT folder found.");
                return;
            }

            Watcher watcher = new();
            watcher.Add(new Request(_inputFolder, false));

            int rarIndex;

            string[] filePaths = Directory.GetFiles(_outputFolder, "*.rar",
                                         SearchOption.TopDirectoryOnly);

            if (filePaths.Length <= 0)
            {
                rarIndex = 0;
            }
            else
            {
                int[] indexArray = new int[filePaths.Length];

                for (int i = 0; i < filePaths.Length; i++)
                {
                    string fileName = filePaths[i];
                    string name = Path.GetFileName(fileName);
                    name = name.Substring(0, name.Length - 4);

                    indexArray[i] = int.Parse(name);
                }

                rarIndex = indexArray.Max() + 1;
            }

            // Add event handlers for all events you want to handle
            watcher.OnAddedAsync += async (s, e) =>
             OnChanged(s, e, _inputFolder, _outputFolder,
             _password, ref rarIndex);

            // Activate the watcher
            watcher.Start();

            Console.WriteLine("AutoRar running. Listening for new files.");
            while (true)
            {

            }
        }

        private static void OnChanged(IFileSystemEvent sender, CancellationToken e, string InputFolder, string OutputFolder, string _password, ref int RarIndex)
        {
            string compressedFileName = RarIndex + ".rar";
            FileInfo fileInfo = new(sender.FullName);

            //ignore rars and directories
            
            if (fileInfo.Extension == ".rar" || fileInfo.Extension == "")
            {
                return;
            }

            string newFileName = String.Concat(sender.Name.Where(c => !Char.IsWhiteSpace(c)));
            File.Move(InputFolder + "/" + sender.Name, InputFolder + "/" + newFileName);

            ProcessStartInfo startInfo = new("rar")
            {
                WorkingDirectory = InputFolder,
                Arguments = $"a {compressedFileName} {newFileName} -p{_password}"
            };

            Process process = Process.Start(startInfo);

            if (process == null)
            {
                return;
            }

            RarIndex++;

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => MoveFiles(InputFolder, OutputFolder, compressedFileName);

        }

        private static void MoveFiles(string InputFolder, string OutputFolder, string compressedFileName)
        {
            if (File.Exists(InputFolder + "/" + compressedFileName) && !File.Exists(OutputFolder + "/" + compressedFileName))
            {
                File.Move(InputFolder + "/" + compressedFileName, OutputFolder + "/" + compressedFileName);
            }
        }

    }
}