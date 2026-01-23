namespace FolderSync;

public class SyncService
{
    private readonly string _sourcePath;
    private readonly string _replicaPath;
    private readonly Logger _logger;

    public SyncService(string sourcePath, string replicaPath, Logger logger)
    {
        _sourcePath = sourcePath;
        _replicaPath = replicaPath;
        _logger = logger;
    }
    
    private void CreateMissingDirectories()
    {
        var sourceDir = Directory.GetDirectories(_sourcePath, "*",  SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(_sourcePath, x)).ToHashSet();

        foreach (var dir in sourceDir)
        {
            var replicaDir = Path.Combine(_replicaPath, dir);
            if (!Directory.Exists(replicaDir))
            {
                Directory.CreateDirectory(replicaDir);
                _logger.Log($"Created missing directory: {replicaDir}");
            }
        }
    }
    
    private void SyncFiles()
    {
        var sourceFiles = Directory.GetFiles(_sourcePath, "*", SearchOption.AllDirectories);

        foreach (var sourceFile in sourceFiles)
        {
            var relativePath = Path.GetRelativePath(_sourcePath, sourceFile);
            var replicaFile = Path.Combine(_replicaPath, relativePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(replicaFile)!);
            
            if (!File.Exists(replicaFile))
            {
                File.Copy(sourceFile, replicaFile);
                _logger.Log($"Copied file: {relativePath} to replica.");
            }
            else if (!FileComparer.AreFilesIdentical(sourceFile, replicaFile))
            {
                File.Copy(sourceFile, replicaFile, overwrite: true);
                _logger.Log($"Updated file: {relativePath} in replica.");
            }
            
            if (!File.Exists(replicaFile))
            {
                var replicaDir = Path.GetDirectoryName(replicaFile);
                if (!Directory.Exists(replicaDir))
                {
                    if (replicaDir != null)
                    {
                        Directory.CreateDirectory(replicaDir);
                    }
                }

                File.Copy(sourceFile, replicaFile);
                _logger.Log($"Copied file: {relativePath} to replica.");
            }
        }
    }
    
    private void DeleteObsoleteFiles()
    {
        var sourceFiles = Directory.GetFiles(_sourcePath, "*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(_sourcePath, x)).ToHashSet();
        var replicaFiles = Directory.GetFiles(_replicaPath, "*", SearchOption.AllDirectories);

        foreach (var replicaFile in replicaFiles)
        {
            var relativePath = Path.GetRelativePath(_replicaPath, replicaFile);

            if (!sourceFiles.Contains(relativePath))
            {
                File.Delete(replicaFile);
                _logger.Log($"Deleted obsolete file: {relativePath} from replica.");
            }
        }
    }
    
    private void DeleteObsoleteDirectories()
    {
        var sourceDirs = Directory.GetDirectories(_sourcePath, "*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(_sourcePath, x)).ToHashSet();
        var replicaDirs = Directory.GetDirectories(_replicaPath, "*", SearchOption.AllDirectories)
            .OrderByDescending(y => y.Length);

        foreach (var dir in replicaDirs)
        {
            var relativePath = Path.GetRelativePath(_replicaPath, dir);
            if (!sourceDirs.Contains(relativePath))
            {
                Directory.Delete(dir, recursive: true);
                _logger.Log($"Deleted obsolete directory: {relativePath} from replica.");
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