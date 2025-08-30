using System.Security.Cryptography;
using System.Text;

namespace FolderSync
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string pathSourceFolder = args[0];
            string pathReplicaFolder = args[1];
            string pathLogFolder = args[2];
            TimeSpan syncInterval = TimeSpan.Parse(args[3]);
            
            Console.WriteLine($"This program synchronize folder every {syncInterval} (HH:MM:SS).");

            using PeriodicTimer timer = new PeriodicTimer(syncInterval);

            do
            {
                var sourceInfo = GetFilesInfo(pathSourceFolder);
                var replicaInfo = GetFilesInfo(pathReplicaFolder);
                            
                CopyNewFiles(FileNameParse(sourceInfo), FileNameParse(replicaInfo), pathSourceFolder, pathReplicaFolder, pathLogFolder);
                DeleteOldFiles(FileNameParse(sourceInfo), FileNameParse(replicaInfo), pathSourceFolder, pathReplicaFolder, pathLogFolder);
                UpdateFiles(FileNameParse(sourceInfo), FileNameParse(replicaInfo), pathSourceFolder, pathReplicaFolder, pathLogFolder);

            } while (await timer.WaitForNextTickAsync());
        }

        static Dictionary<string, string>? GetFilesInfo(string folderPath)
        {
            try
            {
                Dictionary<string, string> filesDir = new Dictionary<string, string>();
                var files = Directory.GetFiles(folderPath);
                for (int i = 0; i < files.Length; i++)
                {
                    filesDir.Add(files[i], GetMD5(files[i]));
                }
                
                return filesDir;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine($"Directory {folderPath} not found. {e.Message}");
                return null;
            }
        }

        static string GetMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var content = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(content);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte hashByte in hashBytes)
                    {
                        sb.Append(hashByte.ToString());
                    }
                    
                    return sb.ToString();
                }
            }
        }

        static Dictionary<string, string> FileNameParse(Dictionary<string, string> inputDictionary)
        {
            var outputDictionary = new Dictionary<string, string>();
            foreach (var element in inputDictionary)
            {
                string[] parts = element.Key.Split('/');
                outputDictionary.Add(parts[^1], element.Value);
            }

            return outputDictionary;
        }

        static void CopyNewFiles(Dictionary<string, string> source, Dictionary<string, string> replica,
            string sourcePath, string replicaPath, string logPath)
        {
            foreach (var element in source)
            {
                if (!replica.ContainsKey(element.Key))
                {
                    File.Copy(sourcePath + element.Key, replicaPath + element.Key);
                    string logText = $"Copied {element.Key} into the {replicaPath} folder.";
                    LogTextToFileAndConsole(logText, logPath);
                }
            }
        }
        
        static void DeleteOldFiles(Dictionary<string, string> source, Dictionary<string, string> replica, 
            string sourcePath, string replicaPath, string logPath)
        {
            foreach (var element in replica)
            {
                if (!source.ContainsKey(element.Key))
                {
                    File.Delete(replicaPath + element.Key);
                    string logText = $"Deleted {element.Key} from the {replicaPath} folder.";
                    LogTextToFileAndConsole(logText, logPath);
                }
            }
        }

        static void UpdateFiles(Dictionary<string, string> source, Dictionary<string, string> replica,
            string sourcePath, string replicaPath, string logPath)
        {
            foreach (var element in source)
            {
                if (replica.ContainsKey(element.Key) && element.Value != replica[element.Key])
                {
                    File.Replace(sourcePath + element.Key, replicaPath + element.Key, null);
                    string logText = $"Updated {element.Key} into the {replicaPath} folder.";
                    LogTextToFileAndConsole(logText, logPath);
                }
            }
        }

        static void LogTextToFileAndConsole(string text, string pathLog)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(pathLog, "log.txt"), true))
            {
                sw.WriteLine($"{DateTime.Now}  --  {text}");
            }
            Console.WriteLine(text);
        }
    }
}