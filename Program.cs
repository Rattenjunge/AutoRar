using System.Diagnostics;
using FileWatcherEx;

namespace AutoRar
{
    class Program
    {
        static private string currentFile;
        static public void Main(string[] args)
        {
            string outputFolder = "Output";
            string inputFolder = "Input";
            string password = args[0];

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            outputFolder = path + "/" + outputFolder;
            inputFolder = path + "/" + inputFolder;

            if (!Path.Exists(outputFolder) || !Path.Exists(inputFolder))
            {
                Console.WriteLine("ERROR: No INPUT OR OUTPUT folder found.");
                return;
            }

             FileSystemWatcherEx watcher = new(inputFolder, (s) => Console.WriteLine(s));
            
            
            int rarIndex;

            string[] filePaths = Directory.GetFiles(outputFolder, "*.rar",
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
            watcher.OnCreated += new((s, e) =>
             OnChanged(e, inputFolder, outputFolder,
             password, ref rarIndex));

            // Activate the watcher
            watcher.Start();

            Console.WriteLine("AutoRar running. Listening for new files.");
            while (true)
            {

            }
        }

        private static void OnChanged(FileChangedEvent e, string InputFolder, string OutputFolder, string _password, ref int RarIndex)
        {
            string compressedFileName = RarIndex + ".rar";
            FileInfo fileInfo = new(e.FullPath);

            //ignore rars and directories

            if (fileInfo.Extension == ".rar" || fileInfo.Extension == "")
            {
                return;
            }
           
            string newFileName = String.Concat(fileInfo.Name.Where(c => !Char.IsWhiteSpace(c)));

            if(currentFile == newFileName)
            {
                return;
            }

            currentFile = newFileName;

            File.Move(InputFolder + "/" + fileInfo.Name, InputFolder + "/" + newFileName);

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