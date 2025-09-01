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
            
            if (!Directory.Exists(pathLogFolder))
            {
                throw new ArgumentException($"Directory {pathLogFolder} not found.");
            }
            
            TimeSpan syncInterval = TimeSpan.Parse(args[3]);
            
            Console.WriteLine($"This program synchronize folder every {syncInterval} (HH:MM:SS).");

            using PeriodicTimer timer = new PeriodicTimer(syncInterval);

            do
            {
                DeleteOldFolders(pathSourceFolder, pathReplicaFolder, pathLogFolder);
                
                var sourceFilesInfo = GetFilesInfo(pathSourceFolder);
                var replicaFilesInfo = GetFilesInfo(pathReplicaFolder);
                
                CopyNewFiles(sourceFilesInfo, replicaFilesInfo, pathSourceFolder, pathReplicaFolder, pathLogFolder);
                DeleteOldFiles(sourceFilesInfo, replicaFilesInfo, pathSourceFolder, pathReplicaFolder, pathLogFolder);
                UpdateFiles(sourceFilesInfo, replicaFilesInfo, pathSourceFolder, pathReplicaFolder, pathLogFolder);

            } while (await timer.WaitForNextTickAsync());
        }

        static Dictionary<string, string>? GetFilesInfo(string folderPath)
        {
            try
            {
                Dictionary<string, string> filesDict = new Dictionary<string, string>();
                var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    filesDict.Add(Path.GetRelativePath(folderPath, file), GetMD5(file));
                }
                
                return filesDict;
            }
            catch (DirectoryNotFoundException e)
            {
                throw new ArgumentException($"Directory {folderPath} not found.", e);
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

        static void CopyNewFiles(Dictionary<string, string> source, Dictionary<string, string> replica,
            string sourcePath, string replicaPath, string logPath)
        {
            foreach (var element in source)
            {
                if (!replica.ContainsKey(element.Key))
                {
                    string replicaAbsolutePath = Path.Combine(replicaPath, element.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(replicaAbsolutePath));
                    File.Copy(Path.Combine(sourcePath, element.Key), replicaAbsolutePath);
                    string logText = $"Copied {element.Key} into the {replicaAbsolutePath} folder.";
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
                    File.Delete(Path.Combine(replicaPath, element.Key));
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
                    File.Replace(Path.Combine(sourcePath, element.Key), Path.Combine(replicaPath, element.Key) , null);
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
            Console.WriteLine($"{DateTime.Now}  --  {text}");
        }

        static void DeleteOldFolders(string sourcePath, string replicaPath, string logPath)
        {
            var sourceDirectories = Directory.GetDirectories(sourcePath, "*",  SearchOption.AllDirectories)
                .Select(x => Path.GetRelativePath(sourcePath, x));
            var replicaDirectories = Directory.GetDirectories(replicaPath, "*",  SearchOption.AllDirectories)
                .Select(x => Path.GetRelativePath(replicaPath, x));
            foreach (var relativeFolderPath in replicaDirectories.OrderByDescending(y => y.Split(Path.DirectorySeparatorChar).Length))
            {
                if (!sourceDirectories.Contains(relativeFolderPath))
                {
                    Directory.Delete(Path.Combine(replicaPath, relativeFolderPath), true);
                    string logText = $"Deleted folder {Path.Combine(replicaPath, relativeFolderPath)}";
                    LogTextToFileAndConsole(logText, logPath);
                }
            }
        }
    }
}