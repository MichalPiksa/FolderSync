namespace FolderSync
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 4)
            {
                throw new ArgumentException("Invalid number of arguments. " +
                                            "Expected arguments: <source folder path> <replica folder path> <log folder path> <synchronization interval (HH:MM:SS)>");
            }
            
            if (!TimeSpan.TryParse(args[3], out var syncInterval) || syncInterval <= TimeSpan.Zero)
            {
                throw new ArgumentException("Invalid synchronization interval.");
            }
            string pathLogFolder = args[2];
            
            var logger = new Logger(pathLogFolder);
            
            string pathSourceFolder = args[0];
            string pathReplicaFolder = args[1];
            
            
            if (!Directory.Exists(pathLogFolder))
            {
                throw new ArgumentException($"Directory {pathLogFolder} not found.");
            }
            
            logger.Log("Folder synchronization started.");
            
            using PeriodicTimer timer = new PeriodicTimer(syncInterval);
            
            var syncService = new SyncService(pathSourceFolder, pathReplicaFolder, logger);
            
            do
            {
                syncService.Synchronize();

            } while (await timer.WaitForNextTickAsync());
        }
    }
}