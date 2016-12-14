using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// watches specified folder changes in the filesystem
// "Listens to the file system change notifications and raises events when a directory, or file in a directory, changes."
// references: http://stackoverflow.com/questions/15017506/using-filesystemwatcher-to-monitor-a-directory and http://www.c-sharpcorner.com/article/monitoring-file-system-using-filesystemwatcher-class-part1/

public class FileSystemWatch : MonoBehaviour
{
    string myPath = "c:\\myfolder\\";
    FileSystemWatcher watcher;

    void Start()
    {
        InitFileSystemWatcher();
    }

    private void InitFileSystemWatcher()
    {
        watcher = new FileSystemWatcher();
        watcher.Path = myPath;
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.Filter = "*.*";

        //Handler for Changed Event
        watcher.Changed += new FileSystemEventHandler(FileChanged);
        //Handler for Created Event
        watcher.Created += new FileSystemEventHandler(FileCreated);
        //Handler for Deleted Event
        watcher.Deleted += new FileSystemEventHandler(FileDeleted);
        //Handler for Renamed Event
        watcher.Renamed += new RenamedEventHandler(FileRenamed);

        watcher.EnableRaisingEvents = true;
    }

    private void FileChanged(object source, FileSystemEventArgs e)
    {
        Debug.Log("FileChanged:" + e.FullPath);
    }

    private void FileCreated(object source, FileSystemEventArgs e)
    {
        Debug.Log("FileCreated:" + e.FullPath);
    }

    private void FileDeleted(object source, FileSystemEventArgs e)
    {
        Debug.Log("FileDeleted:" + e.FullPath);
    }

    private void FileRenamed(object source, FileSystemEventArgs e)
    {
        Debug.Log("FileChanged:" + e.FullPath);
    }
}
