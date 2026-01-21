namespace FolderSync;

public class SyncService
{
    private readonly string SourcePath;
    private readonly string ReplicaPath;
    private readonly Logger Logger;

    public SyncService(string sourcePath, string replicaPath, Logger logger)
    {
        SourcePath = sourcePath;
        ReplicaPath = replicaPath;
        Logger = logger;
    }
    
    private void CreateMissingDirectories()
    {
        var sourceDir = Directory.GetDirectories(SourcePath, "*",  SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(SourcePath, x)).ToHashSet();

        foreach (var dir in sourceDir)
        {
            var replicaDir = Path.Combine(ReplicaPath, dir);
            if (!Directory.Exists(replicaDir))
            {
                Directory.CreateDirectory(replicaDir);
                Logger.Log($"Created missing directory: {replicaDir}");
            }
        }
    }
    
    private void SyncFiles()
    {
        var sourceFiles = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);

        foreach (var sourceFile in sourceFiles)
        {
            var relativePath = Path.GetRelativePath(SourcePath, sourceFile);
            var replicaFile = Path.Combine(ReplicaPath, relativePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(replicaFile)!);
            
            if (!File.Exists(replicaFile))
            {
                File.Copy(sourceFile, replicaFile);
                Logger.Log($"Copied file: {relativePath} to replica.");
            }
            else if (!FileComparer.AreFilesIdentical(sourceFile, replicaFile))
            {
                File.Copy(sourceFile, replicaFile, overwrite: true);
                Logger.Log($"Updated file: {relativePath} in replica.");
            }
            
            if (!File.Exists(replicaFile))
            {
                var replicaDir = Path.GetDirectoryName(replicaFile);
                if (!Directory.Exists(replicaDir))
                {
                    Directory.CreateDirectory(replicaDir);
                }

                File.Copy(sourceFile, replicaFile);
                Logger.Log($"Copied file: {relativePath} to replica.");
            }
        }
    }
    
    private void DeleteObsoleteFiles()
    {
        var sourceFiles = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(SourcePath, x)).ToHashSet();
        var replicaFiles = Directory.GetFiles(ReplicaPath, "*", SearchOption.AllDirectories);

        foreach (var replicaFile in replicaFiles)
        {
            var relativePath = Path.GetRelativePath(ReplicaPath, replicaFile);

            if (!sourceFiles.Contains(relativePath))
            {
                File.Delete(replicaFile);
                Logger.Log($"Deleted obsolete file: {relativePath} from replica.");
            }
        }
    }
    
    private void DeleteObsoleteDirectories()
    {
        var sourceDirs = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(SourcePath, x)).ToHashSet();
        var replicaDirs = Directory.GetDirectories(ReplicaPath, "*", SearchOption.AllDirectories)
            .OrderByDescending(y => y.Length);

        foreach (var dir in replicaDirs)
        {
            var relativePath = Path.GetRelativePath(ReplicaPath, dir);
            if (!sourceDirs.Contains(relativePath))
            {
                Directory.Delete(dir, recursive: true);
                Logger.Log($"Deleted obsolete directory: {relativePath} from replica.");
            }
        }
    }
    
    public void Synchronize()
    {
        CreateMissingDirectories();
        SyncFiles();
        DeleteObsoleteFiles();
        DeleteObsoleteDirectories();
    }
}