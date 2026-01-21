# Folder Synchronization

## 📌 Task Description
This project is a C# console application that synchronizes two folders: **source** and **replica**.  
The program ensures that the replica folder is always an identical copy of the source folder.  

### Requirements:
- **One-way synchronization** → replica should always match source exactly.  
- **Periodic synchronization** → runs at a given time interval.  
- **Logging** → all file operations (creation, copying, removal) are logged to:  
  - A log file.  
  - Console output.  
- **Configuration via command-line arguments**:  
  - `pathSourceFolder` → path to the source folder.  
  - `pathReplicaFolder` → path to the replica folder.  
  - `pathLogFolder` → path where the log file should be stored.  
  - `syncInterval` → synchronization interval in format HH:MM:SS.  

External libraries that implement general-purpose algorithms (e.g., hashing like MD5) may be used.  
However, third-party libraries for folder synchronization **must not** be used.  

---

## 🚀 Usage

### 1. Build the project
```bash
dotnet build
```

### 2. Run the program
```bash
dotnet run -- <pathSourceFolder> <pathReplicaFolder> <pathLogFolder> <syncInterval>
```

### Example
```bash
dotnet run -- /Users/name/Downloads/sync_test/source  /Users/name/Downloads/sync_test/replica  /Users/name/Downloads/sync_test/  0:30:00
```

This will:
- Synchronize `source` → `replica`.  
- Repeat every **30 minutes**.  
- Write logs both to the console and to `log.txt`.  

---

## 🛠 Implementation Notes
- **Language**: C# (.NET 6 or higher recommended).  
- **Logging**: implemented with `StreamWriter` for file logging + console output.  
- **File comparison**: based on MD5 checksums to detect changes.  
- **Periodic execution**: implemented with `PeriodicTimer`.  
