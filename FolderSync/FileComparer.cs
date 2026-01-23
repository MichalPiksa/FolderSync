namespace FolderSync;

internal static class FileComparer
{
    public static bool AreFilesIdentical(string sourceFile, string replicaFile)
    {
        var sourceInfo = new FileInfo(sourceFile);
        var replicaInfo = new FileInfo(replicaFile);
        
        if (sourceInfo.Length != replicaInfo.Length)
        {
            return false;
        }
        
        if (sourceInfo.LastWriteTimeUtc != replicaInfo.LastWriteTimeUtc)
        {
            return false;
        }
        
        return true;
    }
}