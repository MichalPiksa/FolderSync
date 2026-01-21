namespace FolderSync;

public class Logger
{
    private readonly StreamWriter LogWriter;
    
    public Logger(string path)
    {
        LogWriter = new StreamWriter(Path.Combine(path, "log.txt"), true)
        {
            AutoFlush = true
        };
    }
    
    public void Log(string message)
    {
        string logEntry = $"{DateTime.Now}  --  {message}";
        LogWriter.WriteLine(logEntry);
        Console.WriteLine(logEntry);
    }
}