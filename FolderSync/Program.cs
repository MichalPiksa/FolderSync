using System;
using System.Security.Cryptography;
using System.Text;

namespace FolderSync
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathSourceFolder = args[0];
            string pathReplicaFolder = args[1];
            string pathLogFolder = args[2];
            TimeSpan interval = TimeSpan.Parse(args[3]);
            
            Console.WriteLine($"Sync every {interval}");

            var sourceInfo = GetFilesInfo(pathSourceFolder);
            var replicaInfo = GetFilesInfo(pathReplicaFolder);
            CopyNewFiles(FileNameParse(sourceInfo), FileNameParse(replicaInfo), pathSourceFolder, pathReplicaFolder);

            Console.ReadKey();
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
                outputDictionary.Add(parts[parts.Length - 1], element.Value);
            }

            return outputDictionary;
        }

        static void CopyNewFiles(Dictionary<string, string> source, Dictionary<string, string> replica, string sourcePath, string replicaPath)
        {
            foreach (var element in source)
            {
                if (!replica.ContainsKey(element.Key))
                {
                    File.Copy(sourcePath + element.Key, replicaPath + element.Key);
                    Console.WriteLine($"Copied {element} to {replicaPath} folder.");
                }
            }
        }
    }
}