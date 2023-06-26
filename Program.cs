using System;
using System.Diagnostics;

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

            FileSystemWatcher watcher = new FileSystemWatcher()
            {
                Path = _inputFolder
            };

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
            watcher.Created += new FileSystemEventHandler((s, e) =>
             OnChanged(s, e, _inputFolder, _outputFolder,
             _password, ref rarIndex));

            // Activate the watcher
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("AutoRar running. Listening for new files.");
           while(true)
           {
            
           }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e, string InputFolder, string OutputFolder,string _password, ref int RarIndex)
        {
            string compressedFileName = RarIndex + ".rar";
            FileInfo fileInfo = new(e.FullPath);

            if (fileInfo.Extension == ".rar")
            {
                return;
            }

            ProcessStartInfo startInfo = new("rar")
            {
                WorkingDirectory = InputFolder,
                Arguments = $"a {compressedFileName} {e.Name} -p{_password}"
            };

            Process process = Process.Start(startInfo);

            if (process == null)
            {
                return;
            }

            RarIndex++;

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => MoveFiles(s, e, InputFolder, OutputFolder, compressedFileName);

        }

        private static void MoveFiles(Object sender, EventArgs e, string InputFolder, string OutputFolder, string compressedFileName)
        {
            if (File.Exists(InputFolder + "/" + compressedFileName) && !File.Exists(OutputFolder + "/" + compressedFileName))
            {
                File.Move(InputFolder + "/" + compressedFileName, OutputFolder + "/" + compressedFileName);
            }
        }
    }
}